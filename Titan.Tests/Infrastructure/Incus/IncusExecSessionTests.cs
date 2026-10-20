using System.Net.WebSockets;
using System.Text;
using Titan.Domain.Exceptions;
using Titan.Infrastructure.Incus;
using ClusterAggregate = Titan.Domain.Aggregates.Platform.Cluster.Cluster;

namespace Titan.Tests.Infrastructure.Incus;

// Opening an SSH session's exec on the zone's Incus over mutual TLS (docs/features/vm/design.md#ssh-session): the
// same client certificate and pinned server fingerprint as every other call, then the four websockets of the
// operation. Each Incus refusal becomes the code the customer can act on.
public class IncusExecSessionTests
{
    private static async Task<(IncusShellConnector Connector, StubClusterRepository Rows, string TitanFingerprint)> Connector()
    {
        var store = IncusTestKit.IdentityStore();
        var titan = await store.GetCertificateAsync(CancellationToken.None);
        var rows = new StubClusterRepository();
        return (new IncusShellConnector(rows, new IncusClientFactory(store)), rows, titan.Fingerprint);
    }

    private static async Task<string> ReadTextAsync(WebSocket socket, int length)
    {
        var buffer = new byte[length];
        int read = 0;
        while (read < length)
        {
            WebSocketReceiveResult received = await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer, read, length - read), CancellationToken.None);
            read += received.Count;
        }
        return Encoding.ASCII.GetString(buffer);
    }

    [Fact]
    public async Task OpenSsh_ExecsNcInTheInstance_AndConnectsTheFourChannels()
    {
        var (connector, rows, titan) = await Connector();
        await using var server = await FakeIncusExecServer.StartAsync(titan);
        rows.Rows.Add(ClusterAggregate.Create("local-mac", null, server.Endpoint, server.Fingerprint));

        await using IncusExecSession session = await connector.OpenSshAsync("local-mac", "t-abc", "vmxyz", CancellationToken.None);

        string request = Assert.Single(server.ExecRequests);
        Assert.StartsWith("/1.0/instances/vmxyz/exec?project=t-abc ", request);
        Assert.Contains("\"command\":[\"nc\",\"-N\",\"127.0.0.1\",\"22\"]", request);
        Assert.Contains("\"wait-for-websocket\":true", request);
        Assert.Contains("\"interactive\":false", request);
        Assert.Equal(FakeIncusExecServer.Banner, await ReadTextAsync(session.Stdout, FakeIncusExecServer.Banner.Length));
    }

    [Fact]
    public async Task OpenSsh_WhenIncusSaysTheInstanceIsNotRunning_IsVmInvalidStatus()
    {
        var (connector, rows, titan) = await Connector();
        await using var server = await FakeIncusExecServer.StartAsync(titan);
        server.Behaviour = FakeIncusExecServer.Mode.InstanceNotRunning;
        rows.Rows.Add(ClusterAggregate.Create("local-mac", null, server.Endpoint, server.Fingerprint));

        var refused = await Assert.ThrowsAsync<BusinessException>(() =>
            connector.OpenSshAsync("local-mac", "t-abc", "vmxyz", CancellationToken.None));

        Assert.Equal("VM_INVALID_STATUS", refused.Code);
    }

    // A 404 says nothing about the VM: a server without the exec route answers it too.
    [Fact]
    public async Task OpenSsh_WhenTheClusterHasNoExecRoute_IsAClusterError()
    {
        var (connector, rows, titan) = await Connector();
        await using var server = await FakeIncusExecServer.StartAsync(titan);
        server.Behaviour = FakeIncusExecServer.Mode.NoExecRoute;
        rows.Rows.Add(ClusterAggregate.Create("local-mac", null, server.Endpoint, server.Fingerprint));

        var refused = await Assert.ThrowsAsync<ServiceUnavailableException>(() =>
            connector.OpenSshAsync("local-mac", "t-abc", "vmxyz", CancellationToken.None));

        Assert.Equal("CLUSTER_ERROR", refused.Code);
    }

    [Fact]
    public async Task OpenSsh_WhenTheAgentIsNotRunning_IsVmAgentNotReady()
    {
        var (connector, rows, titan) = await Connector();
        await using var server = await FakeIncusExecServer.StartAsync(titan);
        server.Behaviour = FakeIncusExecServer.Mode.AgentNotRunning;
        rows.Rows.Add(ClusterAggregate.Create("local-mac", null, server.Endpoint, server.Fingerprint));

        var refused = await Assert.ThrowsAsync<BusinessException>(() =>
            connector.OpenSshAsync("local-mac", "t-abc", "vmxyz", CancellationToken.None));

        Assert.Equal("VM_AGENT_NOT_READY", refused.Code);
    }

    [Fact]
    public async Task OpenSsh_WhenTheClusterDoesNotTrustTitan_IsClusterUntrusted()
    {
        var (connector, rows, _) = await Connector();
        await using var server = await FakeIncusExecServer.StartAsync();
        rows.Rows.Add(ClusterAggregate.Create("local-mac", null, server.Endpoint, server.Fingerprint));

        var refused = await Assert.ThrowsAsync<ServiceUnavailableException>(() =>
            connector.OpenSshAsync("local-mac", "t-abc", "vmxyz", CancellationToken.None));

        Assert.Equal("CLUSTER_UNTRUSTED", refused.Code);
        Assert.Empty(server.ExecRequests);
    }

    [Fact]
    public async Task OpenSsh_WhenTheServerIsNotThePinnedOne_IsFingerprintMismatch()
    {
        var (connector, rows, titan) = await Connector();
        await using var server = await FakeIncusExecServer.StartAsync(titan);
        rows.Rows.Add(ClusterAggregate.Create("local-mac", null, server.Endpoint, new string('0', 64)));

        var refused = await Assert.ThrowsAsync<ServiceUnavailableException>(() =>
            connector.OpenSshAsync("local-mac", "t-abc", "vmxyz", CancellationToken.None));

        Assert.Equal("CLUSTER_FINGERPRINT_MISMATCH", refused.Code);
        Assert.Empty(server.ExecRequests);
    }

    // A cluster that takes the exec and never answers the upgrade would hold the customer, and one of the tenant's
    // session places, for as long as the connection stays up.
    [Fact]
    public async Task OpenSsh_WhenTheUpgradeNeverAnswers_GivesUpAsUnreachable()
    {
        var store = IncusTestKit.IdentityStore();
        var titan = await store.GetCertificateAsync(CancellationToken.None);
        var rows = new StubClusterRepository();
        await using var server = await FakeIncusExecServer.StartAsync(titan.Fingerprint);
        server.Behaviour = FakeIncusExecServer.Mode.StallsUpgrade;
        rows.Rows.Add(ClusterAggregate.Create("local-mac", null, server.Endpoint, server.Fingerprint));
        var connector = new IncusShellConnector(rows, new IncusClientFactory(store)) { ConnectTimeout = TimeSpan.FromSeconds(1) };

        var refused = await Assert.ThrowsAsync<ServiceUnavailableException>(() =>
            connector.OpenSshAsync("local-mac", "t-abc", "vmxyz", CancellationToken.None)).WaitAsync(TimeSpan.FromSeconds(20));

        Assert.Equal("CLUSTER_UNREACHABLE", refused.Code);
    }

    [Fact]
    public async Task OpenSsh_InAZoneWithNoCluster_IsClusterNotFound()
    {
        var (connector, _, _) = await Connector();

        var refused = await Assert.ThrowsAsync<NotFoundException>(() =>
            connector.OpenSshAsync("nowhere", "t-abc", "vmxyz", CancellationToken.None));

        Assert.Equal("CLUSTER_NOT_FOUND", refused.Code);
    }
}
