namespace Titan.Infrastructure.Incus;

// Titan's client private key at rest (https://github.com/OpenViCloud/titan/blob/main/docs/features/cluster/design.md#provider-client-identity). No
// key-id/rotation surface: one key, and a key change is a manual re-encrypt.
public interface IClusterCredentialCipher
{
    byte[] Encrypt(string plaintext);
    string Decrypt(byte[] cipher);
}
