namespace Titan.Application.Commands.Platform.ProviderCommand.Contracts;

/// Retryable false: the worker gave up for good and has already sent the resource its give-up callback.
public record FailProviderCommandCommand(Guid LeaseId, string Error, bool Retryable);
