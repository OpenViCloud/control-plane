using Titan.Domain.Exceptions;

namespace Titan.Application.Options;

public class ZoneConfig
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}

public class ZoneOptions
{
    public List<ZoneConfig> Zones { get; set; } = [];

    // A zone the customer picks must be one Titan has: an unknown id would fall through the cluster resolver to
    // the config section's cluster (docs/features/catalog/design.md#zone-is-a-real-choice).
    public void Require(string zoneId)
    {
        bool zoneExists = Zones.Any(z => z.Id == zoneId);
        if (!zoneExists)
        {
            throw new NotFoundException("ZONE_NOT_FOUND", $"Zone '{zoneId}' not found.");
        }
    }
}
