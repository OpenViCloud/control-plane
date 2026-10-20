using Titan.Domain.Aggregates.Platform.Cluster;

namespace Titan.Tests.Domain;

// The Incus names a zone's cluster uses that Titan cannot guess: the OVN uplink network and the storage pool
// (docs/features/cluster/design.md#cluster-provider-settings). Null means "not set".
public class ClusterProviderSettingsTests
{
    [Fact]
    public void Create_KeepsTheValues()
    {
        var settings = ClusterProviderSettings.Create("ovnup", "default", "2001:41d0:d00:d400::/56");

        Assert.Equal("ovnup", settings.OvnUplinkNetwork);
        Assert.Equal("default", settings.StoragePool);
        Assert.Equal("2001:41d0:d00:d400::/56", settings.Ipv6Block);
    }

    // A blank form field is "not set", not an empty network name sent to Incus.
    [Fact]
    public void Create_BlankStrings_AreNotSet()
    {
        var settings = ClusterProviderSettings.Create("  ", "", " ");

        Assert.Null(settings.OvnUplinkNetwork);
        Assert.Null(settings.StoragePool);
        Assert.Null(settings.Ipv6Block);
    }

    [Fact]
    public void Create_TrimsTheNames()
    {
        var settings = ClusterProviderSettings.Create(" ovnup ", " default ", null);

        Assert.Equal("ovnup", settings.OvnUplinkNetwork);
        Assert.Equal("default", settings.StoragePool);
    }

    [Fact]
    public void None_HasNothingSet()
    {
        Assert.Null(ClusterProviderSettings.None.OvnUplinkNetwork);
        Assert.Null(ClusterProviderSettings.None.StoragePool);
        Assert.Null(ClusterProviderSettings.None.Ipv6Block);
    }

    [Fact]
    public void Create_AnIpv6BlockWrittenWithLeadingZeros_IsKeptInItsShortForm()
    {
        var settings = ClusterProviderSettings.Create("ovnup", "default", "2001:41d0:0d00:d400::/56");

        Assert.Equal("2001:41d0:d00:d400::/56", settings.Ipv6Block);
    }

    // The address plan numbers the /64s of a /56 (docs/infra/incus/design.md#ipv6-address-plan): anything else
    // would hand out /64s outside the block OVH routes.
    [Theory]
    [InlineData("10.0.0.0/8")]
    [InlineData("2001:41d0:d00:d400::/48")]
    [InlineData("2001:41d0:d00:d400::/64")]
    [InlineData("2001:41d0:d00:d401::/56")]
    [InlineData("not a block")]
    public void Create_AnIpv6BlockThatIsNotAnIpv6Slash56_IsRefused(string block)
    {
        var ex = Assert.Throws<Titan.Domain.Exceptions.BusinessException>(
            () => ClusterProviderSettings.Create("ovnup", "default", block));

        Assert.Equal("INVALID_IPV6_BLOCK", ex.Code);
    }
}
