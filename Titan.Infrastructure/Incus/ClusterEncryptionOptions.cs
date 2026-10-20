using System.ComponentModel.DataAnnotations;

namespace Titan.Infrastructure.Incus;

// Env var Encryption__ClusterKey: a base64-encoded 32-byte key for AES-256-GCM, validated on start so a
// missing or mis-sized key fails fast (docs/features/cluster/design.md#cluster-password-write-only).
public class ClusterEncryptionOptions
{
    public const string SectionName = "Encryption";

    [Required] public string ClusterKey { get; init; } = default!;

    // The key committed in appsettings.json for local runs: anyone with the repo decrypts what it encrypted, so only
    // Development may start with it (docs/features/cluster/design.md#cluster-password-write-only).
    public const string DevelopmentPlaceholderKey = "dGl0YW4tcGxhY2Vob2xkZXItY2x1c3Rlci1rZXkhISE=";

    public static bool IsAllowedKey(string clusterKey, bool isDevelopment)
    {
        bool isPlaceholder = clusterKey == DevelopmentPlaceholderKey;
        return isDevelopment || !isPlaceholder;
    }
}
