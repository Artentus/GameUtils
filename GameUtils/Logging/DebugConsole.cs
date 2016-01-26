using System;
using System.Runtime.InteropServices;

namespace GameUtils.Logging
{
    /// <summary>
    /// The debug console.
    /// </summary>
    /// <remarks>All instances of this class share the same console window.</remarks>
    public sealed class DebugConsole : Logger, IDisposable
    {
        static bool keepConsole;

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool FreeConsole();

        /// <summary>
        /// Creates a new debug console.
        /// </summary>
        public DebugConsole()
        {
            bool hasConsole = GetConsoleWindow() != IntPtr.Zero;
            keepConsole = hasConsole;
            if (!hasConsole) AllocConsole();
        }

        protected override bool IsCompatibleTo(IEngineComponent component)
        {
            return !(component is DebugConsole);
        }

        public override void SendToOutput(string message)
        {
            Console.WriteLine(message);
        }

        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                if (!keepConsole) FreeConsole();

                GC.SuppressFinalize(this);
                disposed = true;
            }
        }

        ~DebugConsole()
        {
            Dispose();
        }
    }
}
