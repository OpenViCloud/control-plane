using Titan.Infrastructure.Incus;

namespace Titan.Tests.Api;

// FullHost — the guard is a startup validation, so only a real host start shows it is wired in
// (https://github.com/OpenViCloud/titan/blob/main/docs/features/cluster/design.md#cluster-password-write-only). The control case starts the same
// host with another key, so a failure of the first case can only come from the key.
[Trait("Category", "FullHost")]
[Collection("Http")]
public class ClusterKeyGuardHostTests
{
    private const string AnotherKey = "c3RhZ2luZy1yZWFsLWtleS0wMTIzNDU2Nzg5YWJjZGU=";

    [Fact]
    public async Task Staging_WithThePlaceholderKey_DoesNotStart()
    {
        await using var factory = new StagingFactory(ClusterEncryptionOptions.DevelopmentPlaceholderKey);
        await factory.InitializeAsync();

        var ex = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

        Assert.Contains("development placeholder", ex.ToString());
    }

    [Fact]
    public async Task Staging_WithAnotherKey_Starts()
    {
        await using var factory = new StagingFactory(AnotherKey);
        await factory.InitializeAsync();

        var client = factory.CreateClient();
        var response = await client.GetAsync("/healthz");

        Assert.NotEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed class StagingFactory(string clusterKey) : RealHostFactory(
        new Dictionary<string, string?>
        {
            ["Encryption:ClusterKey"] = clusterKey,
            ["Keycloak:Authority"] = "http://keycloak.test/realms/titan",
        },
        new Dictionary<string, string> { ["environment"] = "Staging" })
    {
    }
}
