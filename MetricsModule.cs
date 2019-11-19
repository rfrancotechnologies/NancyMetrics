using System;
using System.IO;
using Nancy;
using Prometheus;

namespace Com.RFranco.Iris.NancyMetrics.Modules
{
    public class MetricsModule : NancyModule
    {
        public MetricsModule()
        {
            Get("/metrics", _ => 
            {
                var response = new Response();
                response.ContentType = "text/html";
                try
                {
                    response.Contents = async s =>
                    {
                        await Metrics.DefaultRegistry.CollectAndExportAsTextAsync(s);
                    };
                }
                catch (Exception e)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.Contents = s =>
                    {
                        using (var writer = new StreamWriter(s))
                            writer.Write(e.Message);
                    };
                }

                response.StatusCode = HttpStatusCode.OK;

                return response;
            });

        }
    }
}
