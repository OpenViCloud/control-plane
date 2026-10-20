namespace Titan.Application.Commands.Compute.Vm.Contracts;

// The worker's report that a vm.stop or vm.start command gave up. Both ids come from the
// route: the intent id is the one the command carries, so a late report for an older
// request names that request and not the one waiting now.
public record MarkVmIntentFailedCommand(Guid VmId, Guid IntentId);
