using System;
using System.IO;
using System.Threading.Tasks;
using Nancy;
using Prometheus;

namespace Com.RFranco.Iris.NancyMetrics.Modules
{
    public class MetricsModule : NancyModule
    {
        public MetricsModule()
        {
            Get("/metrics", _ => Response.AsText(GetMetrics().Result));

        }

        private async Task<string> GetMetrics()
        {
            using (var ms = new MemoryStream())
            {
                await Metrics.DefaultRegistry.CollectAndExportAsTextAsync(ms);
                return System.Text.Encoding.UTF8.GetString(ms.ToArray()); 
            }            
        }

    }
}
