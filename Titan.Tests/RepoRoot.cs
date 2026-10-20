namespace Titan.Tests;

// Tests that read source files need the checkout on disk, and the test binary runs from
// bin/Debug/net9.0, so these walk up from there.
internal static class RepoRoot
{
    public static string ControlPlane { get; } = Find("Titan.Domain");

    /// ".git" is a directory in a normal checkout and a file in a worktree, so both are accepted.
    public static string Repo { get; } = FindGit();

    private static string Find(string marker)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, marker)))
            dir = dir.Parent;
        return dir?.FullName
            ?? throw new InvalidOperationException($"Could not find a parent directory containing '{marker}'.");
    }

    private static string FindGit()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null
               && !Directory.Exists(Path.Combine(dir.FullName, ".git"))
               && !File.Exists(Path.Combine(dir.FullName, ".git")))
            dir = dir.Parent;
        return dir?.FullName
            ?? throw new InvalidOperationException("Repo root (.git) not found from test base directory.");
    }
}
