namespace Titan.Application.Abstractions;

// Result of probing an Incus endpoint with a pinned server fingerprint. A failure is a normal result, not an
// exception — the admin UI renders it before deciding whether to save.
public record ClusterTestResult(bool Ok, long? LatencyMs, string? Error);

public interface IClusterConnectionTester
{
    Task<ClusterTestResult> TestAsync(string endpoint, string serverFingerprint, CancellationToken ct);
}
