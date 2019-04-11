# NancyMetrics
Prometheus metrics instrumentation for NancyFx

## Why NancyMetrics

Its needed to improve the observability of the NancyFx-based components. Using this library, the NancyFx service publishs a `/metrics` endpoint from where retrieve the metrics related to the service and know the contextual status of the service.

## How To Use NancyMetrics

When your Nancy application uses an IoC container with auto-registration support, all you have to do is adding a reference to the [NancyMetrics Nuget project](https://www.nuget.org/packages/NancyMetrics) in order to add the `/metrics` endpoint to your NancyFx application.

If the `/metrics` endpoint are still unavailable after adding a reference to NancyMetrics in your project, you might have to explicitly register the Nancy module in your IoC container from your custom Nancy bootstrapper. For example, using Ninject:

```csharp
container.Bind<NancyModule>().To<MetricsModule>();
container.Bind<INancyServiceStatsCollector>().To<NancyServiceStatsCollector>();
```