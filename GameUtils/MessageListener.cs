using System;
using System.Collections.Generic;

namespace GameUtils
{
    internal interface IMessageListener
    {
        void PushMessage(EngineMessage message);
    }

    public sealed class MessageListener<T> : IMessageListener where T : EngineMessage
    {
        readonly Queue<T> messages;
        readonly object locker;

        /// <summary>
        /// Indicates whether there are messages pending.
        /// </summary>
        public bool MessageAvailable
        {
            get { lock (locker) return messages.Count > 0; }
        }

        public MessageListener()
        {
            messages = new Queue<T>();
            locker = new object();
        }

        void IMessageListener.PushMessage(EngineMessage message)
        {
            T msg = message as T;
            if (msg != null) lock (locker) messages.Enqueue(msg);
        }

        /// <summary>
        /// Retrieves the next message in the message queue.
        /// </summary>
        public T PeekMessage()
        {
            lock (locker) return messages.Peek();
        }

        /// <summary>
        /// Retrieves the next message in the message queue and removes it.
        /// </summary>
        public T PopMessage()
        {
            lock (locker) return messages.Dequeue();
        }
    }

    public sealed class MessageListenerHandle : IDisposable
    {
        LinkedList<IMessageListener> list;
        LinkedListNode<IMessageListener> node;
        object locker;
        bool disposed;

        internal MessageListenerHandle(LinkedList<IMessageListener> list, LinkedListNode<IMessageListener> node, object locker)
        {
            this.list = list;
            this.node = node;
            this.locker = locker;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                lock (locker) list.Remove(node);
                node = null;
                list = null;
                locker = null;

                disposed = true;
            }
        }
    }
}
