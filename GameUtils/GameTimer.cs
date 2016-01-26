using System;
using GameUtils.Math;

namespace GameUtils
{
    /// <summary>
    /// A timer within the game loop.
    /// </summary>
    /// <remarks>This timer does not work with events but you have to poll the elapsed ticks manually.
    /// It is recommended using this timer instead of another to let the engine distribute computing power.</remarks>
    public class GameTimer
    {
        double interval;
        double elapsed;

        /// <summary>
        /// The interval at which timer ticks occurr in seconds.
        /// </summary>
        public double Interval
        {
            get { return interval; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");

                interval = value;
            }
        }

        /// <summary>
        /// The number of ticks per second.
        /// </summary>
        /// <remarks>This property only internaly sets the interval property.</remarks>
        public double TicksPerSecond
        {
            get { return 1 / interval; }
            set { interval = 1 / value; }
        }

        /// <summary>
        /// Indicates whether the timer is running.
        /// </summary>
        public bool Enabled { get; set; }

        private GameTimer(double interval, bool enabled)
        {
            if (interval <= 0)
                throw new ArgumentOutOfRangeException("interval");

            Interval = interval;
            Enabled = enabled;
        }

        /// <summary>
        /// Creates a new timer with the interval of one second.
        /// </summary>
        public GameTimer()
            : this(1, false) { }

        /// <summary>
        /// Creates a new timer using the specified ticks per second.
        /// </summary>
        public static GameTimer FromTicksPerSecond(double ticksPerSecond)
        {
            return new GameTimer(1 / ticksPerSecond, false);
        }

        /// <summary>
        /// Creates a new timer and directly starts it.
        /// </summary>
        public static GameTimer StartNew(double interval)
        {
            return new GameTimer(interval, true);
        }

        /// <summary>
        /// Creates a new timer using the specified ticks per second and directly starts it.
        /// </summary>
        public static GameTimer StartNewFromTicksPerSecond(double ticksPerSecond)
        {
            return new GameTimer(1 / ticksPerSecond, true);
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void Start()
        {
            Enabled = true;
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            Enabled = false;
        }

        /// <summary>
        /// Resets the timer so that from now on "interval" has to pass until the next tick.
        /// </summary>
        public void Reset()
        {
            elapsed = 0;
        }

        /// <summary>
        /// Resets the timer an starts it afterwards.
        /// </summary>
        public void Restart()
        {
            Reset();
            Start();
        }

        /// <summary>
        /// Calculates the number of ticks since the last poll.
        /// </summary>
        /// <param name="elapsed">The elapsed time since the last poll.</param>
        /// <returns>Returns the number of ticks since the last poll or -1 if the timer is not enabled.</returns>
        /// <remarks>Call this method in your update handler with the time passed by the engine.</remarks>
        public int Tick(TimeSpan elapsed)
        {
            if (!Enabled) return -1;

            this.elapsed += elapsed.TotalSeconds;
            return MathHelper.DivRem(this.elapsed, interval, out this.elapsed);
        }
    }
}
