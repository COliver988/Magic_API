using Prometheus;

public static class HistogramMetrics
{
    public static readonly Histogram LegacyLoadDuration = Prometheus.Metrics
        .CreateHistogram(
            "legacy_data_load_seconds",
            "Time spent loading legacy data",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.01, 2, 10),
                LabelNames = new[] { "method" }
            });
}