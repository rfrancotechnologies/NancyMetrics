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
        private static readonly Counter ErrorRequestsProcessed = Metrics.CreateCounter("http_error_total", "Number of unsuccessfull processed requests.", "method", "error_code");
        private static readonly Gauge OngoingRequests = Metrics.CreateGauge("http_requests_in_progress", "Number of ongoing requests.", "method");
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
                RequestResponseHistogram.Labels(fullMethodName).Observe((DateTime.UtcNow - (DateTime)ctx.Items[RequestDateTimeNancyContextItem]).TotalSeconds);
                OngoingRequests.Labels(fullMethodName).Dec();
            });            
        }

        public void UpdateMetricsOnError(IPipelines pipelines, NancyContext context)
        {
            string fullMethodName = GetRequestPathTemplate(context);
            
            ErrorRequestsProcessed.Labels(fullMethodName, ((int)context.Response.StatusCode).ToString()).Inc();
            RequestResponseHistogram.Labels(fullMethodName).Observe((DateTime.UtcNow - (DateTime)context.Items[RequestDateTimeNancyContextItem]).TotalSeconds);
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