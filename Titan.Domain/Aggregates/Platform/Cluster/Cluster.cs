namespace Titan.Domain.Aggregates.Platform.Cluster;

// Platform-owned, not tenant-scoped: the Incus cluster serving one zone. Titan pins the server certificate by its
// SHA-256 fingerprint (https://github.com/OpenViCloud/titan-ops/blob/main/docs/features/cluster/design.md#cluster-incus); there is no credential here, Titan
// authenticates with its own client identity.
public class Cluster
{
    public Guid Id { get; private set; }
    public string ZoneId { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Endpoint { get; private set; } = default!;
    public string ServerFingerprint { get; private set; } = default!;
    public ClusterProviderSettings ProviderSettings { get; private set; } = ClusterProviderSettings.None;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Cluster() { }

    public static Cluster Create(string zoneId, string? name, string endpoint, string serverFingerprint)
    {
        if (string.IsNullOrWhiteSpace(zoneId))
        {
            throw new ArgumentException("zoneId must not be blank", nameof(zoneId));
        }
        string normalizedEndpoint = NormalizeEndpoint(endpoint);
        string normalizedFingerprint = NormalizeFingerprint(serverFingerprint);

        var now = DateTimeOffset.UtcNow;
        return new Cluster
        {
            Id = Guid.NewGuid(),
            ZoneId = zoneId,
            Name = NameOrZone(name, zoneId),
            Endpoint = normalizedEndpoint,
            ServerFingerprint = normalizedFingerprint,
            ProviderSettings = ClusterProviderSettings.None,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public void UpdateConnection(string? name, string endpoint, string serverFingerprint)
    {
        string normalizedEndpoint = NormalizeEndpoint(endpoint);
        string normalizedFingerprint = NormalizeFingerprint(serverFingerprint);

        Name = NameOrZone(name, ZoneId);
        Endpoint = normalizedEndpoint;
        ServerFingerprint = normalizedFingerprint;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // Replaced whole: a field the admin cleared goes back to "not set".
    public void SetProviderSettings(ClusterProviderSettings settings)
    {
        ProviderSettings = settings;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NameOrZone(string? name, string zoneId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return zoneId;
        }
        return name;
    }

    private static string NormalizeEndpoint(string endpoint)
    {
        string trimmed = (endpoint ?? "").Trim().TrimEnd('/');
        bool isAbsolute = Uri.TryCreate(trimmed, UriKind.Absolute, out Uri? uri);
        bool isHttpsWithAHost = isAbsolute && uri!.Scheme == Uri.UriSchemeHttps && uri.Host != "";
        if (!isHttpsWithAHost)
        {
            throw new ArgumentException("endpoint must be an https URL", nameof(endpoint));
        }
        return trimmed;
    }

    // Accepts what `incus remote add` and openssl print (colons, upper case); stores what the handshake compares.
    private static string NormalizeFingerprint(string fingerprint)
    {
        string hex = (fingerprint ?? "").Trim().Replace(":", "").ToLowerInvariant();
        bool isSha256Hex = hex.Length == 64 && hex.All(Uri.IsHexDigit);
        if (!isSha256Hex)
        {
            throw new ArgumentException("serverFingerprint must be a SHA-256 fingerprint (64 hex digits)", nameof(fingerprint));
        }
        return hex;
    }
}
