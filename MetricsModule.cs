using System;
using Nancy;
using Prometheus;
using Prometheus.Advanced;

namespace Com.RFranco.Iris.NancyMetrics.Modules
{
    public class MetricsModule : NancyModule
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MetricsModule()
        {
            Get["/metrics"] = parameters =>
            {
                var response = new Response();
                response.ContentType = "text/html";

                try
                {
                    response.Contents = c =>
                        ScrapeHandler.ProcessScrapeRequest(DefaultCollectorRegistry.Instance.CollectAll(), "text/html", c);

                } catch(Exception e)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    log.Error("An error  occurrs retrieving metrics", e);
                }

                response.StatusCode = HttpStatusCode.OK;

                return response;
            };

        }
    }
}
