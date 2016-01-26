using System;

namespace GameUtils
{
    public abstract class UpdateContainer : IDisposable
    {
        public abstract void SetCurrentBufferIndex(int bufferIndex);

        public abstract void Update(int bufferIndex, TimeSpan elapsed);

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
        { }

        ~UpdateContainer()
        {
            Dispose(false);
        }
    }

    internal sealed class UpdateContainer<TState> : UpdateContainer where TState : IBufferedState<TState>
    {
        readonly RegistrationContext<TState> context; 
        readonly TState[] buffers;

        public bool AvailableForRendering { get; private set; }

        public UpdateContainer(RegistrationContext<TState> context, out RenderContainer renderer)
        {
            this.context = context;
            AvailableForRendering = false;
            buffers = new[] { context.CreateBuffer(), context.CreateBuffer() };
            renderer = context.Renderer != null ? new RenderContainer<TState>(this, buffers, context) : null;
        }

        public override void SetCurrentBufferIndex(int bufferIndex)
        {
            context.CurrentState = buffers[bufferIndex];
        }

        public TState GetBuffer(int bufferIndex)
        {
            return buffers[bufferIndex];
        }

        public override void Update(int bufferIndex, TimeSpan elapsed)
        {
            buffers[bufferIndex].Update(buffers[bufferIndex ^ 0x1], elapsed);
            AvailableForRendering = true;
        }

        protected override void Dispose(bool disposing)
        {
            IDisposable disposable = context as IDisposable;
            if (disposable != null) disposable.Dispose();
        }
    }
}
