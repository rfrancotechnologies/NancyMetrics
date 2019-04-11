using System;
using System.Collections.Generic;
using Nancy;
using Nancy.Bootstrapper;
using Prometheus;
using Prometheus.Advanced;

namespace Com.RFranco.Iris.NancyMetrics.Stats {

public interface INancyServiceStatsCollector
    {
        void RegisterMetrics(IEnumerable<IOnDemandCollector> collectors);
        void UpdateMetrics(IPipelines pipelines, NancyContext context);
    }

    public class NancyServiceStatsCollector : INancyServiceStatsCollector
    {
        private const string requestDateTimeNancyContextItem = "RequestDateTime";
        
        private Counter ProcessedRequests;
        private Counter ResponseError;
        private Gauge RequestsInProgress;
        private Histogram RequestsDuration;


        public NancyServiceStatsCollector(bool includeSystemStats=false) 
        {
            ProcessedRequests = Metrics.CreateCounter("total_successfull_requests", "Number of successfull processed requests.");
            ResponseError = Metrics.CreateCounter("total_error_responses", "Number of unsuccessfull processed requests.");
            RequestsInProgress = Metrics.CreateGauge("requests_in_progress", "Number of requests ongoing.");
            RequestsDuration = Metrics.CreateHistogram("requests_duration_seconds", "Histogram of request call processing durations.");

            if(includeSystemStats)  
                RegisterMetrics(new List<IOnDemandCollector> { new DotNetStatsCollector(), new PerfCounterCollector() });          
        } 
        
        public void RegisterMetrics(IEnumerable<IOnDemandCollector> collectors)
        {
            DefaultCollectorRegistry.Instance.RegisterOnDemandCollectors(collectors);
        }

        public void UpdateMetrics(IPipelines pipelines, NancyContext context)
        {
            pipelines.BeforeRequest.AddItemToStartOfPipeline((ctx) =>
            {
                RequestsInProgress.Inc();
                if (!ctx.Items.ContainsKey(requestDateTimeNancyContextItem)) ctx.Items[requestDateTimeNancyContextItem] = DateTime.UtcNow;
                return null;
            });

            pipelines.AfterRequest.AddItemToEndOfPipeline((ctx) =>
            {
                ProcessedRequests.Inc();
                RequestsDuration.Observe((int)(DateTime.UtcNow - (DateTime)ctx.Items[requestDateTimeNancyContextItem]).TotalSeconds);
                RequestsInProgress.Dec();
            });

            pipelines.OnError.AddItemToStartOfPipeline((ctx, ex) =>
            {
                ResponseError.Inc();
                RequestsDuration.Observe((int)(DateTime.UtcNow - (DateTime)ctx.Items[requestDateTimeNancyContextItem]).TotalSeconds);
                RequestsInProgress.Dec();
                return null;
            });
        }
    }

}