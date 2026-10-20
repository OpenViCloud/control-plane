using Titan.Application.Commands.Platform.Tenant.Contracts;
using Titan.Domain.Repositories.Platform;
using TenantAggregate = Titan.Domain.Aggregates.Platform.Tenant.Tenant;

namespace Titan.Application.Commands.Platform.Tenant;

/// Creates the caller's tenant on first sign-in. Idempotent: a caller who already owns a tenant is a no-op.
/// Every signup starts PendingApproval; the middleware approves a verified one in the same request, after
/// opening its wallet (https://github.com/OpenViCloud/titan/blob/main/docs/architecture/tenancy-and-admin.md#approval-is-activation).
public class BootstrapTenantCommandHandler(ITenantRepository repo)
{
    public async Task HandleAsync(BootstrapTenantCommand command, CancellationToken ct = default)
    {
        string keycloakUserId = command.KeycloakUserId;

        // Repeat (https://github.com/OpenViCloud/titan/blob/main/docs/architecture/intent-and-observation.md#domain-refuses-a-repeat): the middleware calls this on a
        // signed-in caller's requests; the first one created the tenant, every later one finds it.
        bool alreadyBootstrapped = await repo.ExistsByOwnerAsync(keycloakUserId, ct);
        if (alreadyBootstrapped)
        {
            return;
        }

        TenantAggregate tenant = TenantAggregate.CreatePending(command.Username, keycloakUserId, command.Email);
        repo.Add(tenant);
        await repo.SaveAsync(ct);
    }
}
