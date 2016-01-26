using System;
using System.Collections.Generic;
using GameUtils.Collections;

namespace GameUtils
{
    /// <summary>
    /// A handle that points to a resource.
    /// </summary>
    public class ResourceHandle : IDisposable
    {
        BufferedLinkedList<IResource> list;
        LinkedListNode<IResource> node;
        bool disposed;

        internal ResourceHandle(BufferedLinkedList<IResource> list, LinkedListNode<IResource> node)
        {
            this.list = list;
            this.node = node;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                list.Remove(node);
                list = null;
                node = null;
                disposed = true;
            }
        }
    }
}
