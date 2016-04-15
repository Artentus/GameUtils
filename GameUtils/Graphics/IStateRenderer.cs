using System;

namespace GameUtils.Graphics
{
    /// <summary>
    /// An object that is capable of rendering the state of an game object.
    /// </summary>
    public interface IStateRenderer<in TState> where TState : IBufferedState<TState>
    {
        /// <summary>
        /// This event should be risen if the depth position of this renderer changed, otherwise the change will not be recognized.
        /// </summary>
        event EventHandler DepthPositionChanged;

        /// <summary>
        /// Indicates the position on the z axis of this renderable allowing to order the drawing operations.
        /// Greater values are drawn on top.
        /// </summary>
        float DepthPosition { get; }

        /// <summary>
        /// Renders the the given state.
        /// </summary>
        /// <param name="state">The state to render.</param>
        /// <param name="elapsed">The time passed since state has been updated.</param>
        /// <param name="renderer">The renderer to use.</param>
        void Render(TState state, TimeSpan elapsed, Renderer renderer);
    }
}
