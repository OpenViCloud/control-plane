using System.Collections.Concurrent;

namespace Titan.Application.Services.Billing.Metering;

/// <summary>
/// Written by <see cref="Titan.Infrastructure.Hosting.MeterHostedService"/> and read by /healthz/meters:
/// a meter whose ticks all fail records nothing and raises no visible error while the invoice silently
/// under-bills.
///
/// Singleton because the hosted service opens a fresh DI scope per tick. Concurrent because the health
/// check reads it off the request thread.
/// </summary>
public sealed class MeterHeartbeat
{
    private readonly ConcurrentDictionary<string, MeterTickState> _states = new();

    /// <summary>
    /// Declares a meter as expected, before it has ticked. Lets the health check distinguish
    /// "registered but never ticked yet" (grace at boot) from an unknown name. First registration
    /// wins — a later Record must not reset a meter's history back to never-ticked.
    /// </summary>
    public void Register(string name, DateTimeOffset at) =>
        _states.TryAdd(name, new MeterTickState(at, LastSuccessAt: null, LastAttemptAt: null));

    public void Record(string name, bool success, DateTimeOffset at) =>
        _states.AddOrUpdate(
            name,
            _ => new MeterTickState(RegisteredAt: at, LastSuccessAt: success ? at : null, LastAttemptAt: at),
            (_, prev) => prev with
            {
                LastSuccessAt = success ? at : prev.LastSuccessAt,
                LastAttemptAt = at
            });

    public IReadOnlyDictionary<string, MeterTickState> Snapshot() =>
        new Dictionary<string, MeterTickState>(_states);
}
