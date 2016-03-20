using System;
using System.Diagnostics;

namespace GitModTimes
{
    [DebuggerDisplay("RelativePath={RelativePath}, Time={Time}, Path={Path}")]
    public class FileTime
    {
        public FileTime(DateTimeOffset time, string relativePath, string path)
        {
            Time = time;
            RelativePath = relativePath;
            Path = path;
        }

        public DateTimeOffset Time { get; }
        public string RelativePath { get; }
        public string Path { get; }
    }
}