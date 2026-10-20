using System.Security.Cryptography;
using System.Text;

namespace Titan.Infrastructure.Endpoints.Internal;

// Constant-time, to avoid a timing side-channel on the secret.
// Every callback must use it: InternalTokenCallbackTests.EveryEndpointThatReadsTheInternalToken_ComparesItThroughInternalTokenAuth.
internal static class InternalTokenAuth
{
    public static bool Matches(string? provided, string? expected)
    {
        if (string.IsNullOrEmpty(provided) || string.IsNullOrEmpty(expected))
            return false;
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(provided),
            Encoding.UTF8.GetBytes(expected));
    }
}
