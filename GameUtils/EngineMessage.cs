using System.Threading;

namespace GameUtils
{
    public abstract class EngineMessage
    {
        static long idCounter;

        /// <summary>
        /// The unique identification of the message.
        /// </summary>
        public readonly long Id;

        /// <summary>
        /// Indicates whether this message has already been processed.
        /// </summary>
        public bool Processed { get; set; }

        protected EngineMessage()
        {
            Id = idCounter;
            Interlocked.Increment(ref idCounter);
        }
    }
}
