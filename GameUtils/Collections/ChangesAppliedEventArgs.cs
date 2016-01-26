using System;

namespace GameUtils.Collections
{
    public class ChangesAppliedEventArgs<T> : EventArgs
    {
        public T[] AddedItems { get; private set; }

        public T[] RemovedItems { get; private set; }

        public ChangesAppliedEventArgs(T[] addedItems, T[] removedItems)
        {
            AddedItems = addedItems;
            RemovedItems = removedItems;
        }
    }
}
