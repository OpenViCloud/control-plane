using System.Net;
using System.Net.Http.Json;

namespace Titan.Tests.Api;

// The retired TopUp/promo routes answer 404 (no route matched) and the /topups routes exist. "Exists" is
// anything but 404: an unauthenticated call to a real route is rejected by auth (401) before a handler runs.
// The dead-route asserts are what a half-done rename (old route still served) fails.
//
// FullHost — WebApplicationFactory on its own Postgres database, own CI process. Reuses OpenApiSpecTests.ApiFactory.
[Trait("Category", "FullHost")]
[Collection("Http")]
public class TopUpRouteContractTests : IClassFixture<OpenApiSpecTests.ApiFactory>
{
    private readonly HttpClient _client;
    public TopUpRouteContractTests(OpenApiSpecTests.ApiFactory factory) => _client = factory.CreateClient();

    public static readonly TheoryData<string, string> DeadRoutes = new()
    {
        { "POST", "/v1/tenant/wallet/topups" },
        { "GET",  "/v1/tenant/wallet/topups" },
        { "POST", "/v1/admin/wallet/topups/11111111-1111-1111-1111-111111111111/confirm" },
        { "POST", "/v1/admin/promotional-credits" },
        { "GET",  "/v1/admin/promotional-credits" },
    };

    public static readonly TheoryData<string, string> LiveRoutes = new()
    {
        { "POST", "/v1/tenant/topups" },
        { "GET",  "/v1/tenant/topups" },
        { "POST", "/v1/admin/topups/11111111-1111-1111-1111-111111111111/confirm" },
        { "POST", "/v1/admin/topups/grant" },
        { "GET",  "/v1/admin/topups" },
    };

    [Theory]
    [MemberData(nameof(DeadRoutes))]
    public async Task OldTopUpRoutes_AreGone_404(string method, string path)
    {
        var res = await Send(method, path);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Theory]
    [MemberData(nameof(LiveRoutes))]
    public async Task NewTopUpRoutes_Exist_NotFoundIsWrong(string method, string path)
    {
        var res = await Send(method, path);
        // Unauthenticated → the route exists and auth rejected it; it must NOT be a routing 404.
        Assert.NotEqual(HttpStatusCode.NotFound, res.StatusCode);
    }

    private Task<HttpResponseMessage> Send(string method, string path) =>
        method == "GET"
            ? _client.GetAsync(path)
            : _client.PostAsJsonAsync(path, new { amount = 1000, tenantId = Guid.NewGuid(), paymentTxnRef = "x" });
}
