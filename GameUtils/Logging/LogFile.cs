using System;
using System.IO;
using System.Linq;

namespace GameUtils.Logging
{
    /// <summary>
    /// A logger which outputs to a file.
    /// </summary>
    public sealed class LogFile : Logger, IDisposable
    {
        readonly StreamWriter writer;
        readonly object locker;

        /// <summary>
        /// Creates a new log file.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="append">Optional. Indicates whether new messages should be appendet to an already existing file. If false an possble old file is overwritten.</param>
        /// <param name="priority">Optional. The priority of this log file.</param>
        public LogFile(string fileName, bool append = false, LogMessagePriority priority = LogMessagePriority.Low)
            : base(priority)
        {
            FileStream stream = File.Open(fileName, append ? FileMode.Append : FileMode.Create, FileAccess.ReadWrite);
            writer = new StreamWriter(stream);
            writer.AutoFlush = true;
            if (append && stream.Length > 0)
                writer.WriteLine(string.Concat(Enumerable.Repeat(writer.NewLine, 3)));

            locker = new object();
        }

        protected override bool IsCompatibleTo(IEngineComponent component)
        {
            return true;
        }

        public override void SendToOutput(string message)
        {
            lock (locker) writer.WriteLine(message);
        }

        /// <summary>
        /// Closes the log file.
        /// </summary>
        public void Close()
        {
            lock (locker) writer.Close();
        }

        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                this.Close();
                GC.SuppressFinalize(this);
            }
        }

        ~LogFile()
        {
            Dispose();
        }
    }
}
