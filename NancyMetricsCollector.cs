using System;
using System.Linq;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Routing;
using Prometheus;

namespace Com.RFranco.Iris.NancyMetrics.Stats
{

    public class NancyMetricsCollector
    {

        private IRouteResolver RouteResolver;
        private NancyMetricsCollectorOptions Options;

        private const string RequestDateTimeNancyContextItem = "RequestDateTime";

        private const string UNKNOWN_REQUEST_PATH = "NotFound";
        private Counter ErrorRequestsProcessed;
        private Gauge OngoingRequests;
        private Histogram RequestResponseHistogram;

        public NancyMetricsCollector(IRouteResolver routeResolver, NancyMetricsCollectorOptions options = null)
        {
            RouteResolver = routeResolver;
            Options = options ?? new NancyMetricsCollectorOptions();
            
            ErrorRequestsProcessed = Metrics.CreateCounter("server_request_error_total", "Number of unsuccessfull processed requests.", "method", "error_code");
            OngoingRequests = Metrics.CreateGauge("server_request_in_progress", "Number of ongoing requests.", "method");
            RequestResponseHistogram = Metrics.CreateHistogram("server_request_duration_seconds", "Histogram of request duration in seconds.",
                new HistogramConfiguration() {
                    LabelNames = new string[]{"method"},
                    Buckets = Options.Buckets
                });           
        }


        public void UpdateMetrics(IPipelines pipelines, NancyContext context)
        {
            if (MustBeObserved(context))
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
        }

        public void UpdateMetricsOnError(IPipelines pipelines, NancyContext context)
        {
            if (MustBeObserved(context))
            {
                string fullMethodName = GetRequestPathTemplate(context);

                if((int)context.Response.StatusCode >= 400)
                    ErrorRequestsProcessed.Labels(fullMethodName, ((int)context.Response.StatusCode).ToString()).Inc();

                RequestResponseHistogram.Labels(fullMethodName).Observe((DateTime.UtcNow - (DateTime)context.Items[RequestDateTimeNancyContextItem]).TotalSeconds);
                OngoingRequests.Labels(fullMethodName).Dec();
            }
        }

        private bool MustBeObserved(NancyContext context)
        {
            return Options.ExcludedPaths.Where(path => context.Request.Path.IndexOf(path) != -1).Count() == 0;
        }

        private string GetRequestPathTemplate(NancyContext context)
        {
            var resolveResult = RouteResolver.Resolve(context);
            return resolveResult.After == null ?
                UNKNOWN_REQUEST_PATH : $"{context.Request.Method} {resolveResult.Route.Description.Path}";
        }
    }

}