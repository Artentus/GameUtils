using System;
using System.Collections.Generic;

namespace GameUtils.Input
{
    public interface IInputRecorder : IDisposable
    {
        
    }

    public abstract class InputRecorder<T> : IInputRecorder where T : InputRecorder<T>
    {
        LinkedList<T> list;
        LinkedListNode<T> node;

        internal void SetDependency(LinkedList<T> list, LinkedListNode<T> node)
        {
            this.list = list;
            this.node = node;
        }

        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);

                disposed = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            list.Remove(node);

            if (disposing)
            {
                list = null;
                node = null;
            }
        }

        ~InputRecorder()
        {
            Dispose(false);
        }
    }
}
