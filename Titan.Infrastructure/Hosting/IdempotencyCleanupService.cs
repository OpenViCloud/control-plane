using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Titan.Application.Abstractions;
using Titan.Application.Options;

namespace Titan.Infrastructure.Hosting;

/// <summary>
/// Without a TTL idempotency_keys grows unbounded. A CP-side BackgroundService, not a provider command:
/// this is local DB maintenance, not a business workflow.
/// </summary>
public class IdempotencyCleanupService(
    IServiceScopeFactory scopeFactory,
    IOptions<IdempotencyCleanupOptions> options,
    ILogger<IdempotencyCleanupService> logger) : BackgroundService
{
    private readonly IdempotencyCleanupOptions _opts = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_opts.IntervalMinutes));

        // Wait-first: no purge during startup, and none under the InMemory provider in WebApplicationFactory
        // tests, which does not support ExecuteDelete.
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var store = scope.ServiceProvider.GetRequiredService<IIdempotencyStore>();
                var removed = await PurgeOnceAsync(store, stoppingToken);
                if (removed > 0)
                    logger.LogInformation("Idempotency cleanup removed {Count} expired records", removed);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Idempotency cleanup pass failed");
            }
        }
    }

    /// <summary>Public for testing.</summary>
    public Task<int> PurgeOnceAsync(IIdempotencyStore store, CancellationToken ct)
    {
        var cutoff = DateTimeOffset.UtcNow.AddHours(-_opts.TtlHours);
        return store.PurgeExpiredAsync(cutoff, ct);
    }
}
