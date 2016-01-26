using System;

namespace GameUtils.Collections
{
    /// <summary>
    /// Offers information for item events.
    /// </summary>
    public class ItemEventArgs<T> : EventArgs
    {
        /// <summary>
        /// The item that caused the event.
        /// </summary>
        public T Item { get; private set; }

        public ItemEventArgs(T item)
        {
            Item = item;
        }
    }
}
