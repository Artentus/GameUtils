using System;
using System.Collections.Generic;
using System.Linq;
using GameUtils.Collections;
using GameUtils.Logging;

namespace GameUtils
{
    public abstract class RegistrationTargetBase
    {
        protected const int BufferCount = 3;

        protected internal readonly BufferedLinkedList<UpdateContainer> Updateables;
        protected internal readonly BufferedList<RenderContainer> Renderables;

        bool sortAtNextCall;

        internal event EventHandler<ChangesAppliedEventArgs<UpdateContainer>> ChangesApplied
        {
            add { Updateables.ChangesApplied += value; }
            remove { Updateables.ChangesApplied -= value; }
        }

        /// <summary>
        /// Indicates whether depth buffering is enabled.
        /// </summary>
        public bool EnableDepthBuffering { get; set; }

        protected RegistrationTargetBase()
        {
            Updateables = new BufferedLinkedList<UpdateContainer>();
            Renderables = new BufferedList<RenderContainer>();
        }

        protected void SortRenderables()
        {
            if (EnableDepthBuffering && sortAtNextCall)
            {
                Renderables.Sort();
                sortAtNextCall = false;
            }
        }

        /// <summary>
        /// Registeres a game object.
        /// </summary>
        public GameHandle<T> Register<T>(RegistrationContext<T> context) where T : IBufferedState<T>
        {
            Logger logger = GameEngine.TryQueryComponent<Logger>();
            if (context == null)
            {
                logger?.PostMessage(
                    string.Format("An atempt to register 'null' on target {0} has been made.", this.GetType().FullName),
                    LogMessageKind.Error, LogMessagePriority.Engine);
                throw new ArgumentNullException(nameof(context));
            }

            RenderContainer renderable;
            var node = Updateables.Add(new UpdateContainer<T>(context, BufferCount, out renderable));
            if (renderable != null)
            {
                Renderables.Add(renderable);
                renderable.DepthPositionChanged += (sender, e) => sortAtNextCall = true;
                sortAtNextCall = true;
            }

            logger?.PostMessage(
                string.Format("Game handle {0} has been registered in context {1} on target {2}.",
                    typeof(GameHandle<T>).FullName, context.GetType().FullName, this.GetType().FullName),
                LogMessageKind.Information, LogMessagePriority.Engine);

            return new GameHandle<T>(this, node, renderable);
        }

        protected abstract int CurrentBufferIndex { get; }

        /// <summary>
        /// Enumerates all buffers of a specific type.
        /// </summary>
        public IEnumerable<T> EnumerateBuffers<T>() where T : IBufferedState<T>
        {
            return Updateables.OfType<UpdateContainer<T>>().Select(updateable => updateable.GetBuffer(CurrentBufferIndex));
        }
    }
}
