using System;

namespace GameUtils.Input
{
    public abstract class InputSource<TState> : IEngineComponent, IDisposable where TState : IInputState
    {
        public abstract TState CurrentState { get; }

        public object Tag { get; set; }

        protected abstract bool IsCompatibleTo(IEngineComponent component);

        bool IEngineComponent.IsCompatibleTo(IEngineComponent component)
        {
            return IsCompatibleTo(component);
        }

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

        ~InputSource()
        {
            Dispose(false);
        }
    }
}
