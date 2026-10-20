using Titan.Application.Services.Compute;

namespace Titan.Tests.Services;

// The script shape was measured on Incus with the distro's own Ubuntu image: keys appended next to keys the customer's
// user-data set; no `set -e`, so one failed step does not stop the next.
public class VmVendorDataTests
{
    private const string Ed = "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIJQIuvIctzZ61SoksnjJlkCRERFjIowWaKhlaoSOsHMB";
    private const string Rsa = "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQCzyjeB";

    [Fact]
    public void NoKeys_IsNoVendorData()
    {
        Assert.Null(VmVendorData.ForKeys([]));
    }

    [Fact]
    public void Keys_AreAppendedToTheUbuntuUsersAuthorizedKeys()
    {
        string script = VmVendorData.ForKeys([Ed, Rsa])!;

        Assert.StartsWith("#!/bin/sh\n", script);
        Assert.Contains("cat >> \"$home/.ssh/authorized_keys\" <<'KEYS'\n" + Ed + "\n" + Rsa + "\nKEYS\n", script);
        Assert.Contains("user=ubuntu", script);
        Assert.DoesNotContain("set -e", script);
        Assert.DoesNotContain("usermod", script);
    }

    // Each line was rebuilt from a parsed key, so it can hold nothing but a type and base64; anything else means a
    // caller handed raw input in.
    [Theory]
    [InlineData("ssh-ed25519 AAAA\nKEYS\nrm -rf /")]
    [InlineData("ssh-ed25519 AAAA comment")]
    [InlineData("ssh-ed25519 AAAA'")]
    [InlineData("ssh-ed25519 AAAA\n")]
    public void ALineThatIsNotTypeAndBase64_IsRefused(string line)
    {
        Assert.Throws<ArgumentException>(() => VmVendorData.ForKeys([line]));
    }
}
