using System;

namespace GameUtils
{
    /// <summary>
    /// Represents the state of a game object.
    /// </summary>
    public interface IBufferedState<in TSelf> where TSelf : IBufferedState<TSelf>
    {
        /// <summary>
        /// Updates the state.
        /// </summary>
        /// <param name="oldState">The old state.</param>
        /// <param name="elapsed">The time passed since the last update.</param>
        void Update(TSelf oldState, TimeSpan elapsed);
    }
}
