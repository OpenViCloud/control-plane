using Microsoft.EntityFrameworkCore;
using Npgsql;
using Titan.Domain.Aggregates.Platform.ProviderClientIdentity;
using Titan.Domain.Repositories.Platform;
using Titan.Infrastructure.Persistence;

namespace Titan.Infrastructure.Repositories;

public class ProviderClientIdentityRepository(TitanDbContext db) : IProviderClientIdentityRepository
{
    public Task<ProviderClientIdentity?> FindAsync(CancellationToken ct) =>
        db.ProviderClientIdentities
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == ProviderClientIdentity.SingletonId, ct);

    public async Task<bool> TryAddAsync(ProviderClientIdentity identity, CancellationToken ct)
    {
        db.ProviderClientIdentities.Add(identity);
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // A concurrent control plane stored the identity first (same fixed id). Detach ours so the scoped context
            // does not resubmit it; the caller reads the winner back.
            db.Entry(identity).State = EntityState.Detached;
            return false;
        }
    }
}
