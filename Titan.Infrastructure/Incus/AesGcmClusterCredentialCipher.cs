using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Titan.Infrastructure.Incus;

// GCM is authenticated: a tampered blob throws on Decrypt rather than yielding a garbage TLS key
// (https://github.com/OpenViCloud/titan/blob/main/docs/features/cluster/design.md#cluster-password-write-only). Blob layout: nonce (12) ‖ tag (16) ‖
// ciphertext; a fresh nonce per Encrypt. A bad Encryption__ClusterKey throws in the ctor.
public sealed class AesGcmClusterCredentialCipher : IClusterCredentialCipher
{
    private const int NonceSize = 12; // AesGcm standard nonce
    private const int TagSize = 16;   // AesGcm max tag
    private readonly byte[] _key;

    public AesGcmClusterCredentialCipher(IOptions<ClusterEncryptionOptions> options)
    {
        var raw = options.Value.ClusterKey
                  ?? throw new InvalidOperationException("Encryption:ClusterKey is not configured");
        var key = Convert.FromBase64String(raw); // FormatException if not base64 → fail fast
        if (key.Length != 32)
            throw new InvalidOperationException(
                $"Encryption:ClusterKey must be a base64-encoded 32-byte (256-bit) key; got {key.Length} bytes");
        _key = key;
    }

    public byte[] Encrypt(string plaintext)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var blob = new byte[NonceSize + TagSize + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, blob, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, blob, NonceSize, TagSize);
        Buffer.BlockCopy(cipherBytes, 0, blob, NonceSize + TagSize, cipherBytes.Length);
        return blob;
    }

    public string Decrypt(byte[] blob)
    {
        if (blob.Length < NonceSize + TagSize)
            throw new ArgumentException("cipher blob too short", nameof(blob));

        var nonce = blob.AsSpan(0, NonceSize);
        var tag = blob.AsSpan(NonceSize, TagSize);
        var cipherBytes = blob.AsSpan(NonceSize + TagSize);
        var plainBytes = new byte[cipherBytes.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes); // AuthenticationTagMismatchException on tamper

        return Encoding.UTF8.GetString(plainBytes);
    }
}
