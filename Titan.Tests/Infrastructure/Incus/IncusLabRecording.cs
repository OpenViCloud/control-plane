namespace Titan.Tests.Infrastructure.Incus;

// What Incus 7.0.1 answered in the lab (testdata/, copied from incus/provider/testdata/ of OpenViCloud/incus-worker):
// the oracle the fake server replays, so the client is tested against real bodies, not against what we think Incus says.
internal static class IncusLabRecording
{
    private static readonly string[] Files =
    [
        "lab-tls-shapes-2026-10-07.log",
        "lab-untrusted-2026-10-07.log",
    ];

    private static readonly Dictionary<string, (int Status, string Body)> Answers = Load();

    /// who is "trusted" or "untrusted"; pathAndQuery as the client sent it.
    public static (int Status, string Body) Answer(string who, string pathAndQuery)
    {
        bool recorded = Answers.TryGetValue(who + " " + pathAndQuery, out var answer);
        if (recorded)
        {
            return answer;
        }
        return Answers["untrusted /1.0/nope"];
    }

    private static Dictionary<string, (int, string)> Load()
    {
        var answers = new Dictionary<string, (int, string)>();
        string dir = Path.Combine(RepoRoot.ControlPlane, "Titan.Tests", "Infrastructure", "Incus", "testdata");
        foreach (string file in Files)
        {
            string[] lines = File.ReadAllLines(Path.Combine(dir, file));
            string? key = null;
            string body = "";
            foreach (string line in lines)
            {
                bool isHeader = line.StartsWith("== ") && line.Contains(" GET ");
                if (isHeader)
                {
                    string[] fields = line[3..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    key = fields[0] + " " + fields[2];
                    body = "";
                    continue;
                }
                if (key is null)
                {
                    continue;
                }
                bool isStatus = line.StartsWith("HTTP ");
                if (isStatus)
                {
                    answers[key] = (int.Parse(line[5..]), body);
                    key = null;
                    continue;
                }
                bool isFirstBodyLine = body == "" && line.Trim() != "";
                if (isFirstBodyLine)
                {
                    body = line;
                }
            }
        }
        return answers;
    }
}
