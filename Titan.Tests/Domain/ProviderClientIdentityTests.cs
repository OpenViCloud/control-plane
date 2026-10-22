using System.Text;
using Titan.Domain.Aggregates.Platform.ProviderClientIdentity;

namespace Titan.Tests.Domain;

// Titan's own TLS client identity toward every Incus cluster (https://github.com/OpenViCloud/titan-ops/blob/main/docs/features/cluster/design.md#provider-client-identity).
// One row with a fixed key: a second identity would leave every cluster that trusts the first one refusing Titan.
public class ProviderClientIdentityTests
{
    private const string CertificatePem = "-----BEGIN CERTIFICATE-----\nMIIB\n-----END CERTIFICATE-----\n";
    private const string Fingerprint = "d5d7e5097b92f32181a74c286eccbda67c869ce881ed8273fbd223eeade2420e";
    private static readonly byte[] KeyCipher = Encoding.UTF8.GetBytes("cipher");

    [Fact]
    public void Create_UsesTheOneFixedId()
    {
        var first = ProviderClientIdentity.Create(CertificatePem, KeyCipher, Fingerprint);
        var second = ProviderClientIdentity.Create(CertificatePem, KeyCipher, Fingerprint);

        Assert.Equal(ProviderClientIdentity.SingletonId, first.Id);
        Assert.Equal(first.Id, second.Id);
        Assert.NotEqual(Guid.Empty, first.Id);
    }

    [Fact]
    public void Create_KeepsWhatItIsGiven()
    {
        var identity = ProviderClientIdentity.Create(CertificatePem, KeyCipher, Fingerprint);

        Assert.Equal(CertificatePem, identity.CertificatePem);
        Assert.Equal(KeyCipher, identity.KeyCipher);
        Assert.Equal(Fingerprint, identity.Fingerprint);
        Assert.NotEqual(default, identity.CreatedAt);
    }

    // The domain never sees a plaintext key: a PEM private key handed in by mistake is refused, not stored.
    [Fact]
    public void Create_APlaintextPrivateKeyAsTheCertificate_IsRefused()
    {
        string keyPem = "-----BEGIN PRIVATE KEY-----\nMIIB\n-----END PRIVATE KEY-----\n";

        Assert.Throws<ArgumentException>(() => ProviderClientIdentity.Create(keyPem, KeyCipher, Fingerprint));
    }

    [Fact]
    public void Create_EmptyKeyCipher_IsRefused()
    {
        Assert.Throws<ArgumentException>(() => ProviderClientIdentity.Create(CertificatePem, [], Fingerprint));
    }

    [Theory]
    [InlineData("")]
    [InlineData("D5D7E5097B92F32181A74C286ECCBDA67C869CE881ED8273FBD223EEADE2420E")]
    [InlineData("d5d7")]
    public void Create_FingerprintThatIsNot64LowerCaseHex_IsRefused(string fingerprint)
    {
        Assert.Throws<ArgumentException>(() => ProviderClientIdentity.Create(CertificatePem, KeyCipher, fingerprint));
    }
}
