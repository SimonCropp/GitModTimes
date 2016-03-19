using System;

namespace GitModTimes
{
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