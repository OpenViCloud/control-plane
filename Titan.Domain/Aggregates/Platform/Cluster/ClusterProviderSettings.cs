using Titan.Domain.Exceptions;

namespace Titan.Domain.Aggregates.Platform.Cluster;

// The Incus names a zone's cluster uses that Titan cannot guess (https://github.com/OpenViCloud/titan-ops/blob/main/docs/features/cluster/design.md#cluster-provider-settings).
// `{ get; private init; }`, not a positional record: a public `with` would skip Create (the TenantQuota rule).
public sealed record ClusterProviderSettings
{
    /// The OVN uplink network tenant networks attach to (https://github.com/OpenViCloud/titan-ops/blob/main/docs/infra/incus/design.md#network).
    public string? OvnUplinkNetwork { get; private init; }

    public string? StoragePool { get; private init; }

    /// The zone's public IPv6 /56: its /64s go to networks that ask for public IPv6
    /// (https://github.com/OpenViCloud/titan-ops/blob/main/docs/infra/incus/design.md#ipv6-address-plan). Null = the zone has no public IPv6.
    public string? Ipv6Block { get; private init; }

    private ClusterProviderSettings() { }

    public static readonly ClusterProviderSettings None = new();

    public static ClusterProviderSettings Create(string? ovnUplinkNetwork, string? storagePool, string? ipv6Block)
    {
        string? block = NotSetWhenBlank(ipv6Block);
        if (block != null && !IsIpv6Block(block))
        {
            throw new BusinessException("INVALID_IPV6_BLOCK", "The IPv6 block must be an IPv6 /56, written as its first address.");
        }
        return new ClusterProviderSettings
        {
            OvnUplinkNetwork = NotSetWhenBlank(ovnUplinkNetwork),
            StoragePool = NotSetWhenBlank(storagePool),
            Ipv6Block = block == null ? null : System.Net.IPNetwork.Parse(block).ToString(),
        };
    }

    // The address plan numbers the 256 /64s of a /56; a block written from a host inside it would shift every /64.
    public static bool IsIpv6Block(string block)
    {
        bool parsed = System.Net.IPNetwork.TryParse(block, out var network);
        if (!parsed)
        {
            return false;
        }
        bool isIpv6 = network.BaseAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
        return isIpv6 && network.PrefixLength == 56;
    }

    // A blank form field is "not set", not an empty name sent to Incus.
    private static string? NotSetWhenBlank(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        return value.Trim();
    }
}
