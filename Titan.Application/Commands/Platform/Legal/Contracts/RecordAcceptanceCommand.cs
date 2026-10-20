namespace Titan.Application.Commands.Platform.Legal.Contracts;

// The caller comes from the request context; the endpoint pulls the source IP off the connection.
public record RecordAcceptanceCommand(string? SourceIp);
