using System.Diagnostics;
using Titan.Application.Abstractions;
using Titan.Domain.Exceptions;
using ClusterAggregate = Titan.Domain.Aggregates.Platform.Cluster.Cluster;

namespace Titan.Infrastructure.Incus;

// Probes what the admin typed, before it is saved: GET /1.0 must come back "trusted" from the pinned server.
// A failure is a result, never an exception (https://github.com/OpenViCloud/titan/blob/main/docs/features/cluster/design.md#cluster-incus).
public sealed class IncusClusterConnectionTester(IncusClientFactory factory) : IClusterConnectionTester
{
    public async Task<ClusterTestResult> TestAsync(string endpoint, string serverFingerprint, CancellationToken ct)
    {
        ClusterAggregate candidate;
        try
        {
            // The aggregate's own rules decide what a valid endpoint and fingerprint are; nothing is stored.
            candidate = ClusterAggregate.Create("test", null, endpoint, serverFingerprint);
        }
        catch (ArgumentException e)
        {
            return new ClusterTestResult(false, null, e.Message);
        }

        var watch = Stopwatch.StartNew();
        try
        {
            using IncusClient client = await factory.CreateAsync(candidate.Endpoint, candidate.ServerFingerprint, ct);
            await client.RequireTrustedAsync(ct);
            return new ClusterTestResult(true, watch.ElapsedMilliseconds, null);
        }
        catch (ServiceUnavailableException e)
        {
            return new ClusterTestResult(false, null, e.Message);
        }
    }
}
