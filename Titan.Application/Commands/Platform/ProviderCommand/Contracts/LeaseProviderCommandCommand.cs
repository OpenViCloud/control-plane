namespace Titan.Application.Commands.Platform.ProviderCommand.Contracts;

/// The worker mints LeaseId; Kinds are the command kinds it can run.
public record LeaseProviderCommandCommand(Guid LeaseId, IReadOnlyList<string> Kinds);
