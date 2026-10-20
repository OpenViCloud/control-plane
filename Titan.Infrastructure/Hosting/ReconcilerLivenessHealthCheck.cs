using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Titan.Application.Options;

namespace Titan.Infrastructure.Hosting;

/// <summary>
/// Same shape as MeterLivenessHealthCheck: a reconciler that has never asked for its list is measured from
/// its registration.
/// </summary>
public sealed class ReconcilerLivenessHealthCheck(
    ReconcilerHeartbeat heartbeat,
    IOptions<ReconcilerLivenessOptions> options) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        HealthCheckResult result = Evaluate(DateTimeOffset.UtcNow, heartbeat.Snapshot(), options.Value.StaleMinutes);
        return Task.FromResult(result);
    }

    /// An empty snapshot is Healthy: nothing is expected yet, so there is no evidence of a stopped reconciler.
    public static HealthCheckResult Evaluate(
        DateTimeOffset now,
        IReadOnlyDictionary<string, ReconcilerPollState> snapshot,
        int staleMinutes)
    {
        TimeSpan staleWindow = TimeSpan.FromMinutes(staleMinutes);

        var stale = new List<string>();
        foreach (KeyValuePair<string, ReconcilerPollState> entry in snapshot)
        {
            DateTimeOffset reference = entry.Value.RegisteredAt;
            if (entry.Value.LastPollAt != null)
            {
                reference = entry.Value.LastPollAt.Value;
            }

            bool tooLongSinceLastPoll = now - reference > staleWindow;
            if (tooLongSinceLastPoll)
            {
                stale.Add(entry.Key);
            }
        }

        if (stale.Count == 0)
        {
            return HealthCheckResult.Healthy("every reconciler polled within the stale window");
        }

        stale.Sort(StringComparer.Ordinal);
        return HealthCheckResult.Unhealthy($"reconcilers not polling: {string.Join(", ", stale)}");
    }
}
