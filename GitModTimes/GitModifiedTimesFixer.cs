using System;
using System.IO;

namespace GitModTimes
{
    public static class GitModifiedTimesFixer
    {
        public static void FixTimes(string gitDirectory, DateTime missingFileDateTime, DateTimeOffset? stopBefore = null)
        {
            var findResult = GitModifiedTimesFinder.GetTimes(gitDirectory, stopBefore);
            foreach (var fileTime in findResult.FoundFiles)
            {
                File.SetLastWriteTimeUtc(fileTime.Path, fileTime.Time.UtcDateTime);
            }
            foreach (var file in findResult.MissingFiles)
            {
                File.SetLastWriteTimeUtc(file, missingFileDateTime);
            }
        }
    }
}