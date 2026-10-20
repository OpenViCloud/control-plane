using System.Security.Claims;
using Titan.Infrastructure.Auth;

namespace Titan.Tests.Auth;

// Keycloak puts realm roles in a nested `realm_access.roles` JSON claim. ASP.NET's
// RequireRole/IsInRole only reads flat role claims, so without this transformation a
// valid platform-admin token is rejected 403 (the admin API's authorization never fires).
public class KeycloakRealmRolesClaimsTransformationTests
{
    private const string RoleType = ClaimTypes.Role;

    private static ClaimsPrincipal Authenticated(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, authenticationType: "jwt", nameType: "preferred_username", roleType: RoleType));

    private static readonly KeycloakRealmRolesClaimsTransformation Sut = new();

    [Fact]
    public async Task Flattens_realm_access_roles_into_role_claims()
    {
        var principal = Authenticated(
            new Claim("realm_access", """{"roles":["platform-admin","offline_access"]}"""));

        var result = await Sut.TransformAsync(principal);

        Assert.True(result.IsInRole("platform-admin"));
        Assert.True(result.IsInRole("offline_access"));
    }

    [Fact]
    public async Task Without_realm_access_claim_adds_no_roles()
    {
        var principal = Authenticated(new Claim("sub", "u1"));

        var result = await Sut.TransformAsync(principal);

        Assert.False(result.IsInRole("platform-admin"));
    }

    [Fact]
    public async Task Is_idempotent_no_duplicate_role_claims()
    {
        var principal = Authenticated(
            new Claim("realm_access", """{"roles":["platform-admin"]}"""));

        await Sut.TransformAsync(principal);
        var result = await Sut.TransformAsync(principal);

        Assert.True(result.IsInRole("platform-admin"));
        Assert.Single(result.FindAll(c => c.Type == RoleType && c.Value == "platform-admin"));
    }

    [Fact]
    public async Task Unauthenticated_principal_is_left_unchanged()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity()); // not authenticated

        var result = await Sut.TransformAsync(principal);

        Assert.False(result.IsInRole("platform-admin"));
    }

    [Fact]
    public async Task Malformed_realm_access_does_not_throw()
    {
        var principal = Authenticated(new Claim("realm_access", "not-json"));

        var result = await Sut.TransformAsync(principal);

        Assert.False(result.IsInRole("platform-admin"));
    }
}
