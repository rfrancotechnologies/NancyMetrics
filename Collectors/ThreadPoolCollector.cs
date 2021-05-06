using System.Threading;
using Prometheus;

namespace Com.RFranco.Iris.NancyMetrics.Stats
{
    public class ThreadPoolCollector : ISystemMetricCollector
    {
        private Gauge BusyThreads;
        private Gauge MinThreads;
        private Gauge AvailableThreads;
        private Gauge MaxThreads;

        private static readonly string TYPE_IOCP = "iocp";
        private static readonly string TYPE_WORKER = "worker";

        public ThreadPoolCollector()
        {
            BusyThreads = Metrics.CreateGauge("threadpool_num_busy_threads", "Number of busy threads of the thread pool.", "type");
            MinThreads = Metrics.CreateGauge("threadpool_num_min_threads", "Minimum number of threads of the thread pool.", "type");
            MaxThreads = Metrics.CreateGauge("threadpool_num_max_threads", "Maximum number of threads of the thread pool.", "type");
            AvailableThreads = Metrics.CreateGauge("threadpool_num_available_threads", "Number of free threads of the thread pool.", "type");
        }

        public void UpdateMetrics()
        {
            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minIoThreads);
            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxIoThreads);
            ThreadPool.GetAvailableThreads(out int freeWorkerThreads, out int freeIoThreads);
            
            MinThreads.WithLabels(TYPE_IOCP).Set(minIoThreads);
            MinThreads.WithLabels(TYPE_WORKER).Set(minWorkerThreads);
            MaxThreads.WithLabels(TYPE_IOCP).Set(maxIoThreads);
            MaxThreads.WithLabels(TYPE_WORKER).Set(maxWorkerThreads);
            AvailableThreads.WithLabels(TYPE_IOCP).Set(freeIoThreads);
            AvailableThreads.WithLabels(TYPE_WORKER).Set(freeWorkerThreads);
            BusyThreads.WithLabels(TYPE_IOCP).Set(maxIoThreads - freeIoThreads);
            BusyThreads.WithLabels(TYPE_WORKER).Set(maxWorkerThreads - freeWorkerThreads);
            

        }
    }
}