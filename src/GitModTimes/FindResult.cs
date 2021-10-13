namespace GitModTimes;

[DebuggerDisplay("FoundFiles={FoundFiles.Count}, MissingFiles={MissingFiles.Count}")]
public class FindResult
{
    public IReadOnlyList<FileTime> FoundFiles { get; }
    public IReadOnlyList<string> MissingFiles { get; }

    public FindResult(IReadOnlyList<FileTime> foundFiles, IReadOnlyList<string> missingFiles)
    {
        FoundFiles = foundFiles;
        MissingFiles = missingFiles;
    }
}