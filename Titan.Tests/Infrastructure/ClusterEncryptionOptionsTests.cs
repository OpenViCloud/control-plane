using System.Text.Json;
using Titan.Infrastructure.Incus;

namespace Titan.Tests.Infrastructure;

// The committed placeholder key decrypts anything it encrypted for whoever has the repo, so a process outside
// Development must refuse to start with it (https://github.com/OpenViCloud/titan/blob/main/docs/features/cluster/design.md#cluster-password-write-only).
public class ClusterEncryptionOptionsTests
{
    private const string AnotherKey = "c3RhZ2luZy1yZWFsLWtleS0wMTIzNDU2Nzg5YWJjZGU=";

    [Fact]
    public void PlaceholderKey_OutsideDevelopment_IsRefused()
    {
        bool allowed = ClusterEncryptionOptions.IsAllowedKey(ClusterEncryptionOptions.DevelopmentPlaceholderKey, isDevelopment: false);

        Assert.False(allowed);
    }

    [Fact]
    public void PlaceholderKey_InDevelopment_IsAllowed()
    {
        bool allowed = ClusterEncryptionOptions.IsAllowedKey(ClusterEncryptionOptions.DevelopmentPlaceholderKey, isDevelopment: true);

        Assert.True(allowed);
    }

    [Fact]
    public void AnotherKey_OutsideDevelopment_IsAllowed()
    {
        bool allowed = ClusterEncryptionOptions.IsAllowedKey(AnotherKey, isDevelopment: false);

        Assert.True(allowed);
    }

    // The guard compares against a constant; appsettings.json is what actually ships the placeholder. If the two
    // drift apart the guard would stop recognising the committed key.
    [Fact]
    public void AppSettings_CommitsExactlyThePlaceholderTheGuardRefuses()
    {
        string appSettingsPath = FindHostFile("appsettings.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(appSettingsPath));

        string committedKey = doc.RootElement.GetProperty("Encryption").GetProperty("ClusterKey").GetString()!;

        Assert.Equal(ClusterEncryptionOptions.DevelopmentPlaceholderKey, committedKey);
    }

    private static string FindHostFile(string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            string candidate = Path.Combine(dir.FullName, "Titan.Host", fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
            dir = dir.Parent;
        }
        throw new FileNotFoundException($"Titan.Host/{fileName} not found above {AppContext.BaseDirectory}");
    }
}
