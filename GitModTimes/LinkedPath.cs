using System.Diagnostics;

[DebuggerDisplay("GitPath={GitPath}, OriginalPath={OriginalPath}")]
class LinkedPath
{
    public string OriginalPath;
    public string GitPath;
}
