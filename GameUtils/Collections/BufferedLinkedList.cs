using System;
using System.Collections.Generic;
using System.Linq;

namespace GameUtils.Collections
{
    /// <summary>
    /// A buffered, linked collection that writes changes only to a buffer until it is updated.
    /// </summary>
    public class BufferedLinkedList<T> : IEnumerable<T>
    {
        readonly LinkedList<T> list;
        readonly Queue<BufferedChange<T>> bufferedChanges;
        readonly object locker;

        /// <summary>
        /// Is raised when an element is getting added.
        /// </summary>
        public event EventHandler<ItemEventArgs<T>> ItemAdding;
        /// <summary>
        /// Is raised when an element was added.
        /// </summary>
        public event EventHandler<ItemEventArgs<T>> ItemAdded;
        /// <summary>
        /// Is raised when an element is getting removed.
        /// </summary>
        public event EventHandler<ItemEventArgs<T>> ItemRemoving;
        /// <summary>
        /// Is raised when an element was removed.
        /// </summary>
        public event EventHandler<ItemEventArgs<T>> ItemRemoved;

        /// <summary>
        /// Is risen when the buffered changes got applied;
        /// </summary>
        public event EventHandler<ChangesAppliedEventArgs<T>>  ChangesApplied;

        /// <summary>
        /// Creates a new buffered, linked collection.
        /// </summary>
        public BufferedLinkedList()
        {
            list = new LinkedList<T>();
            bufferedChanges = new Queue<BufferedChange<T>>();
            locker = new object();
        }

        /// <summary>
        /// Applies all buffered changes to the list.
        /// </summary>
        public void ApplyChanges()
        {
            var added = new List<T>(bufferedChanges.Count);
            var removed = new List<T>(bufferedChanges.Count + list.Count);

            lock (locker)
            {
                while (bufferedChanges.Count > 0)
                    bufferedChanges.Dequeue().ApplyTo(this, list, added, removed);
            }

            if (ChangesApplied != null)
                ChangesApplied(this, new ChangesAppliedEventArgs<T>(added.ToArray(), removed.ToArray()));
        }

        /// <summary>
        /// Adds an element to the colection.
        /// </summary>
        /// <remarks>This memeber is buffered.</remarks>
        public LinkedListNode<T> Add(T item)
        {
            lock (locker)
            {
                var node = new LinkedListNode<T>(item);
                bufferedChanges.Enqueue(new AddChange<T>(node));
                return node;
            }
        }

        /// <summary>
        /// Adds an element to the colection.
        /// </summary>
        /// <remarks>This memeber is buffered.</remarks>
        public void Add(LinkedListNode<T> node)
        {
            lock (locker)
            {
                bufferedChanges.Enqueue(new AddChange<T>(node));
            }
        }

        /// <summary>
        /// Removes an element from the colection.
        /// </summary>
        /// <remarks>This memeber is buffered.</remarks>
        public bool Remove(LinkedListNode<T> item)
        {
            lock (locker)
            {
                bufferedChanges.Enqueue(new RemoveChange<T>(item));
                return true;
            }
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        /// <remarks>This memeber is buffered.</remarks>
        public void Clear()
        {
            lock (locker)
            {
                bufferedChanges.Enqueue(new ClearChange<T>());
            }
        }

        /// <summary>
        /// Returns a value indicating whether this collection contains a specific element.
        /// </summary>
        /// <remarks>This memeber is buffered.</remarks>
        public bool Contains(T item)
        {
            lock (locker)
            {
                return list.Contains(item);
            }
        }

        /// <summary>
        /// Copies the collection to an array.
        /// </summary>
        /// <remarks>This memeber is buffered.</remarks>
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (locker)
            {
                list.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// Returns the count of elements in this collection.
        /// </summary>
        /// <remarks>This memeber is buffered.</remarks>
        public int Count
        {
            get
            {
                lock (locker)
                {
                    return list.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (locker)
            {
                return list.GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock (locker)
            {
                return list.GetEnumerator();
            }
        }

        protected virtual void OnItemAdding(ItemEventArgs<T> e)
        {
            if (ItemAdding != null)
                ItemAdding(this, e);
        }

        protected virtual void OnItemAdded(ItemEventArgs<T> e)
        {
            if (ItemAdded != null)
                ItemAdded(this, e);
        }

        protected virtual void OnItemRemoving(ItemEventArgs<T> e)
        {
            if (ItemRemoving != null)
                ItemRemoving(this, e);
        }

        protected virtual void OnItemRemoved(ItemEventArgs<T> e)
        {
            if (ItemRemoved != null)
                ItemRemoved(this, e);
        }

        private abstract class BufferedChange<T>
        {
            public abstract void ApplyTo(BufferedLinkedList<T> parent, LinkedList<T> list, List<T> added, List<T> removed);
        }

        private class AddChange<T> : BufferedChange<T>
        {
            readonly LinkedListNode<T> item;

            public AddChange(LinkedListNode<T> item)
            {
                this.item = item;
            }

            public override void ApplyTo(BufferedLinkedList<T> parent, LinkedList<T> list, List<T> added, List<T> removed)
            {
                added.Add(item.Value);
                parent.OnItemAdding(new ItemEventArgs<T>(item.Value));
                list.AddLast(item);
                parent.OnItemAdded(new ItemEventArgs<T>(item.Value));
            }
        }

        private class RemoveChange<T> : BufferedChange<T>
        {
            readonly LinkedListNode<T> item;

            public RemoveChange(LinkedListNode<T> item)
            {
                this.item = item;
            }

            public override void ApplyTo(BufferedLinkedList<T> parent, LinkedList<T> list, List<T> added, List<T> removed)
            {
                removed.Add(item.Value);
                parent.OnItemRemoving(new ItemEventArgs<T>(item.Value));
                list.Remove(item);
                parent.OnItemRemoved(new ItemEventArgs<T>(item.Value));
            }
        }

        private class ClearChange<T> : BufferedChange<T>
        {
            public override void ApplyTo(BufferedLinkedList<T> parent, LinkedList<T> list, List<T> added, List<T> removed)
            {
                T[] temp = list.ToArray();
                removed.AddRange(temp);

                for (int i = 0; i < temp.Length; i++)
                    parent.OnItemRemoving(new ItemEventArgs<T>(temp[i]));

                list.Clear();

                for (int i = 0; i < temp.Length; i++)
                    parent.OnItemRemoved(new ItemEventArgs<T>(temp[i]));
            }
        }
    }
}
