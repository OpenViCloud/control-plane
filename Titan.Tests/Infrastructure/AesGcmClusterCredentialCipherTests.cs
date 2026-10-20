using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Titan.Infrastructure.Incus;

namespace Titan.Tests.Infrastructure;

// Titan's TLS client key is encrypted at rest with AES-256-GCM (https://github.com/OpenViCloud/titan/blob/main/docs/features/cluster/design.md#cluster-db-entity). GCM is chosen
// over raw AES so a tampered ciphertext fails to decrypt (authenticated) rather than silently
// yielding a garbage key. These tests pin: round-trip,
// a fresh nonce per encryption (identical plaintext → different ciphertext), tamper detection, and
// fail-fast on a mis-sized key (a bad Encryption__ClusterKey must stop startup, not sit latent until the
// first decrypt).
public class AesGcmClusterCredentialCipherTests
{
    // A deterministic 32-byte key, base64-encoded, standing in for the k8s secret.
    private static string Key256() => Convert.ToBase64String(Enumerable.Range(0, 32).Select(i => (byte)i).ToArray());

    private static AesGcmClusterCredentialCipher Cipher(string? base64Key = null) =>
        new(Options.Create(new ClusterEncryptionOptions { ClusterKey = base64Key ?? Key256() }));

    [Fact]
    public void RoundTrip_DecryptReturnsOriginalPlaintext()
    {
        var cipher = Cipher();
        const string secret = "s3cr3t-oneadmin-password";

        var decrypted = cipher.Decrypt(cipher.Encrypt(secret));

        Assert.Equal(secret, decrypted);
    }

    [Fact]
    public void Encrypt_SamePlaintextTwice_ProducesDifferentCiphertext()
    {
        var cipher = Cipher();

        var a = cipher.Encrypt("same");
        var b = cipher.Encrypt("same");

        Assert.NotEqual(a, b); // fresh random nonce each time
        Assert.Equal("same", cipher.Decrypt(a));
        Assert.Equal("same", cipher.Decrypt(b));
    }

    [Fact]
    public void Decrypt_TamperedCiphertext_Throws()
    {
        var cipher = Cipher();
        var blob = cipher.Encrypt("secret");
        blob[^1] ^= 0xFF; // flip a byte in the tag/ciphertext

        Assert.Throws<AuthenticationTagMismatchException>(() => cipher.Decrypt(blob));
    }

    [Theory]
    [InlineData("")]                                   // empty
    [InlineData("bm90LTMyLWJ5dGVz")]                   // "not-32-bytes" → 12 bytes
    [InlineData("not-valid-base64!!!")]                // not base64 at all
    public void Ctor_BadKey_FailsFast(string badKey)
    {
        Assert.ThrowsAny<Exception>(() => Cipher(badKey));
    }
}
