using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace YA.TenantWorker.Infrastructure.Logging
{
    public class DiagnosticsProvider
    {
        private static Process _process = Process.GetCurrentProcess();
        private static TimeSpan _oldCPUTime = TimeSpan.Zero;
        private static DateTime _lastMonitorTime = DateTime.UtcNow;
        private static DateTime _lastRpsTime = DateTime.UtcNow;
        private static double _cpu = 0, _rps = 0;
        private static readonly double RefreshRate = TimeSpan.FromSeconds(1).TotalMilliseconds;
        public static long Requests = 0;

        /// <summary>
        /// 获取诊断汇总信息
        /// </summary>
        /// <returns></returns>
        public DiagnosticsSummary GetDiagnostics()
        {
            _process.Refresh();

            var now = DateTime.UtcNow;
            var diagnostics = new DiagnosticsSummary()
            {
                PID = _process.Id,
                Allocated = GC.GetTotalMemory(false),
                WorkingSet = _process.WorkingSet64,
                Gen0 = GC.CollectionCount(0),
                Gen1 = GC.CollectionCount(1),
                Gen2 = GC.CollectionCount(2)
            };


            var cpuElapsedTime = now.Subtract(_lastMonitorTime).TotalMilliseconds;

            if (cpuElapsedTime > RefreshRate)
            {
                var newCPUTime = _process.TotalProcessorTime;
                var elapsedCPU = (newCPUTime - _oldCPUTime).TotalMilliseconds;

                _cpu = elapsedCPU * 100 / Environment.ProcessorCount / cpuElapsedTime;

                _lastMonitorTime = now;
                _oldCPUTime = newCPUTime;
            }

            var rpsElapsedTime = now.Subtract(_lastRpsTime).TotalMilliseconds;
            if (rpsElapsedTime > RefreshRate)
            {
                _rps = Requests * 1000 / rpsElapsedTime;

                System.Threading.Interlocked.Exchange(ref Requests, 0);

                _lastRpsTime = now;
            }

            diagnostics.CPU = _cpu;
            diagnostics.RPS = _rps;

            return diagnostics;
        }
    }

    /// <summary>
    /// 诊断汇总
    /// </summary>
    public class DiagnosticsSummary
    {
        /// <summary>
        /// Proccess PID 
        /// </summary>
        public int PID { get; set; }
        /// <summary>
        /// The memory occupied by objects.
        /// </summary>
        public long Allocated { get; set; }
        /// <summary>
        /// The working set includes both shared and private data. The shared data includes the pages that contain all the 
        /// instructions that the process executes, including instructions in the process modules and the system libraries.
        /// </summary>
        public long WorkingSet { get; set; }
        /// <summary>
        /// The value returned by this property represents the current size of memory used by the process, in bytes, that
        /// cannot be shared with other processes.
        /// </summary>
        public long PrivateBytes { get; set; }
        /// <summary>
        /// The number of generation 0 collections
        /// </summary>
        public int Gen0 { get; set; }
        /// <summary>
        /// The number of generation 1 collections
        /// </summary>
        public int Gen1 { get; set; }
        /// <summary>
        /// The number of generation 2 collections
        /// </summary>
        public int Gen2 { get; set; }
        /// <summary>
        /// The CPU occupied by system
        /// </summary>
        public double CPU { get; set; }
        /// <summary>
        /// The RPS occupied by system
        /// </summary>
        public double RPS { get; set; }
    }
}
