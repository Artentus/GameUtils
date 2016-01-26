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
        Thread updateThread;
        Thread renderThread;
        readonly AutoResetEvent[] updateHandle;
        readonly AutoResetEvent[] renderHandle;
        readonly AutoResetEvent resourceUpdateHandle;
        readonly List<double> frameTimes;
        int currentBufferIndex;

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
        /// Indicates the current framerate.
        /// </summary>
        public double Fps { get; private set; }

        protected override int CurrentBufferIndex
        {
            get { return currentBufferIndex; }
        }

        bool IEngineComponent.IsCompatibleTo(IEngineComponent component)
        {
            return !(component is GameLoop);
        }

        object IEngineComponent.Tag
        {
            get { return null; }
        }

        /// <summary>
        /// Creates a new game loop.
        /// </summary>
        public GameLoop(bool enableDepthBuffering = false)
        {
            this.IsRunning = false;
            EnableDepthBuffering = enableDepthBuffering;

            updateHandle = new AutoResetEvent[2];
            for (int i = 0; i < updateHandle.Length; i++)
                updateHandle[i] = new AutoResetEvent(false);

            renderHandle = new AutoResetEvent[2];
            for (int i = 0; i < renderHandle.Length; i++)
                renderHandle[i] = new AutoResetEvent(true);

            resourceUpdateHandle = new AutoResetEvent(true);

            frameTimes = new List<double>();

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
                if (logger != null) logger.PostMessage(
                    "It has been tried to start a already running game loop.",
                    LogMessageKind.Warning, LogMessagePriority.Engine);
                return;
            }

            this.IsRunning = true;
            updateThread = new Thread(UpdateLoop);
            updateThread.Name = "game update thread";
            updateThread.Start();
            renderThread = new Thread(RenderLoop);
            renderThread.Name = "game render thread";
            renderThread.Start();
            
            if (logger != null) logger.PostMessage(
                "Game loop started.",
                LogMessageKind.Information, LogMessagePriority.Engine);
        }

        /// <summary>
        /// Stops the loop.
        /// </summary>
        /// <remarks>
        /// This method blocks until both update and render thread have terminated so don't call it from there.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">Is thrown when the method was called from the update or the render thread.</exception>
        public void Stop()
        {
            Logger logger = GameEngine.TryQueryComponent<Logger>();

            if (Thread.CurrentThread.ManagedThreadId == updateThread.ManagedThreadId
                || Thread.CurrentThread.ManagedThreadId == renderThread.ManagedThreadId)
            {
                if (logger != null) logger.PostMessage(
                    "Stopping of a game loop is impossible from the update thread or the render thread.",
                    LogMessageKind.Error, LogMessagePriority.Engine);
                throw new InvalidOperationException("The loop must not be stopped from the update thread or the render thread.");
            }

            if (!this.IsRunning)
            {
                if (logger != null) logger.PostMessage(
                    "It has been tried to stop a already stopped game loop.",
                    LogMessageKind.Warning, LogMessagePriority.Engine);
                return;
            }

            this.IsRunning = false;
            resourceUpdateHandle.Set();
            updateThread.Join();
            renderThread.Join();
            Fps = 0;
            frameTimes.Clear();

            for (int i = 0; i < updateHandle.Length; i++)
                updateHandle[i].Reset();
            for (int i = 0; i < renderHandle.Length; i++)
                renderHandle[i].Set();
            resourceUpdateHandle.Set();

            if (logger != null) logger.PostMessage(
                "Game loop stopped.",
                LogMessageKind.Information, LogMessagePriority.Engine);
        }

        private void UpdateLoop()
        {
            Stopwatch sw = new Stopwatch();
            TimeSpan elapsed = default(TimeSpan);
            int bufferIndex = 0x0;
            currentBufferIndex = 0x1;

            while (this.IsRunning)
            {
                sw.Start();

                Parallel.ForEach(Updateables, updateable => updateable.SetCurrentBufferIndex(currentBufferIndex));

                Update(bufferIndex, elapsed);

                currentBufferIndex = bufferIndex;
                bufferIndex ^= 0x1;
                elapsed = sw.Elapsed;
                sw.Reset();
            }
        }

        private void Update(int bufferIndex, TimeSpan elapsed)
        {
            renderHandle[bufferIndex].WaitOne();
            resourceUpdateHandle.WaitOne();

            Updateables.ApplyChanges();
            Parallel.ForEach(Updateables, updateable => updateable.Update(bufferIndex, elapsed));

            updateHandle[bufferIndex].Set();
        }

        private void RenderLoop()
        {
            Renderer renderer = GameEngine.QueryComponent<Renderer>();
            var sw = new Stopwatch();
            int bufferIndex = 0x0;

            while (this.IsRunning)
            {
                sw.Restart();

                Render(bufferIndex, renderer);
                bufferIndex ^= 0x1;

                sw.Stop();
                frameTimes.Add(sw.Elapsed.TotalMilliseconds);
                int counter = 0;
                int index = frameTimes.Count - 1;
                double ms = 0;
                while (index >= 0 && ms + frameTimes[index] <= 1000)
                {
                    ms += frameTimes[index];
                    counter++;
                    index--;
                }
                if (index > 0) frameTimes.RemoveRange(0, index);
                Fps = counter + ((1000 - ms) / frameTimes[0]);
            }
        }

        private void Render(int bufferIndex, Renderer renderer)
        {
            updateHandle[bufferIndex].WaitOne();

            Renderables.ApplyChanges();
            if (ResourceUpdateRequested != null)
                ResourceUpdateRequested(this, EventArgs.Empty);
            resourceUpdateHandle.Set();

            SortRenderables();

            renderer.BeginRender(BackColor);

            foreach (RenderContainer renderable in Renderables)
                renderable.Render(bufferIndex, renderer);

            renderer.EndRender();
            renderHandle[bufferIndex].Set();
        }
    }
}
