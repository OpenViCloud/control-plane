namespace Titan.Application.Commands.Compute.SshKey.Contracts;

/// `publicKey` is one public key line as ssh-keygen writes it, e.g. the content of ~/.ssh/id_ed25519.pub.
public record AddSshKeyCommand(string Name, string PublicKey);
