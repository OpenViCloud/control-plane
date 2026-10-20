using ClusterAggregate = Titan.Domain.Aggregates.Platform.Cluster.Cluster;

namespace Titan.Infrastructure.Endpoints.Admin;

internal static class ClusterConnectionRules
{
    private const string AValidEndpoint = "https://validation.invalid:8443";
    private static readonly string AValidFingerprint = new('0', 64);

    // The aggregate's own rules decide, so a 400 here and the domain never disagree.
    public static bool IsValidEndpoint(string? endpoint) => Accepts(endpoint ?? "", AValidFingerprint);

    public static bool IsValidFingerprint(string? serverFingerprint) => Accepts(AValidEndpoint, serverFingerprint ?? "");

    private static bool Accepts(string endpoint, string serverFingerprint)
    {
        try
        {
            ClusterAggregate.Create("validation", null, endpoint, serverFingerprint);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
