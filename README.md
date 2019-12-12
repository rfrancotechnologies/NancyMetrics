# NancyMetrics

Prometheus metrics instrumentation on [NancyFx 2.x](http://nancyfx.org/) applications.

## Why NancyMetrics

Its needed to improve the observability of the NancyFx-based components.
Using this library, the NancyFx service publishs a `/metrics` endpoint from where retrieve the metrics related to the service in order to know its contextual status allowing a real-time monitoring, apply horizontal autoscaling and so on.

## How To Use NancyMetrics

To `expose the metrics`, all you have to do is adding a reference to the [NancyMetrics Nuget project](https://www.nuget.org/packages/NancyMetrics) in order to add the `/metrics` endpoint to your NancyFx application. [Nancy will scan and identify all types that are descendants of the NancyModule type](https://github.com/NancyFx/Nancy/wiki/Exploring-the-nancy-module "Modules are globally discovered") for you.

Custom metrics defined:
    - Number of total processed requests per method (path template).
    - Number of unsuccessfull processed requests per method (path template) and error code.
    - Number of ongoing requests per method (path template).
    - A Summary of request duration in seconds over last 10 minutes per method path template).
    - A histogram of HTTP requests duration in seconds per method (path template).

To `update the metrics` exposed, you should:

1) Register `NancyMetricsCollector` on `ApplicationStartup` method:

```csharp
protected override void ApplicationStartup(IKernel container, IPipelines pipelines)
        {
            ...
            container.Bind<NancyMetricsCollector>().ToConstant(new NancyMetricsCollector(container.Get<IRouteResolver>()));
            ...
        }
```

Its also possible to configure the middleware overriding the default behavior (ignoring calls that contains`/swagger`, `/health` and `/metrics` in the path) passing a list of path to be ignored as parameters (in the next example, we are excluding calls that contains `/swagger` in the path):

```csharp
protected override void ApplicationStartup(IKernel container, IPipelines pipelines)
        {
            ...
            container.Bind<NancyMetricsCollector>().ToConstant(new NancyMetricsCollector(container.Get<IRouteResolver>()),
                new NancyMetricsCollectorOptions{ExcludedPaths = new List<string>{"/swagger"}});
            ...
        }
```

Also is possible to customize the buckets of the internal histogram used:

```csharp
protected override void ApplicationStartup(IKernel container, IPipelines pipelines)
        {
            ...
            container.Bind<NancyMetricsCollector>().ToConstant(new NancyMetricsCollector(container.Get<IRouteResolver>()),
                new NancyMetricsCollectorOptions{Buckets = new double[]{ .005, .01, .025, .05, .075, .1, .25, .5, .75, 1, 2.5, 5, 7.5, 10 }});
            ...
        }
```

By default, it uses the next list of buckets `{ .005, .01, .025, .05, .075, .1, .25, .5, .75, 1, 2.5, 5, 7.5, 10 }`.

2)Call `UpdateMetrics(...)` or `UpdateMetricsOnError(...)` from  `RequestStartup` method of your bootstrapper

```csharp
protected override void RequestStartup(IKernel requestContainer, IPipelines pipelines, NancyContext context)
{
    ...
    requestContainer.Get<NancyMetricsCollector>().UpdateMetrics(pipelines, context);
    ...
    //  if error
    requestContainer.Get<NancyMetricsCollector>().UpdateMetricsOnError(pipelines, context);
    ...
}
```

An example of `/metrics` response:

```text
# HELP requests_in_progress Number of requests ongoing.
# TYPE requests_in_progress GAUGE
requests_in_progress 0
# HELP requests_duration_seconds Histogram of request call processing durations.
# TYPE requests_duration_seconds HISTOGRAM
requests_duration_seconds_sum 0
requests_duration_seconds_count 1
requests_duration_seconds_bucket{le="0.005"} 1
requests_duration_seconds_bucket{le="0.01"} 1
requests_duration_seconds_bucket{le="0.025"} 1
requests_duration_seconds_bucket{le="0.05"} 1
requests_duration_seconds_bucket{le="0.075"} 1
requests_duration_seconds_bucket{le="0.1"} 1
requests_duration_seconds_bucket{le="0.25"} 1
requests_duration_seconds_bucket{le="0.5"} 1
requests_duration_seconds_bucket{le="0.75"} 1
requests_duration_seconds_bucket{le="1"} 1
requests_duration_seconds_bucket{le="2.5"} 1
requests_duration_seconds_bucket{le="5"} 1
requests_duration_seconds_bucket{le="7.5"} 1
requests_duration_seconds_bucket{le="10"} 1
requests_duration_seconds_bucket{le="+Inf"} 1
# HELP total_error_responses Number of unsuccessfull processed requests.
# TYPE total_error_responses COUNTER
# HELP total_successfull_requests Number of successfull processed requests.
# TYPE total_successfull_requests COUNTER
total_successfull_requests 1
```

### Path Template

`NancyMetricsCollector` requires the `IRouteResolver` instance to avoid unbounded request path label.
Instead of use the request URL path, internally uses the path template defined if exists (in other case, register the operation with the `NotFound` label).
