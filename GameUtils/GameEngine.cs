using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameUtils.Collections;
using GameUtils.Graphics;
using GameUtils.Logging;

namespace GameUtils
{
    /// <summary>
    /// Beeing the central unit of any game, the game engine is in charge of managing all components and resources.
    /// </summary>
    public static class GameEngine
    {
        static readonly LinkedList<IEngineComponent> Components;
        static readonly object ComponentsLocker;
        static readonly BufferedLinkedList<IResource> Resources;
        static readonly LinkedList<IMessageListener> MessageListeners;
        static readonly object ListenersLocker;
        static readonly List<Logger> Loggers;
        static readonly object LoggersLocker;

        static GameEngine()
        {
            Components = new LinkedList<IEngineComponent>();
            ComponentsLocker = new object();
            Resources = new BufferedLinkedList<IResource>();
            Resources.ChangesApplied += (sender, e) => Parallel.ForEach(e.AddedItems,
                resource =>
                {
                    if (resource.UpdateMode == UpdateMode.Synchronous)
                    {
                        resource.ApplyChanges();
                    }
                    else
                    {
                        Task.Factory.StartNew(() =>
                        {
                            resource.IsReady = false;
                            resource.ApplyChanges();
                            resource.IsReady = true;
                        }, TaskCreationOptions.PreferFairness);
                    }
                });
            MessageListeners = new LinkedList<IMessageListener>();
            ListenersLocker = new object();
            Loggers = new List<Logger>();
            LoggersLocker = new object();
        }

        /// <summary>
        /// Posts a log message to all registered loggers.
        /// </summary>
        /// <param name="message">The message to post.</param>
        /// <param name="messageKind">Optional. The kind of the message.</param>
        /// <param name="messagePriority">Optional. The priority of the message.</param>
        /// <exception cref="System.ArgumentNullException">Is thrown if message is null.</exception>
        public static void PostGlobalLogMessage(string message, LogMessageKind messageKind = LogMessageKind.None, LogMessagePriority messagePriority = LogMessagePriority.Low)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            lock (LoggersLocker)
            {
                foreach (var logger in Loggers)
                    logger.PostMessage(message, messageKind, messagePriority);
            }
        }

        /// <summary>
        /// Registers a component.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is thrown when the component was incompatible with another component.</exception>
        public static EngineHandle RegisterComponent<T>(T component) where T : class, IEngineComponent
        {
            var conflictingComponents = Components.Where(comp => !component.IsCompatibleTo(comp) || !comp.IsCompatibleTo(component));
            if (conflictingComponents.Any())
            {
                PostGlobalLogMessage(string.Format("Error registering engine component {0}: incompatibility to existing component(s) {1} detected.",
                        component.GetType().FullName, string.Join(", ", conflictingComponents.Select(c => c.GetType().FullName))), LogMessageKind.Error, LogMessagePriority.Engine);
                throw new InvalidOperationException("This component is not compatible to an already registered one.");
            }

            OnComponentAdded(component);
            LinkedListNode<IEngineComponent> node;
            lock (ComponentsLocker) node = Components.AddLast(component);
            var handle = new EngineHandle(Components, node, ComponentsLocker);
            handle.Disposed += (sender, e) => OnComponentRemoved(((EngineHandle)sender).Component);
            PostGlobalLogMessage(string.Format("Engine component {0} successfully registered.", component.GetType().FullName),
                LogMessageKind.Information, LogMessagePriority.Engine);
            return handle;
        }

        private static void OnComponentAdded(IEngineComponent component)
        {
            GameLoop loop = component as GameLoop;
            if (loop != null) OnGameLoopAdded(loop);

            Renderer renderer = component as Renderer;
            if (renderer != null) OnRendererAdded(renderer);

            Logger logger = component as Logger;
            if (logger != null) OnLoggerAdded(logger);
        }

        private static void OnGameLoopAdded(GameLoop loop)
        {
            loop.ResourceUpdateRequested += GameLoop_ResourceUpdateRequested;
        }

        private static void OnRendererAdded(Renderer renderer)
        {
            GameLoop loop = TryQueryComponent<GameLoop>();
            if (loop != null && loop.IsRunning)
                throw new InvalidOperationException("A renderer cannot be added while the game loop is running.");
        }

        private static void OnLoggerAdded(Logger logger)
        {
            lock (LoggersLocker) Loggers.Add(logger);
        }

        private static void OnComponentRemoved(IEngineComponent component)
        {
            GameLoop loop = component as GameLoop;
            if (loop != null) OnGameLoopRemoved(loop);

            if (component is Renderer)
                OnRendererRemoved();

            Logger logger = component as Logger;
            if (logger != null) OnLoggerRemoved(logger);
        }

        private static void OnGameLoopRemoved(GameLoop loop)
        {
            loop.ResourceUpdateRequested -= GameLoop_ResourceUpdateRequested;
        }

        private static void OnRendererRemoved()
        {
            GameLoop loop = TryQueryComponent<GameLoop>();
            if (loop != null && loop.IsRunning)
                throw new InvalidOperationException("The renderer cannot be removed while the game loop is running.");
        }

        private static void OnLoggerRemoved(Logger logger)
        {
            lock (LoggersLocker) Loggers.Remove(logger);
        }

        private static void GameLoop_ResourceUpdateRequested(object sender, EventArgs e)
        {
            Parallel.ForEach(Resources,
                resource =>
                {
                    if (resource.IsReady)
                    {
                        if (resource.UpdateMode == UpdateMode.Synchronous || resource.UpdateMode == UpdateMode.InitAsynchronous)
                        {
                            resource.ApplyChanges();
                        }
                        else
                        {
                            Task.Factory.StartNew(() =>
                            {
                                resource.IsReady = false;
                                resource.ApplyChanges();
                                resource.IsReady = true;
                            }, TaskCreationOptions.PreferFairness);
                        }
                    }
                });
            Resources.ApplyChanges();
        }

        /// <summary>
        /// Queries a component.
        /// </summary>
        public static T QueryComponent<T>(object tag = null) where T : IEngineComponent
        {
            try
            {
                if (tag == null)
                    return Components.OfType<T>().First();
                else
                    return Components.OfType<T>().First(component => tag.Equals(component.Tag));
            }
            catch (InvalidOperationException)
            {
                PostGlobalLogMessage(string.Format("Error querying component of type {0}{1}: no matches found.",
                    typeof (T).FullName, tag != null ? string.Format(" with tag '{0}'", tag) : string.Empty),
                    LogMessageKind.Error, LogMessagePriority.Engine);
                throw;
            }
        }

        private static bool TryQueryComponentInner<T>(object tag, out T result) where T : IEngineComponent
        {
            result = default(T);

            foreach (T component in Components.OfType<T>())
            {
                if (tag == null || tag.Equals(component.Tag))
                {
                    result = component;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to query a component. If no matches were found, a default value is returned.
        /// </summary>
        public static T TryQueryComponent<T>(object tag = null) where T : IEngineComponent
        {
            T result;
            if (!TryQueryComponentInner(tag, out result))
            {
                PostGlobalLogMessage(string.Format("No matches found for queried component {0}{1}, returning default value.",
                    typeof(T).FullName, tag != null ? string.Format(" with tag '{0}'", tag) : string.Empty),
                    LogMessageKind.Warning, LogMessagePriority.Engine);
            }

            return result;
        }

        /// <summary>
        /// Registeres a resource.
        /// </summary>
        public static ResourceHandle RegisterResource(IResource resource)
        {
            string tagString = resource.Tag?.ToString() ?? string.Empty;
            PostGlobalLogMessage(string.Format("Resource {0}{1} loaded.", resource.GetType().FullName, string.IsNullOrEmpty(tagString) ? string.Empty : " with tag " + tagString),
                LogMessageKind.Information, LogMessagePriority.Engine);

            return new ResourceHandle(Resources, Resources.Add(resource));
        }

        /// <summary>
        /// Queries a resource.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is thrown when there was no matching resource found.</exception>
        public static T QueryResource<T>(object tag = null) where T : IResource
        {
            try
            {
                if (tag == null)
                    return Resources.OfType<T>().First();
                else
                    return Resources.OfType<T>().First(resource => tag.Equals(resource.Tag));
            }
            catch (InvalidOperationException)
            {
                PostGlobalLogMessage(string.Format("Error querying resource of type {0}{1}: no matches found.",
                    typeof (T).FullName, tag != null ? string.Format(" with tag '{0}'", tag) : string.Empty),
                    LogMessageKind.Error, LogMessagePriority.Engine);
                throw;
            }
        }

        /// <summary>
        /// Registeres a message listener.
        /// </summary>
        public static MessageListenerHandle RegisterMessageListener<T>(MessageListener<T> listener) where T : EngineMessage
        {
            LinkedListNode<IMessageListener> node;
            lock (ListenersLocker) node = MessageListeners.AddLast(listener);
            return new MessageListenerHandle(MessageListeners, node, ListenersLocker);
        }

        /// <summary>
        /// Posts a global message.
        /// </summary>
        public static void PostMessage(EngineMessage message)
        {
            lock (ListenersLocker)
            {
                foreach (IMessageListener listener in MessageListeners)
                    listener.PushMessage(message);
            }
            
            PostGlobalLogMessage(string.Format("Global message {0} with ID {1} posted to all listener queues.", message.GetType().FullName, message.Id),
                LogMessageKind.Information, LogMessagePriority.Engine);
        }
    }
}
