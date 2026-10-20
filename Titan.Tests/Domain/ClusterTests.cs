using Titan.Domain.Aggregates.Platform.Cluster;

namespace Titan.Tests.Domain;

// An Incus cluster is reached at an https endpoint whose server certificate Titan pins by SHA-256 fingerprint
// (https://github.com/OpenViCloud/titan/blob/main/docs/features/cluster/design.md#cluster-incus). The fingerprint is compared byte for byte at every
// handshake, so it is stored in one canonical form however the admin pasted it.
public class ClusterTests
{
    private const string Fingerprint = "ffcf7803b2371e90834c2dd2a454f48bcb9c541b16dd7dc54a93a104b1aa108f";

    [Fact]
    public void Create_StampsIdentityAndTimestamps()
    {
        var cluster = Cluster.Create("mock", "incus-lab", "https://incus-lab:8443", Fingerprint);

        Assert.NotEqual(Guid.Empty, cluster.Id);
        Assert.Equal("mock", cluster.ZoneId);
        Assert.Equal("incus-lab", cluster.Name);
        Assert.Equal("https://incus-lab:8443", cluster.Endpoint);
        Assert.Equal(Fingerprint, cluster.ServerFingerprint);
        Assert.Equal(cluster.CreatedAt, cluster.UpdatedAt);
        Assert.NotEqual(default, cluster.CreatedAt);
    }

    [Fact]
    public void Create_BlankName_DefaultsToZoneId()
    {
        var cluster = Cluster.Create("mock", " ", "https://incus-lab:8443", Fingerprint);

        Assert.Equal("mock", cluster.Name);
    }

    [Theory]
    [InlineData("https://incus-lab:8443/", "https://incus-lab:8443")]
    [InlineData("https://incus-lab:8443//", "https://incus-lab:8443")]
    [InlineData("HTTPS://incus-lab:8443", "HTTPS://incus-lab:8443")]
    public void Create_DropsTrailingSlashes(string input, string expected)
    {
        var cluster = Cluster.Create("mock", null, input, Fingerprint);

        Assert.Equal(expected, cluster.Endpoint);
    }

    // `incus remote add` and openssl print the fingerprint with colons and upper case; the handshake compares hex.
    [Theory]
    [InlineData("FFCF7803B2371E90834C2DD2A454F48BCB9C541B16DD7DC54A93A104B1AA108F")]
    [InlineData("ff:cf:78:03:b2:37:1e:90:83:4c:2d:d2:a4:54:f4:8b:cb:9c:54:1b:16:dd:7d:c5:4a:93:a1:04:b1:aa:10:8f")]
    [InlineData("  ffcf7803b2371e90834c2dd2a454f48bcb9c541b16dd7dc54a93a104b1aa108f  ")]
    public void Create_StoresTheFingerprintAs64LowerCaseHex(string pasted)
    {
        var cluster = Cluster.Create("mock", null, "https://incus-lab:8443", pasted);

        Assert.Equal(Fingerprint, cluster.ServerFingerprint);
    }

    [Theory]
    [InlineData("http://incus-lab:8443")]
    [InlineData("incus-lab:8443")]
    [InlineData("https://")]
    [InlineData("")]
    public void Create_EndpointThatIsNotAnHttpsUrl_IsRefused(string endpoint)
    {
        Assert.Throws<ArgumentException>(() => Cluster.Create("mock", null, endpoint, Fingerprint));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ffcf7803")]
    [InlineData("zzcf7803b2371e90834c2dd2a454f48bcb9c541b16dd7dc54a93a104b1aa108f")]
    [InlineData("ffcf7803b2371e90834c2dd2a454f48bcb9c541b16dd7dc54a93a104b1aa108f00")]
    public void Create_FingerprintThatIsNotASha256_IsRefused(string fingerprint)
    {
        Assert.Throws<ArgumentException>(() => Cluster.Create("mock", null, "https://incus-lab:8443", fingerprint));
    }

    [Fact]
    public void Create_BlankZone_IsRefused()
    {
        Assert.Throws<ArgumentException>(() => Cluster.Create(" ", null, "https://incus-lab:8443", Fingerprint));
    }

    [Fact]
    public void UpdateConnection_ChangesFields_AndBumpsUpdatedAt()
    {
        var cluster = Cluster.Create("mock", "old", "https://old:8443", Fingerprint);
        var before = cluster.UpdatedAt;
        Thread.Sleep(5);

        cluster.UpdateConnection("new", "https://new:8443/", Fingerprint.ToUpperInvariant());

        Assert.Equal("new", cluster.Name);
        Assert.Equal("https://new:8443", cluster.Endpoint);
        Assert.Equal(Fingerprint, cluster.ServerFingerprint);
        Assert.True(cluster.UpdatedAt > before);
        Assert.Equal("mock", cluster.ZoneId);
    }

    [Fact]
    public void UpdateConnection_WithABadFingerprint_ChangesNothing()
    {
        var cluster = Cluster.Create("mock", "old", "https://old:8443", Fingerprint);

        Assert.Throws<ArgumentException>(() => cluster.UpdateConnection("new", "https://new:8443", "nope"));

        Assert.Equal("old", cluster.Name);
        Assert.Equal("https://old:8443", cluster.Endpoint);
    }
}
