namespace GitModTimes;

[DebuggerDisplay("FoundFiles={FoundFiles.Count}, MissingFiles={MissingFiles.Count}")]
public class FindResult(IReadOnlyList<FileTime> foundFiles, IReadOnlyList<string> missingFiles)
{
    public IReadOnlyList<FileTime> FoundFiles { get; } = foundFiles;
    public IReadOnlyList<string> MissingFiles { get; } = missingFiles;
}