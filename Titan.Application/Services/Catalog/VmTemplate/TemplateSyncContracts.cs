// Same namespace as TemplateSyncRunner on purpose (https://github.com/OpenViCloud/titan-ops/blob/main/docs/architecture/code-structure.md#services-branch).

namespace Titan.Application.Services.Catalog.VmTemplate;

public record TemplateSyncRunResult(TemplateSyncOutcome Outcome, int Total, int Synced, int Failed, int Pending);
