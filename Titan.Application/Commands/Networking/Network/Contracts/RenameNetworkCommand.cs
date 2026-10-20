namespace Titan.Application.Commands.Networking.Network.Contracts;

public record RenameNetworkCommand(Guid NetworkId, string Name);
