namespace GameUtils
{
    /// <summary>
    /// An engine component represents an autonomous part of the engine.
    /// </summary>
    /// <remarks>This interface should only be implemented from classes because the engine doesnt allow value types.</remarks>
    public interface IEngineComponent
    {
        /// <summary>
        /// A unique identification for this component.
        /// </summary>
        object Tag { get; }

        /// <summary>
        /// Determines if this component ist compatible with another component. Incompatible components cannot be registered at the same time.
        /// </summary>
        bool IsCompatibleTo(IEngineComponent component);
    }
}
