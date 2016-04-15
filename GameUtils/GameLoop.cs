using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GameUtils.Graphics;
using GameUtils.Logging;

namespace GameUtils
{
    /// <summary>
    /// The GameLoop is a singletone game component and serves for continous updating and rendering as well as managing all game objects.
    /// </summary>
    public sealed class GameLoop : RegistrationTargetBase, IEngineComponent
    {
        readonly int desiredUpdatesPerSecond;

        Thread updateThread;
        Thread renderThread;

        readonly Stopwatch gameTime;
        volatile int currentBufferIndex;
        readonly TimeSpan[] bufferUpdateTimes;

        readonly AutoResetEvent resourceUpdateHandle;

        readonly List<float> updateTimes;
        volatile float ups;
        readonly List<float> frameTimes;
        volatile float fps;

        internal event EventHandler ResourceUpdateRequested;

        /// <summary>
        /// The color the surface will be cleared with at the beginning of a render cycle.
        /// </summary>
        public Color4 BackColor { get; set; }

        /// <summary>
        /// Indicates whether the loop is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// The current update rate.
        /// </summary>
        public float UpdatesPerSecond => ups;

        /// <summary>
        /// The current frame rate.
        /// </summary>
        public float FramesPerSecond => fps;

        protected override int CurrentBufferIndex => currentBufferIndex;

        bool IEngineComponent.IsCompatibleTo(IEngineComponent component)
        {
            return !(component is GameLoop);
        }

        object IEngineComponent.Tag => null;

        /// <summary>
        /// Creates a new game loop.
        /// </summary>
        public GameLoop(int desiredUpdatesPerSecond, bool enableDepthBuffering = false)
        {
            this.desiredUpdatesPerSecond = desiredUpdatesPerSecond;

            gameTime = new Stopwatch();
            bufferUpdateTimes = new TimeSpan[BufferCount];

            this.IsRunning = false;
            EnableDepthBuffering = enableDepthBuffering;

            resourceUpdateHandle = new AutoResetEvent(true);

            updateTimes = new List<float>();
            frameTimes = new List<float>();

            BackColor = Color4.Black;
        }

        /// <summary>
        /// Starts the loop.
        /// </summary>
        public void Start()
        {
            Logger logger = GameEngine.TryQueryComponent<Logger>();

            if (this.IsRunning)
            {
                logger?.PostMessage(
                    "It has been tried to start an already running game loop.",
                    LogMessageKind.Warning, LogMessagePriority.Engine);
                return;
            }

            this.IsRunning = true;
            for (int i = 0; i < BufferCount; i++)
                bufferUpdateTimes[i] = TimeSpan.Zero;
            gameTime.Start();
            updateThread = new Thread(UpdateLoop) { Name = "game update thread" };
            updateThread.Start();

            logger?.PostMessage(
                "Game loop started.",
                LogMessageKind.Information, LogMessagePriority.Engine);
        }

        /// <summary>
        /// Stops the loop.
        /// </summary>
        /// <remarks>
        /// This method blocks until both update and render thread have terminated so don't call it within these threads.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">Is thrown when the method was called from the update or the render thread.</exception>
        public void Stop()
        {
            Logger logger = GameEngine.TryQueryComponent<Logger>();

            if (Thread.CurrentThread.ManagedThreadId == updateThread.ManagedThreadId
                || Thread.CurrentThread.ManagedThreadId == renderThread.ManagedThreadId)
            {
                logger?.PostMessage(
                    "Stopping of a game loop is impossible from the update thread or the render thread.",
                    LogMessageKind.Error, LogMessagePriority.Engine);
                throw new InvalidOperationException("The loop must not be stopped from the update thread or the render thread.");
            }

            if (!this.IsRunning)
            {
                logger?.PostMessage(
                    "It has been tried to stop a already stopped game loop.",
                    LogMessageKind.Warning, LogMessagePriority.Engine);
                return;
            }

            this.IsRunning = false;
            resourceUpdateHandle.Set();
            updateThread.Join();
            renderThread?.Join();
            updateThread = null;
            renderThread = null;
            resourceUpdateHandle.Set();

            gameTime.Reset();
            ups = 0;
            fps = 0;
            updateTimes.Clear();
            frameTimes.Clear();

            logger?.PostMessage(
                "Game loop stopped.",
                LogMessageKind.Information, LogMessagePriority.Engine);
        }

        private static void WaitForElapsed(Stopwatch sw, TimeSpan targetElapsed)
        {
            var bufferTime = TimeSpan.FromMilliseconds(1);
            if (sw.Elapsed < (targetElapsed - bufferTime))
            {
                Thread.Sleep(targetElapsed - sw.Elapsed - bufferTime);
            }

            while (sw.Elapsed < targetElapsed) { }
        }

        private void UpdateLoop()
        {
            var elapsed = TimeSpan.Zero;
            var targetElapsed = TimeSpan.FromTicks((long)((1000.0 / desiredUpdatesPerSecond) * TimeSpan.TicksPerMillisecond));
            int bufferIndex = 0;
            currentBufferIndex = BufferCount - 1;

            while (this.IsRunning)
            {
                TimeSpan startTime = gameTime.Elapsed;
                bufferUpdateTimes[bufferIndex] = startTime;

                Parallel.ForEach(Updateables, updateable => updateable.SetCurrentBufferIndex(currentBufferIndex));

                Update(bufferIndex, elapsed);
                WaitForElapsed(gameTime, startTime + targetElapsed);
                currentBufferIndex = bufferIndex;
                bufferIndex = (bufferIndex + 1) % BufferCount;

                elapsed = gameTime.Elapsed - startTime;

                updateTimes.Add((float)elapsed.TotalMilliseconds);
                int counter = 0;
                int index = updateTimes.Count - 1;
                float ms = 0;
                while (index >= 0 && ms + updateTimes[index] <= 1000)
                {
                    ms += updateTimes[index];
                    counter++;
                    index--;
                }
                if (index > 0) updateTimes.RemoveRange(0, index);
                ups = counter + ((1000 - ms) / updateTimes[0]);

                if (renderThread == null)
                {
                    renderThread = new Thread(RenderLoop) { Name = "game render thread" };
                    renderThread.Start();
                }
            }
        }

        private void Update(int bufferIndex, TimeSpan elapsed)
        {
            //resourceUpdateHandle.WaitOne();

            Updateables.ApplyChanges();
            Parallel.ForEach(Updateables, updateable => updateable.Update(bufferIndex, elapsed));
        }

        private void RenderLoop()
        {
            Renderer renderer = GameEngine.QueryComponent<Renderer>();

            while (this.IsRunning)
            {
                TimeSpan startTime = gameTime.Elapsed;

                Render(currentBufferIndex, gameTime.Elapsed - bufferUpdateTimes[currentBufferIndex], renderer);

                TimeSpan elapsed = gameTime.Elapsed - startTime;

                frameTimes.Add((float)elapsed.TotalMilliseconds);
                int counter = 0;
                int index = frameTimes.Count - 1;
                float ms = 0;
                while (index >= 0 && ms + frameTimes[index] <= 1000)
                {
                    ms += frameTimes[index];
                    counter++;
                    index--;
                }
                if (index > 0) frameTimes.RemoveRange(0, index);
                fps = counter + ((1000 - ms) / frameTimes[0]);
            }
        }

        private void Render(int bufferIndex, TimeSpan elapsed, Renderer renderer)
        {
            Renderables.ApplyChanges();
            ResourceUpdateRequested?.Invoke(this, EventArgs.Empty);
            resourceUpdateHandle.Set();

            SortRenderables();

            renderer.BeginRender(BackColor);

            foreach (RenderContainer renderable in Renderables)
                renderable.Render(bufferIndex, elapsed, renderer);

            renderer.EndRender();
        }
    }
}
