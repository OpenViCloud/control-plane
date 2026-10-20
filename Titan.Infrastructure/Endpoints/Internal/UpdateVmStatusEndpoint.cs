using FastEndpoints;
using Microsoft.Extensions.Configuration;
using Titan.Application.Commands.Compute.Vm;
using Titan.Application.Commands.Compute.Vm.Contracts;

namespace Titan.Infrastructure.Endpoints.Internal;

public class UpdateVmStatusEndpoint(UpdateVmStatusCommandHandler handler, IConfiguration config)
    : Endpoint<UpdateVmStatusBody>
{
    public override void Configure()
    {
        Patch("internal/vms/{vmId:guid}/status");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateVmStatusBody body, CancellationToken ct)
    {
        var token = HttpContext.Request.Headers["X-Internal-Token"].FirstOrDefault();
        if (!InternalTokenAuth.Matches(token, config["Internal:Token"]))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var vmId = Route<Guid>("vmId");
        await handler.HandleAsync(
            new UpdateVmStatusCommand(vmId, body.Report, body.Exists, body.Status, body.ExternalId, body.IpAddress,
                body.PublicIpv6Address, body.Reason, body.Detail),
            ct);
        await SendNoContentAsync(ct);
    }
}

// The worker's JSON body; UpdateVmStatusCommand additionally carries the route id.
public record UpdateVmStatusBody(
    string Report,
    bool? Exists = null,
    string? Status = null,
    string? ExternalId = null,
    string? IpAddress = null,
    string? PublicIpv6Address = null,
    string? Reason = null,
    string? Detail = null);
