using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;

namespace TOH.Network.Common
{
    public class TimerService
    {
        private Stopwatch stopwatch = new Stopwatch();

        private readonly ILogger _logger;

        public long TimerFrequency { get { return Stopwatch.Frequency; } }

        public TimerService(ILogger<TimerService> logger)
        {
            _logger = logger;
        }

        public void Start(CancellationToken cancellationToken)
        {
            if (!stopwatch.IsRunning)
            {
                stopwatch.Start();
            }
            else
            {
                _logger.LogWarning("The stopwatch is already running.");
            }
        }

        public long GetTicks()
        {
            if (!stopwatch.IsRunning)
            {
                throw new Exception("The stopwatch is not running.");
            }

            return stopwatch.ElapsedTicks;
        }

        public void Stop()
        {
            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
            }
        }
    }
}