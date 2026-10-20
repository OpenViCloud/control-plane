using System.Net;

namespace Titan.Tests.Api;

// FullHost — WebApplicationFactory test; own CI process (see CatalogContractTests for why).
[Trait("Category", "FullHost")]
[Collection("Http")]
public class HealthCheckTests : IClassFixture<HealthCheckTests.ApiFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task GET_healthz_EndpointExists()
    {
        var response = await _client.GetAsync("/healthz");
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GET_healthz_DoesNotRequireAuthentication()
    {
        // No Authorization header
        var response = await _client.GetAsync("/healthz");
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // Meter liveness has its own endpoint so a stale meter never pulls the pod out of the readiness
    // probe. Only "is mapped" is checkable here: this harness's http:// Keycloak authority makes the auth
    // middleware 500 before any health endpoint. The verdict is in MeterLivenessHealthCheckTests.
    [Fact]
    public async Task GET_healthz_meters_EndpointExists()
    {
        var response = await _client.GetAsync("/healthz/meters");
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    public class ApiFactory() : RealHostFactory(
        new Dictionary<string, string?> { ["Keycloak:Authority"] = "http://keycloak.test/realms/titan" },
        new Dictionary<string, string>())
    {
    }
}
