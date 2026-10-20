using Titan.Infrastructure.Endpoints.Internal;

namespace Titan.Tests.Infrastructure;

// InternalTokenAuth guards worker→CP callbacks: a missing, empty, or wrong
// X-Internal-Token must never authenticate — and neither must a missing server-side secret
// (fail closed rather than treating "no token configured" as open access).
public class InternalTokenAuthTests
{
    [Theory]
    [InlineData(null, "secret")]      // header absent
    [InlineData("", "secret")]        // header empty
    [InlineData("wrong", "secret")]   // header wrong
    [InlineData("secret", null)]      // secret not configured — must fail closed
    [InlineData("secret", "")]        // secret configured empty — must fail closed
    public void Matches_RejectsMissingEmptyOrWrongTokens(string? provided, string? expected)
    {
        Assert.False(InternalTokenAuth.Matches(provided, expected));
    }

    [Fact]
    public void Matches_AcceptsExactToken()
    {
        Assert.True(InternalTokenAuth.Matches("secret", "secret"));
    }
}
