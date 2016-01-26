using System;
using System.Threading.Tasks;
using GameUtils.Graphics;

namespace GameUtils
{
    /// <summary>
    /// Groups game objects to control z-order.
    /// </summary>
    public abstract class Layer<TState, TRenderer> : RegistrationTargetBase, IEngineComponent, IDisposable
        where TState : LayerState<TState>
        where TRenderer : LayerRenderer<TState>
    {
        private sealed class LayerContext<TState, TRenderer> : RegistrationContext<TState>
            where TState : LayerState<TState>
            where TRenderer : LayerRenderer<TState>
        {
            readonly Layer<TState, TRenderer> layer;
            int counter;

            public new event EventHandler CurrentStateChanged
            {
                add { base.CurrentStateChanged += value; }
                remove { base.CurrentStateChanged -= value; }
            }

            protected internal override IStateRenderer<TState> Renderer
            {
                get { return layer.Renderer; }
            }

            public LayerContext(Layer<TState, TRenderer> layer)
            {
                this.layer = layer;
            }

            protected internal override TState CreateBuffer()
            {
                TState updateable = layer.CreateBufferInner();
                updateable.SetLayerInfo(layer.Updateables, counter++);
                return updateable;
            }
        }

        readonly LayerContext<TState, TRenderer> context; 
        readonly GameHandle<TState> handle;
        TRenderer renderer;

        public event EventHandler DepthPositionChanged;

        /// <summary>
        /// A unique identifier for this layer.
        /// </summary>
        public object Tag { get; set; }

        protected override int CurrentBufferIndex
        {
            get { return context.CurrentState.BufferIndex; }
        }

        protected abstract bool IsCompatibleTo(IEngineComponent component);

        bool IEngineComponent.IsCompatibleTo(IEngineComponent component)
        {
            return IsCompatibleTo(component);
        }

        protected abstract TRenderer GetRendererInner();

        private TRenderer Renderer
        {
            get
            {
                if (renderer == null)
                {
                    renderer = GetRendererInner();
                    renderer.SetLayerInfo(Renderables);
                    renderer.SortingRequested += (sender, e) => SortRenderables();
                }

                return renderer;
            }
        }

        public float DepthPosition
        {
            get { return Renderer.DepthPosition; }
            set
            {
                if (value != Renderer.DepthPosition)
                {
                    Renderer.DepthPosition = value;
                    OnDepthPositionChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Creates a new layer.
        /// </summary>
        /// <param name="parent">Optional. The parent of the layer (this can be another layer, too).
        /// If this parameter is not specified or null is passed the game loop will be used as the parent.</param>
        protected Layer(RegistrationTargetBase parent = null)
        {
            context = new LayerContext<TState, TRenderer>(this);
            if (parent == null) parent = GameEngine.QueryComponent<GameLoop>();
            handle = parent.Register(context);

            context.CurrentStateChanged += (sender, e) => Parallel.ForEach(Updateables, updateable => updateable.SetCurrentBufferIndex(context.CurrentState.BufferIndex));
        }

        protected virtual void OnDepthPositionChanged(EventArgs e)
        {
            if (DepthPositionChanged != null)
                DepthPositionChanged(this, e);
        }

        protected abstract TState CreateBufferInner();

        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);

                disposed = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                handle.Dispose();
            }
        }

        ~Layer()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Groups game objects to control z-order.
    /// </summary>
    public sealed class Layer : Layer<LayerState, LayerRenderer>
    {
        LayerRenderer renderer;

        protected override bool IsCompatibleTo(IEngineComponent component)
        {
            return true;
        }

        protected override LayerRenderer GetRendererInner()
        {
            return renderer ?? (renderer = new LayerRenderer());
        }

        /// <summary>
        /// Creates a new layer with the game loop as parent.
        /// </summary>
        public Layer()
        { }

        /// <summary>
        /// Creates a new layer with the specified parent.
        /// </summary>
        public Layer(Layer parent)
            : base(parent)
        { }

        protected override LayerState CreateBufferInner()
        {
            return new LayerState();
        }
    }
}
