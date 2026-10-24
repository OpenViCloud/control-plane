using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Titan.Tests.Api;

file sealed class KeycloakTestFactory(string? metadataAddress) : RealHostFactory(
    metadataAddress is null
        ? new Dictionary<string, string?>()
        : new Dictionary<string, string?> { ["Keycloak:MetadataAddress"] = metadataAddress },
    new Dictionary<string, string>())
{
}

// FullHost — WebApplicationFactory test; own CI process (see CatalogContractTests for why).
// Keycloak:MetadataAddress lets the control plane read discovery and JWKS from Keycloak's in-cluster Service while tokens
// keep the public issuer (https://github.com/OpenViCloud/titan-ops/blob/main/docs/architecture/platform-infra.md#keycloak-metadata).
[Trait("Category", "FullHost")]
[Collection("Http")]
public class KeycloakMetadataTests
{
    private const string PublicAuthority = "https://keycloak.test/realms/titan";

    private static async Task<JwtBearerOptions> BearerOptionsOf(string? metadataAddress)
    {
        await using var factory = new KeycloakTestFactory(metadataAddress);
        await factory.InitializeAsync();
        var monitor = factory.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
        return monitor.Get(JwtBearerDefaults.AuthenticationScheme);
    }

    [Fact]
    public async Task NoMetadataAddress_MetadataComesFromTheAuthority()
    {
        var options = await BearerOptionsOf(null);

        Assert.Equal(PublicAuthority, options.Authority);
        Assert.Equal(PublicAuthority + "/.well-known/openid-configuration", options.MetadataAddress);
        Assert.True(options.RequireHttpsMetadata);
        Assert.Null(options.TokenValidationParameters.ValidIssuer);
    }

    [Fact]
    public async Task InClusterMetadataAddress_IsUsed_OverHttp_AndTheIssuerStaysTheAuthority()
    {
        const string inCluster = "http://keycloak-http.titan-auth-staging.svc/auth/realms/titan/.well-known/openid-configuration";

        var options = await BearerOptionsOf(inCluster);

        Assert.Equal(inCluster, options.MetadataAddress);
        Assert.False(options.RequireHttpsMetadata);
        Assert.Equal(PublicAuthority, options.TokenValidationParameters.ValidIssuer);
    }

    [Fact]
    public async Task HttpsMetadataAddress_StillRequiresHttps()
    {
        var options = await BearerOptionsOf("https://keycloak.internal.test/realms/titan/.well-known/openid-configuration");

        Assert.True(options.RequireHttpsMetadata);
    }
}
