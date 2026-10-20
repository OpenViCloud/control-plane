namespace Titan.Application.Commands.Catalog.VmTemplate.Contracts;

// Internal callback (worker → CP). The route carries the id; the body carries the rest. Report says which of three
// reports this is (https://github.com/OpenViCloud/titan/blob/main/docs/features/catalog/design.md#template-reports): an observation, the sync's alias, or its give-up.
public record UpdateTemplateStatusCommand(
    Guid TemplateId,
    string ZoneId,               // the zone whose cluster was synced or observed (https://github.com/OpenViCloud/titan/blob/main/docs/features/catalog/design.md#template-per-zone)
    string Report,
    bool? Exists = null,         // required when Report == "Observed"
    string? ExternalId = null,   // the image alias; required when Report == "Created"
    string? Reason = null,       // the fixed reason of a give-up, shown to the operator
    string? Detail = null);      // the raw error of a give-up, operator-only
