using Microsoft.Extensions.Options;
using Titan.Application.Options;
using Titan.Domain.Aggregates.Platform.ProviderClientIdentity;
using Titan.Domain.Repositories.Platform;
using Titan.Infrastructure.Incus;
using ClusterAggregate = Titan.Domain.Aggregates.Platform.Cluster.Cluster;

namespace Titan.Tests.Infrastructure.Incus;

internal static class IncusTestKit
{
    public static AesGcmClusterCredentialCipher Cipher(int keySeed = 0) =>
        new(Options.Create(new ClusterEncryptionOptions
        {
            ClusterKey = Convert.ToBase64String(Enumerable.Range(keySeed, 32).Select(i => (byte)i).ToArray()),
        }));

    // A fresh cache each time: a store built here behaves like a newly started control plane.
    public static ProviderClientIdentityStore IdentityStore(StubIdentityRepository? repository = null) =>
        new(repository ?? new StubIdentityRepository(), Cipher(), new ProviderClientCertificateCache());

    public static ZoneOptions Zones(params string[] ids) =>
        new() { Zones = ids.Select(id => new ZoneConfig { Id = id, Name = id }).ToList() };
}

internal sealed class StubIdentityRepository : IProviderClientIdentityRepository
{
    public ProviderClientIdentity? Stored { get; set; }

    /// Set to make TryAddAsync lose the race to this identity, the way a concurrent writer would.
    public ProviderClientIdentity? WinnerOfARace { get; set; }

    public int Adds { get; private set; }

    public Task<ProviderClientIdentity?> FindAsync(CancellationToken ct) => Task.FromResult(Stored);

    public Task<bool> TryAddAsync(ProviderClientIdentity identity, CancellationToken ct)
    {
        Adds++;
        if (WinnerOfARace != null)
        {
            Stored = WinnerOfARace;
            return Task.FromResult(false);
        }
        Stored = identity;
        return Task.FromResult(true);
    }
}

internal sealed class StubClusterRepository : IClusterRepository
{
    public List<ClusterAggregate> Rows { get; } = new();

    public Task<ClusterAggregate?> FindByZoneIdAsync(string zoneId, CancellationToken ct) =>
        Task.FromResult(Rows.FirstOrDefault(r => r.ZoneId == zoneId));

    public Task<IReadOnlyList<ClusterAggregate>> GetAllAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<ClusterAggregate>>(Rows.ToList());

    public Task<ClusterAggregate?> GetForUpdateAsync(string zoneId, CancellationToken ct) =>
        Task.FromResult(Rows.FirstOrDefault(r => r.ZoneId == zoneId));

    public Task AddAsync(ClusterAggregate cluster, CancellationToken ct)
    {
        Rows.Add(cluster);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(ClusterAggregate cluster, CancellationToken ct)
    {
        Rows.Remove(cluster);
        return Task.CompletedTask;
    }

    public int Saves { get; private set; }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        Saves++;
        return Task.CompletedTask;
    }
}
