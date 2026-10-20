using FastEndpoints;
using Titan.Application.Abstractions;

namespace Titan.Infrastructure.Endpoints.Admin;

public record TestClusterRequest(string Endpoint, string ServerFingerprint);

// 200 either way: a failed probe is a result to render, not an HTTP error. Stores no cluster (Titan's own identity is
// generated on first use, as by any Incus call).
// Covered by IncusClusterConnectionTesterTests.
public class TestAdminClusterEndpoint(IClusterConnectionTester tester)
    : Endpoint<TestClusterRequest, ClusterTestResult>
{
    public override void Configure()
    {
        Post("v1/admin/clusters/test");
        Roles("platform-admin");
    }

    public override async Task HandleAsync(TestClusterRequest req, CancellationToken ct)
    {
        var result = await tester.TestAsync(req.Endpoint, req.ServerFingerprint, ct);
        await SendAsync(result, cancellation: ct);
    }
}
