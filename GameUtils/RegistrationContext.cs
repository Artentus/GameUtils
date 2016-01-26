using System;
using GameUtils.Graphics;

namespace GameUtils
{
    /// <summary>
    /// Creates objects for registration at the game loop.
    /// </summary>
    public abstract class RegistrationContext<TState> where TState : IBufferedState<TState>
    {
        TState currentState;

        protected event EventHandler CurrentStateChanged;

        /// <summary>
        /// The current state of this context.
        /// </summary>
        public TState CurrentState
        {
            get { return currentState; }
            set
            {
                currentState = value;

                if (CurrentStateChanged != null)
                    CurrentStateChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Creates a new buffer in this context.
        /// </summary>
        protected internal abstract TState CreateBuffer();

        /// <summary>
        /// The renderer shared by all buffers creted in this context.
        /// </summary>
        protected internal abstract IStateRenderer<TState> Renderer { get; }
    }
}
