namespace GitModTimes;

[DebuggerDisplay("RelativePath={RelativePath}, Time={Time}, Path={Path}")]
public class FileTime(DateTimeOffset time, string relativePath, string path)
{
    public DateTimeOffset Time { get; } = time;
    public string RelativePath { get; } = relativePath;
    public string Path { get; } = path;
}