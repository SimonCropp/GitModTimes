using System;
using System.IO;

namespace GitModTimes
{
    public static class GitModifiedTimesFixer
    {
        public static void FixTimes(string gitDirectory, DateTimeOffset? stopBefore = null)
        {
            foreach (var fileTime in GitModifiedTimesFinder.GetTimes(gitDirectory, stopBefore: stopBefore))
            {
                File.SetLastWriteTimeUtc(fileTime.Path, fileTime.Time.UtcDateTime);
            }
        }

    }
}