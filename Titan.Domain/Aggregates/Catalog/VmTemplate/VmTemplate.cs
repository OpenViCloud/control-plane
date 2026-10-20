using Titan.Domain.Exceptions;
using Titan.Domain.Values;
namespace Titan.Domain.Aggregates.Catalog.VmTemplate;

// A template answers "which operating system", never "how big" (docs/features/catalog/design.md#vm-offering-sizing).
// It points at an offering (VmOfferingId) for its base sizing instead of owning sizing numbers: a tier id cannot drift
// (docs/features/catalog/design.md#template-borrows-sizing).
public class VmTemplate
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string OsImageName { get; private set; } = default!;

    /// The distro's own cloud image, with "{arch}" where the distro names the architecture; null ⇒ the worker
    /// pulls OsImageName from the image server (docs/features/catalog/design.md#template-is-an-image-alias).
    /// Set by the seed only.
    public string? ImageUrl { get; private set; }

    /// Customers see and may pick only listed templates (docs/features/catalog/design.md#template-listed).
    /// Create lists a template; only the seed unlists one.
    public bool IsListed { get; private set; }
    public int? RootDiskSizeGb { get; private set; }

    /// The base sizing the provider stores on the template, not what a VM gets: the customer's offering
    /// overrides it at instantiate, so nothing here constrains what the customer may choose.
    public string VmOfferingId { get; private set; } = default!;

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// One sync state per zone the template was asked into or observed in
    /// (docs/features/catalog/design.md#template-per-zone).
    private readonly List<VmTemplateZone> _zones = [];
    public IReadOnlyCollection<VmTemplateZone> Zones => _zones;

    private VmTemplate() { }

    public static VmTemplate Create(string name, string? description, string osImageName,
        int? rootDiskSizeGb, string vmOfferingId)
    {
        // The FK rejects an unknown id but accepts "", which would reach the sync path as a lookup miss far
        // from the row that caused it.
        if (string.IsNullOrWhiteSpace(vmOfferingId))
            throw new ArgumentException("vmOfferingId must not be empty", nameof(vmOfferingId));

        return new VmTemplate
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            OsImageName = osImageName,
            RootDiskSizeGb = rootDiskSizeGb,
            VmOfferingId = vmOfferingId,
            IsListed = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    public VmTemplateZone? ZoneOrNull(string zoneId)
    {
        foreach (VmTemplateZone zone in _zones)
        {
            if (zone.ZoneId == zoneId)
            {
                return zone;
            }
        }
        return null;
    }

    /// What customers in that zone may pick: the template whose alias that zone's cluster was last seen to hold.
    public bool IsSyncedIn(string zoneId)
    {
        VmTemplateZone? zone = ZoneOrNull(zoneId);
        if (zone == null)
        {
            return false;
        }
        return zone.CurrentExistence == CurrentExistence.Exists;
    }

    /// The image alias a VM in that zone is created from, handed out only past the check
    /// (docs/architecture/intent-and-observation.md#aggregate-hands-out-provider-id).
    public string ProviderIdIn(string zoneId)
    {
        VmTemplateZone? zone = ZoneOrNull(zoneId);
        string? externalTemplateId = null;
        if (zone != null)
        {
            externalTemplateId = zone.ExternalTemplateId;
        }
        if (string.IsNullOrEmpty(externalTemplateId))
        {
            throw new BusinessException("TEMPLATE_NOT_SYNCED",
                $"Template '{Name}' has not been synced to zone '{zoneId}' yet.");
        }
        // An alias that went missing keeps its provider id, so the check above lets it through.
        bool aliasIsThere = zone!.CurrentExistence == CurrentExistence.Exists;
        if (!aliasIsThere)
        {
            throw new BusinessException("TEMPLATE_UNAVAILABLE",
                $"Template '{Name}' is not available in zone '{zoneId}' right now.");
        }
        return externalTemplateId;
    }

    /// Refused when that zone has the alias or a sync waiting: the handler recognises the repeat first and
    /// re-sends the command (docs/architecture/intent-and-observation.md#domain-refuses-a-repeat).
    public void RequestSyncIn(string zoneId)
    {
        RowFor(zoneId).RequestSync();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// Observations converge (docs/architecture/intent-and-observation.md#observation-converges): a report for a
    /// zone with no row yet records what was observed there instead of refusing it. Which zones exist is the
    /// handler's check, not the aggregate's. Returns whether it ended a waiting sync.
    public bool MarkCreatedIn(string zoneId, string externalTemplateId)
    {
        bool fulfilled = RowFor(zoneId).MarkCreated(externalTemplateId);
        UpdatedAt = DateTimeOffset.UtcNow;
        return fulfilled;
    }

    public void MarkObservedIn(string zoneId)
    {
        RowFor(zoneId).MarkObserved();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkNotFoundIn(string zoneId)
    {
        RowFor(zoneId).MarkNotFound();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// Returns whether a waiting sync took the failure; a stale give-up records nothing.
    public bool MarkRequestFailedIn(string zoneId, string? reason, string? detail)
    {
        bool recorded = RowFor(zoneId).MarkRequestFailed(reason, detail);
        UpdatedAt = DateTimeOffset.UtcNow;
        return recorded;
    }

    private VmTemplateZone RowFor(string zoneId)
    {
        if (string.IsNullOrWhiteSpace(zoneId))
            throw new ArgumentException("zoneId must not be empty", nameof(zoneId));

        VmTemplateZone? existing = ZoneOrNull(zoneId);
        if (existing != null)
        {
            return existing;
        }
        var added = VmTemplateZone.NotYetSynced(Id, zoneId);
        _zones.Add(added);
        return added;
    }
}
