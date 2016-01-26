using System;

namespace GameUtils.UI
{
    public class UIState : IBufferedState<UIState>
    {
        protected internal UIState()
        { }

        protected virtual void Update(UIState oldState, TimeSpan elapsed)
        { }

        void IBufferedState<UIState>.Update(UIState oldState, TimeSpan elapsed)
        {
            Update(oldState, elapsed);
        }
    }
}
