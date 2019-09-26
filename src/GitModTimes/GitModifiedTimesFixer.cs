using System;
using System.IO;
using LibGit2Sharp;

namespace GitModTimes
{
    public static class GitModifiedTimesFixer
    {
        public static void FixTimes(
            string gitDirectory,
            DateTimeOffset missingFileDateTime,
            IncludeFile? includeFile = null,
            DateTimeOffset? stopBefore = null)
        {
            using (var repository = new Repository(gitDirectory))
            {
                repository.FixTimes(gitDirectory, missingFileDateTime, includeFile, stopBefore);
            }
        }

        public static void FixTimes(
            this Repository repository,
            string gitDirectory,
            DateTimeOffset missingFileDateTime,
            IncludeFile? includeFile = null,
            DateTimeOffset? stopBefore = null)
        {
            var findResult = repository.GetTimes(gitDirectory, includeFile, stopBefore);
            foreach (var fileTime in findResult.FoundFiles)
            {
                File.SetLastWriteTimeUtc(fileTime.Path, fileTime.Time.UtcDateTime);
            }
            var utcDateTime = missingFileDateTime.UtcDateTime;
            foreach (var file in findResult.MissingFiles)
            {
                File.SetLastWriteTimeUtc(file, utcDateTime);
            }
        }
    }
}