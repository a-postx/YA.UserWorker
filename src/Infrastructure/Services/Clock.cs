using System;
using YA.UserWorker.Application.Interfaces;

namespace YA.UserWorker.Infrastructure.Services
{
    /// <summary>
    /// Retrieves the current date and/or time. Helps with unit testing by letting you mock the system clock.
    /// </summary>
    public class Clock : IClockService
    {
        /// <summary>
        /// Current time in UTC.
        /// </summary>
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
