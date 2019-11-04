using System;
using Nancy;
using Nancy.Bootstrapper;
using Prometheus;

namespace Com.RFranco.Iris.NancyMetrics.Stats
{

    public class NancyServiceStatsCollector
    {
        private const string requestDateTimeNancyContextItem = "RequestDateTime";

        private static readonly Counter RequestsProcessed = Metrics.CreateCounter("total_successfull_requests", "Number of successfull processed requests.", "method");
        private static readonly Counter ErrorRequestsProcessed = Metrics.CreateCounter("total_error_responses", "Number of unsuccessfull processed requests.", "method", "code_error");
        private static readonly Gauge OngoingRequests = Metrics.CreateGauge("requests_in_progress", "Number of ongoing requests.", "method");
        private static readonly Summary RequestsDurationSummaryInSeconds = Metrics.CreateSummary("requests_duration_summary_seconds", "A Summary of request duration (in seconds) over last 10 minutes.", "method");
        private static readonly Histogram RequestResponseHistogram = Metrics.CreateHistogram("requests_duration_histogram_seconds", "Histogram of request duration in seconds.", "method");

        public static void UpdateMetrics(IPipelines pipelines, NancyContext context)
        {
            var fullMethodName = $"{context.Request.Method} {context.Request.Path}";
            pipelines.BeforeRequest.AddItemToStartOfPipeline((ctx) =>
            {
                OngoingRequests.Labels(fullMethodName).Inc();
                if (!ctx.Items.ContainsKey(requestDateTimeNancyContextItem)) ctx.Items[requestDateTimeNancyContextItem] = DateTime.UtcNow;
                return null;
            });

            pipelines.AfterRequest.AddItemToEndOfPipeline((ctx) =>
            {
                var now = DateTime.UtcNow;
                RequestsProcessed.Labels(fullMethodName).Inc();
                RequestsDurationSummaryInSeconds.Labels(fullMethodName).Observe((now - (DateTime)ctx.Items[requestDateTimeNancyContextItem]).TotalSeconds);
                RequestResponseHistogram.Labels(fullMethodName).Observe((now - (DateTime)ctx.Items[requestDateTimeNancyContextItem]).TotalSeconds);
                OngoingRequests.Labels(fullMethodName).Dec();
            });

            pipelines.OnError.AddItemToStartOfPipeline((ctx, ex) =>
            {
                var now = DateTime.UtcNow;
                ErrorRequestsProcessed.Labels(fullMethodName, context.Response.StatusCode.ToString()).Inc();
                RequestsDurationSummaryInSeconds.Labels(fullMethodName).Observe((now - (DateTime)ctx.Items[requestDateTimeNancyContextItem]).TotalSeconds);
                RequestResponseHistogram.Labels(fullMethodName).Observe((now - (DateTime)ctx.Items[requestDateTimeNancyContextItem]).TotalSeconds);
                OngoingRequests.Labels(fullMethodName).Dec();
                return null;
            });
        }
    }

}