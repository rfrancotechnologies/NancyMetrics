using System;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Routing;
using Prometheus;

namespace Com.RFranco.Iris.NancyMetrics.Stats
{

    public class NancyMetricsCollector
    {
        
        private IRouteResolver RouteResolver;
        private const string RequestDateTimeNancyContextItem = "RequestDateTime";

        private const string UNKNOWN_REQUEST_PATH = "NotFound";
        private static readonly Counter RequestsProcessed = Metrics.CreateCounter("http_requests_total", "Number of processed requests.", "method");
        private static readonly Counter ErrorRequestsProcessed = Metrics.CreateCounter("http_error_total", "Number of unsuccessfull processed requests.", "method", "error_code");
        private static readonly Gauge OngoingRequests = Metrics.CreateGauge("http_requests_in_progress", "Number of ongoing requests.", "method");
        private static readonly Summary RequestsDurationSummaryInSeconds = Metrics.CreateSummary("http_requests_duration_summary_seconds", "A Summary of request duration (in seconds) over last 10 minutes.", "method");
        private static readonly Histogram RequestResponseHistogram = Metrics.CreateHistogram("http_requests_duration_histogram_seconds", "Histogram of request duration in seconds.", "method");

        public NancyMetricsCollector(IRouteResolver routeResolver)
        {
            RouteResolver = routeResolver;
        }

        public void UpdateMetrics(IPipelines pipelines, NancyContext context)
        {

            string fullMethodName = GetRequestPathTemplate(context);

            pipelines.BeforeRequest.AddItemToStartOfPipeline((ctx) =>
            {
                OngoingRequests.Labels(fullMethodName).Inc();
                if (!ctx.Items.ContainsKey(RequestDateTimeNancyContextItem)) ctx.Items[RequestDateTimeNancyContextItem] = DateTime.UtcNow;
                return null;
            });

            pipelines.AfterRequest.AddItemToEndOfPipeline((ctx) =>
            {
                var now = DateTime.UtcNow;
                RequestsProcessed.Labels(fullMethodName).Inc();
                RequestsDurationSummaryInSeconds.Labels(fullMethodName).Observe((now - (DateTime)ctx.Items[RequestDateTimeNancyContextItem]).TotalSeconds);
                RequestResponseHistogram.Labels(fullMethodName).Observe((now - (DateTime)ctx.Items[RequestDateTimeNancyContextItem]).TotalSeconds);
                OngoingRequests.Labels(fullMethodName).Dec();
            });            
        }

        public void UpdateMetricsOnError(IPipelines pipelines, NancyContext context)
        {
            string fullMethodName = GetRequestPathTemplate(context);
            var now = DateTime.UtcNow;
            ErrorRequestsProcessed.Labels(fullMethodName, context.Response.StatusCode.ToString()).Inc();
            RequestsDurationSummaryInSeconds.Labels(fullMethodName).Observe((now - (DateTime)context.Items[RequestDateTimeNancyContextItem]).TotalSeconds);
            RequestResponseHistogram.Labels(fullMethodName).Observe((now - (DateTime)context.Items[RequestDateTimeNancyContextItem]).TotalSeconds);
            OngoingRequests.Labels(fullMethodName).Dec();
        }

        private string GetRequestPathTemplate(NancyContext context)
        {
            var resolveResult = RouteResolver.Resolve(context);
            return resolveResult.After == null ?
                UNKNOWN_REQUEST_PATH : $"{context.Request.Method} {resolveResult.Route.Description.Path}";
        }
    }

}