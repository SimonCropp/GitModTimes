using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace GitModTimes
{
    public static class GitModifiedTimesFinder
    {
        public static FindResult GetTimes(string gitDirectory, IncludeFile? includeFile = null, DateTimeOffset? stopBefore = null)
        {
            using var repository = new Repository(gitDirectory);
            return repository.GetTimes(gitDirectory, includeFile, stopBefore);
        }

        public static FindResult GetTimes(this Repository repository, string gitDirectory, IncludeFile? includeFile = null, DateTimeOffset? stopBefore = null)
        {
            var allRelativePaths = repository.GetAllRelativePaths(gitDirectory, includeFile, stopBefore)
                .ToList();

            var fileTimes = new List<FileTime>();
            repository.ThrowIfUnborn();

            var tipBlobIds = repository.GetTipBlobIds(allRelativePaths.Select(x => x.GitPath));

            var seen = new HashSet<ObjectId>();
            var queue = new Queue<Commit>();

            var current = repository.Head.Tip;
            queue.Enqueue(current);
            seen.Add(current.Id);

            while (allRelativePaths.Count > 0 && queue.Count > 0)
            {
                current = queue.Dequeue();
                var parents = current.Parents.ToList();

                foreach (var commit in parents.OrderByDescending(c => c.Committer.When))
                {
                    if (stopBefore != null && commit.Author.When <= stopBefore.Value)
                    {
                        break;
                    }

                    if (seen.Add(commit.Id))
                    {
                        queue.Enqueue(commit);
                    }
                }

                // Merge commits are ignored. The hypothesis is that no conflict is solved
                // by hand by the person performing the merge (as it's mostly done through the GitHub Ui).
                // As such, the modified files should appear in the parent commits as well.
                // This workaround avoids using the merge signature (ie. Identity and time of the merge commit)
                // as the pivot to detect the most recent changes.
                if (parents.Count > 1)
                {
                    continue;
                }

                var parent = parents.FirstOrDefault();

                for (var i = allRelativePaths.Count - 1; i >= 0; i--)
                {
                    var relativePath = allRelativePaths[i];

                    if (parent == null)
                    {
                        // No parent. The file has been created in this commit
                        fileTimes.Add(current.CreateFileTime(relativePath));
                        allRelativePaths.Remove(relativePath);
                        continue;
                    }

                    var parBlobId = parent.RetrieveBlobObjectId(relativePath.GitPath);
                    if (parBlobId == null)
                    {
                        // The parent doesn't know about the file
                        // The file has been created in this commit
                        fileTimes.Add(current.CreateFileTime(relativePath));
                        allRelativePaths.Remove(relativePath);
                        continue;
                    }

                    var curBlobId = tipBlobIds[relativePath.GitPath];
                    if (curBlobId != parBlobId)
                    {
                        // The file has been updated in this commit
                        fileTimes.Add(current.CreateFileTime(relativePath));
                        allRelativePaths.Remove(relativePath);
                    }
                }
            }

            return new FindResult(
                foundFiles: fileTimes,
                missingFiles: allRelativePaths.Select(x => x.OriginalPath).ToList());
        }

        static FileTime CreateFileTime(this Commit current, LinkedPath path) =>
            new(
                time: current.Committer.When,
                path: path.OriginalPath,
                relativePath: path.GitPath
            );

        static IEnumerable<LinkedPath> GetAllRelativePaths(this Repository repository, string directory, IncludeFile? includeFile = null, DateTimeOffset? stopBefore = null)
        {
            return from file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
                where
                    !file.Contains(".git") &&
                    (includeFile == null || includeFile(file)) &&
                    (stopBefore == null || File.GetLastWriteTimeUtc(file) > stopBefore.Value.UtcDateTime)
                let gitPath = GetRelativePath(directory, file)
                where !repository.Ignore.IsPathIgnored(gitPath)
                select new LinkedPath
                (
                    originalPath: file,
                    gitPath: gitPath
                );
        }

        static string GetRelativePath(string directory, string file)
        {
            return file.Replace(directory, "").TrimStart(Path.DirectorySeparatorChar);
        }
    }
}