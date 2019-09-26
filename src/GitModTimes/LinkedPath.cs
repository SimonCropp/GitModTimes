using System.Diagnostics;

[DebuggerDisplay("GitPath={GitPath}, OriginalPath={OriginalPath}")]
class LinkedPath
{
    public string OriginalPath { get; }
    public string GitPath { get; }

    public LinkedPath(string originalPath, string gitPath)
    {
        OriginalPath = originalPath;
        GitPath = gitPath;
    }
}