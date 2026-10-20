using System.Net;
using System.Text.Json;

namespace Titan.Tests.Api;

/// <summary>
/// Drift guard for the public OpenAPI contract: the runtime spec must equal the committed baseline
/// api/openapi-v1.json, so an endpoint/DTO change fails until the baseline is regenerated and committed.
/// </summary>
// FullHost — WebApplicationFactory test; own CI process (see CatalogContractTests for why).
[Trait("Category", "FullHost")]
[Collection("Http")]
public class OpenApiSpecTests : IClassFixture<OpenApiSpecTests.ApiFactory>
{
    private const string SpecUrl = "/swagger/v1/swagger.json";
    private readonly HttpClient _client;

    public OpenApiSpecTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task OpenApiSpec_MatchesCommittedBaseline()
    {
        var response = await _client.GetAsync(SpecUrl);
        var raw = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"GET {SpecUrl} -> {(int)response.StatusCode}. Body[0..300]: {raw[..Math.Min(300, raw.Length)]}");
        var live = Normalize(raw);

        var baselinePath = LocateBaseline();
        if (!File.Exists(baselinePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
            await File.WriteAllTextAsync(baselinePath, live);
            Assert.Fail($"OpenAPI baseline did not exist — generated it at {baselinePath}. Review and commit, then re-run.");
        }

        var committed = Normalize(await File.ReadAllTextAsync(baselinePath));
        Assert.True(live == committed,
            $"OpenAPI spec drifted from the committed baseline ({baselinePath}). " +
            "Regenerate by deleting that file and re-running this test, then commit the change.");
    }

    [Fact]
    public async Task PublicSpec_ExcludesInternalAndAdminRoutes_IncludesTenantRoutes()
    {
        var spec = await (await _client.GetAsync(SpecUrl)).Content.ReadAsStringAsync();

        Assert.DoesNotContain("/internal/", spec);   // worker callbacks — not public
        Assert.DoesNotContain("/v1/tenants/", spec);  // admin tenant-lifecycle — not public
        Assert.DoesNotContain("/v1/admin/", spec);    // admin pricing — not public

        Assert.Contains("/v1/tenant/vms", spec);      // public tenant-owned route present
    }

    // "servers" is dropped: its url is whichever host rendered the spec, so keeping it would pin the
    // baseline to one machine. The contract is paths + schemas, not the address they answer on.
    private static string Normalize(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var kept = doc.RootElement.EnumerateObject()
            .Where(p => p.Name != "servers")
            .ToDictionary(p => p.Name, p => p.Value.Clone());
        return JsonSerializer.Serialize(kept, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string LocateBaseline()
    {
        // ".git" is a FILE in a git worktree; checking only for the directory would walk past a worktree
        // root and overwrite the main checkout's baseline.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, ".git"))
                              && !File.Exists(Path.Combine(dir.FullName, ".git")))
            dir = dir.Parent;
        if (dir is null) throw new InvalidOperationException("Repo root (.git) not found from test base directory.");
        return Path.Combine(dir.FullName, "api", "openapi-v1.json");
    }

    public class ApiFactory : RealHostFactory
    {
    }
}
