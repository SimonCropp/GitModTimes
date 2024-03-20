[DebuggerDisplay("GitPath={GitPath}, OriginalPath={OriginalPath}")]
class LinkedPath(string originalPath, string gitPath)
{
    public string OriginalPath { get; } = originalPath;
    public string GitPath { get; } = gitPath;
}