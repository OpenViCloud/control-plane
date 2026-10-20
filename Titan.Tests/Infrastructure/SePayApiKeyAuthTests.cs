using Titan.Infrastructure.Endpoints.Payments;

namespace Titan.Tests.Infrastructure;

// The SePay webhook Apikey guard: a missing, empty, wrong, or wrongly-schemed
// Authorization header must never authenticate, and neither must a missing server-side key (fail
// closed). Matches parses the "Apikey <key>" scheme SePay sends.
public class SePayApiKeyAuthTests
{
    [Theory]
    [InlineData(null, "k3y")]                 // header absent
    [InlineData("", "k3y")]                    // header empty
    [InlineData("Apikey wrong", "k3y")]        // wrong key
    [InlineData("Apikey ", "k3y")]             // scheme but no key
    [InlineData("k3y", "k3y")]                 // key without the Apikey scheme
    [InlineData("Bearer k3y", "k3y")]          // wrong scheme
    [InlineData("Apikey k3y", null)]           // server key not configured — fail closed
    [InlineData("Apikey k3y", "")]             // server key empty — fail closed
    public void Matches_RejectsMissingEmptyOrWrong(string? header, string? expected)
    {
        Assert.False(SePayApiKeyAuth.Matches(header, expected));
    }

    [Theory]
    [InlineData("Apikey k3y")]
    [InlineData("apikey k3y")]   // scheme is case-insensitive
    public void Matches_AcceptsExactKeyWithScheme(string header)
    {
        Assert.True(SePayApiKeyAuth.Matches(header, "k3y"));
    }
}
