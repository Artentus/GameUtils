using System;
using System.Collections.Generic;

namespace GameUtils.Logging
{
    /// <summary>
    /// A engine component which performs basic logging.
    /// </summary>
    public abstract class Logger : IEngineComponent
    {
        static readonly Dictionary<LogMessageKind, string> MessageKindStrings;

        static Logger()
        {
            MessageKindStrings = new Dictionary<LogMessageKind, string>(3)
            {
                { LogMessageKind.Information, "[Info]" },
                { LogMessageKind.Warning, "[Warning]" },
                { LogMessageKind.Error, "[Error]" },
            };
        }

        /// <summary>
        /// A unique Identifier for this logger.
        /// </summary>
        public object Tag { get; set; }

        protected abstract bool IsCompatibleTo(IEngineComponent component);

        bool IEngineComponent.IsCompatibleTo(IEngineComponent component)
        {
            return IsCompatibleTo(component);
        }

        /// <summary>
        /// Indicates whether the logger should include time stamps in the output.
        /// </summary>
        public bool IncludeTimeStamp { get; set; }

        /// <summary>
        /// Specifies the format of the time stamps.
        /// </summary>
        public string TimestampFormat { get; set; }

        /// <summary>
        /// The priority of this logger. Messages with a smaller priority will not be postet.
        /// </summary>
        public LogMessagePriority Priority { get; set; }

        protected Logger(bool includeTimeStamp, string timestampFormat)
        {
            IncludeTimeStamp = includeTimeStamp;
            TimestampFormat = timestampFormat;
            Priority = LogMessagePriority.Low;
        }

        protected Logger(bool includeTimeStamp)
        {
            IncludeTimeStamp = includeTimeStamp;
            TimestampFormat = "yyyy-MM-dd HH:mm:ss";
            Priority = LogMessagePriority.Low;
        }

        protected Logger(LogMessagePriority priority)
        {
            IncludeTimeStamp = true;
            TimestampFormat = "yyyy-MM-dd HH:mm:ss";
            Priority = priority;
        }

        protected Logger()
        {
            IncludeTimeStamp = true;
            TimestampFormat = "yyyy-MM-dd HH:mm:ss";
            Priority = LogMessagePriority.Low;
        }

        /// <summary>
        /// Posts a message to the logger output.
        /// </summary>
        /// <param name="message">The message to post.</param>
        /// <param name="messageKind">Optional. The kind of the message.</param>
        /// <param name="messagePriority">Optional. The priority of the message.</param>
        /// <remarks>
        /// A message kind different from none will be indicated by a prefix in the output.
        /// If the message priority is smaller then the priority of the logger, the message will not be postet.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Is thrown if message is null.</exception>
        public void PostMessage(string message, LogMessageKind messageKind = LogMessageKind.None, LogMessagePriority messagePriority = LogMessagePriority.Low)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            
            if (messagePriority >= Priority)
            {
                var messageParts = new List<string>();
                if (IncludeTimeStamp) messageParts.Add(string.Concat("[", DateTime.Now.ToString(TimestampFormat), "]"));
                if (messageKind != LogMessageKind.None) messageParts.Add(MessageKindStrings[messageKind]);
                messageParts.Add(message);

                this.SendToOutput(string.Join(" ", messageParts));
            }
        }

        /// <summary>
        /// Directly pushes a message to the output.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <remarks>
        /// This method does not perform any formatting. If you want to include time stamps and message kinds, use PostMessage instead.
        /// </remarks>
        public abstract void SendToOutput(string message);
    }
}
