using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Titan.Infrastructure.Auth;

namespace Titan.Tests.Auth;

public class ApiTokenSchemesTests
{
    [Theory]
    [InlineData("Bearer ovc_abc", "ovc_abc")]
    [InlineData("bearer ovc_abc", "ovc_abc")]
    [InlineData("BEARER   ovc_abc  ", "ovc_abc")]
    [InlineData("Bearer ovc_", "ovc_")]
    public void ReadToken_FindsAnOvcBearer(string authorization, string expected)
    {
        Assert.Equal(expected, ApiTokenSchemes.ReadToken(authorization));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Bearer eyJhbGciOiJSUzI1NiJ9.e30.sig")]
    [InlineData("Basic b3ZjX2FiYw==")]
    [InlineData("ovc_abc")]
    [InlineData("Bearer OVC_abc")]
    public void ReadToken_LeavesEverythingElseAlone(string? authorization)
    {
        Assert.Null(ApiTokenSchemes.ReadToken(authorization));
    }

    [Fact]
    public void Choose_SendsAnOvcBearerToTheTokenScheme_AndTheRestToJwt()
    {
        Assert.Equal(ApiTokenSchemes.ApiToken, ApiTokenSchemes.Choose(With("Bearer ovc_abc")));
        Assert.Equal(JwtBearerDefaults.AuthenticationScheme, ApiTokenSchemes.Choose(With("Bearer eyJ.e30.sig")));
        Assert.Equal(JwtBearerDefaults.AuthenticationScheme, ApiTokenSchemes.Choose(With(null)));
    }

    private static HttpContext With(string? authorization)
    {
        var context = new DefaultHttpContext();
        if (authorization != null)
        {
            context.Request.Headers.Authorization = authorization;
        }

        return context;
    }
}
