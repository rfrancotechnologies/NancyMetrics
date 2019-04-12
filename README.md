# NancyMetrics
Prometheus metrics instrumentation for NancyFx

## Why NancyMetrics

Its needed to improve the observability of the NancyFx-based components. Using this library, the NancyFx service publishs a `/metrics` endpoint from where retrieve the metrics related to the service and know the contextual status of the service.

## How To Use NancyMetrics

When your Nancy application uses an IoC container with auto-registration support, all you have to do is adding a reference to the [NancyMetrics Nuget project](https://www.nuget.org/packages/NancyMetrics) in order to add the `/metrics` endpoint to your NancyFx application.

If the `/metrics` endpoint are still unavailable after adding a reference to NancyMetrics in your project, you might have to explicitly register the Nancy module in your IoC container from your custom Nancy bootstrapper. For example, using Ninject:

```csharp
container.Bind<NancyModule>().To<MetricsModule>();
```

To collect metrics to be published, the library provide a custom implementation of INancyServiceStatsCollector, NancyServiceStatsCollector that its needed to be expliocitily register i your IoC container.

```csharp
container.Bind<INancyServiceStatsCollector>().To<NancyServiceStatsCollector>();
```

By default, NancyServiceStatsCollector doesnt include system stats and include custom metrics defined:
    - Number of HTTP requests in progress.
    - Total number of received HTTP requests.
    - Duration of HTTP requests.

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

To include system stats as cpu and  memory usage or process information, its needed to instance NancyServiceStatsCollector in this way:

```csharp
container.Bind<INancyServiceStatsCollector>().ToConstant(new NancyServiceStatsCollector(true));
```

An example of `/metrics` response including system stats:

```text
# HELP dotnet_totalmemory Total known allocated memory
# TYPE dotnet_totalmemory GAUGE
dotnet_totalmemory 13265328
# HELP dotnet_collection_errors_total Total number of errors that occured during collections
# TYPE dotnet_collection_errors_total COUNTER
# HELP process_start_time_seconds Start time of the process since unix epoch in seconds
# TYPE process_start_time_seconds GAUGE
process_start_time_seconds 1555050311.60261
# HELP process_pct_processor_time % Processor Time Perf Counter
# TYPE process_pct_processor_time GAUGE
process_pct_processor_time 0
# HELP total_error_responses Number of unsuccessfull processed requests.
# TYPE total_error_responses COUNTER
# HELP process_cpu_seconds_total Total user and system CPU time spent in seconds
# TYPE process_cpu_seconds_total COUNTER
process_cpu_seconds_total 5.390625
# HELP process_working_set Working Set Perf Counter
# TYPE process_working_set GAUGE
process_working_set 100315136
# HELP process_private_bytes Private Bytes Perf Counter
# TYPE process_private_bytes GAUGE
process_private_bytes 66187264
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
# HELP dotnet_clr_memory_pct_time_in_gc % Time in GC Perf Counter
# TYPE dotnet_clr_memory_pct_time_in_gc GAUGE
dotnet_clr_memory_pct_time_in_gc 0.549717128276825
# HELP dotnet_clr_memory_gen_0_heap_size Gen 0 heap size Perf Counter
# TYPE dotnet_clr_memory_gen_0_heap_size GAUGE
dotnet_clr_memory_gen_0_heap_size 2097152
# HELP process_windows_private_bytes Process private memory size
# TYPE process_windows_private_bytes GAUGE
process_windows_private_bytes 65568768
# HELP dotnet_clr_memory_gen_2_heap_size Gen 2 heap size Perf Counter
# TYPE dotnet_clr_memory_gen_2_heap_size GAUGE
dotnet_clr_memory_gen_2_heap_size 5620464
# HELP process_windows_open_handles Number of open handles
# TYPE process_windows_open_handles GAUGE
process_windows_open_handles 1019
# HELP requests_in_progress Number of requests ongoing.
# TYPE requests_in_progress GAUGE
requests_in_progress 0
# HELP total_successfull_requests Number of successfull processed requests.
# TYPE total_successfull_requests COUNTER
total_successfull_requests 1
# HELP performance_counter_errors_total Total number of errors that occured during performance counter collections
# TYPE performance_counter_errors_total COUNTER
# HELP dotnet_clr_memory_gen_1_heap_size Gen 1 heap size Perf Counter
# TYPE dotnet_clr_memory_gen_1_heap_size GAUGE
dotnet_clr_memory_gen_1_heap_size 1107032
# HELP process_windows_num_threads Total number of threads
# TYPE process_windows_num_threads GAUGE
process_windows_num_threads 28
# HELP process_windows_working_set Process working set
# TYPE process_windows_working_set GAUGE
process_windows_working_set 98828288
# HELP process_windows_virtual_bytes Process virtual memory size
# TYPE process_windows_virtual_bytes GAUGE
process_windows_virtual_bytes 315392000
# HELP dotnet_collection_count_total GC collection count
# TYPE dotnet_collection_count_total COUNTER
dotnet_collection_count_total{generation="2"} 2
dotnet_collection_count_total{generation="0"} 32
dotnet_collection_count_total{generation="1"} 9
# HELP process_windows_processid Process ID
# TYPE process_windows_processid GAUGE
process_windows_processid 19624
# HELP dotnet_clr_memory_large_object_heap_size Large Object Heap size Perf Counter
# TYPE dotnet_clr_memory_large_object_heap_size GAUGE
dotnet_clr_memory_large_object_heap_size 5701240
# HELP process_virtual_bytes Virtual Bytes Perf Counter
# TYPE process_virtual_bytes GAUGE
process_virtual_bytes 317530112
```

It also possible to add custom collectors using `RegisterMetrics(IEnumerable<IOnDemandCollector> collectors)` method or defining a new implementation of INancyServiceStatsCollector.