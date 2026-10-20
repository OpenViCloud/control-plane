namespace Titan.Application.Commands.Compute.Vm.Contracts;

// Carries the route id as well as the body. FastEndpoints binds Name from the JSON body and the
// endpoint fills VmId from the route, so one type covers the whole request.
public record RenameVmCommand(Guid VmId, string Name);
