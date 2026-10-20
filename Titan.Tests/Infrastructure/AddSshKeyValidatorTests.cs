using FluentValidation.TestHelper;
using Titan.Application.Commands.Compute.SshKey.Contracts;
using Titan.Infrastructure.Endpoints.SshKey;
using Titan.Tests.Domain;

namespace Titan.Tests.Infrastructure;

// The API answers a bad key with 400 VALIDATION_ERROR before the handler runs; the domain refuses it too
// (SshPublicKeyTests), so the validator is not the only gate.
public class AddSshKeyValidatorTests
{
    private readonly AddSshKeyValidator _validator = new();

    [Fact]
    public void AcceptedKeyAndName_Pass()
    {
        var result = _validator.TestValidate(new AddSshKeyCommand("laptop", SshPublicKeyTests.Ed25519));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Rsa1024_IsRejected_WithTheReason()
    {
        var result = _validator.TestValidate(new AddSshKeyCommand("old", SshPublicKeyTests.Rsa1024));

        result.ShouldHaveValidationErrorFor(r => r.PublicKey).WithErrorMessage(
            "RSA keys must be at least 2048 bits; this one has 1024.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankName_IsRejected(string name)
    {
        _validator.TestValidate(new AddSshKeyCommand(name, SshPublicKeyTests.Ed25519))
            .ShouldHaveValidationErrorFor(r => r.Name);
    }

    [Fact]
    public void NameOf65_IsRejected()
    {
        _validator.TestValidate(new AddSshKeyCommand(new string('k', 65), SshPublicKeyTests.Ed25519))
            .ShouldHaveValidationErrorFor(r => r.Name);
    }
}
