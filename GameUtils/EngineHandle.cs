using System;
using System.Collections.Generic;

namespace GameUtils
{
    /// <summary>
    /// A handle that points to an engine component.
    /// </summary>
    public class EngineHandle : IDisposable
    {
        LinkedList<IEngineComponent> list;
        LinkedListNode<IEngineComponent> node;
        object locker;
        bool disposed;

        internal event EventHandler Disposed;

        internal IEngineComponent Component => node.Value;

        internal EngineHandle(LinkedList<IEngineComponent> list, LinkedListNode<IEngineComponent> node, object locker)
        {
            this.list = list;
            this.node = node;
            this.locker = locker;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                lock (locker) list.Remove(node);
                Disposed?.Invoke(this, EventArgs.Empty);
                list = null;
                node = null;
                locker = null;
            }
        }
    }
}
