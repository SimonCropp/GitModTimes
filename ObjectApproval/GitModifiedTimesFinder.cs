using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace GitModTimes
{
    public static class GitModifiedTimesFinder
    {
        public static List<FileTime> GetModifiedDates(string gitDirectory, IncludeFile includeFile)
        {
            var allRelativePaths = GetAllRelativePaths(gitDirectory, includeFile)
                .ToList();

            var fileTimes = new List<FileTime>();
            using (var repository = new Repository(gitDirectory))
            {
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

                    foreach (var p in parents.OrderByDescending(c => c.Committer.When))
                    {
                        if (seen.Add(p.Id))
                        {
                            queue.Enqueue(p);
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
                            // No parent. The article has been created in this commit
                            fileTimes.Add(CreateFileTime(current, relativePath));
                            allRelativePaths.Remove(relativePath);
                            continue;
                        }

                        var parBlobId = parent.RetrieveBlobObjectId(relativePath.GitPath);
                        if (parBlobId == null)
                        {
                            // The parent doesn't know about the article
                            // The article has been created in this commit
                            fileTimes.Add(CreateFileTime(current, relativePath));
                            allRelativePaths.Remove(relativePath);
                            continue;
                        }

                        var curBlobId = tipBlobIds[relativePath.GitPath];
                        if (curBlobId != parBlobId)
                        {
                            // The article has been updated in this commit
                            fileTimes.Add(CreateFileTime(current, relativePath));
                            allRelativePaths.Remove(relativePath);
                        }
                    }
                }
            }
            return fileTimes;
        }


        static FileTime CreateFileTime(Commit current, LinkedPath path)
        {
            return new FileTime
            {
                Time = current.Committer.When,
                Path = path.OriginalPath,
                RelativePath = path.GitPath
            };
        }


        static IEnumerable<LinkedPath> GetAllRelativePaths(string directory, IncludeFile includeFile)
        {
            return Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
                .Where(s => !s.Contains(".git") && includeFile(s))
                .Select(file => new LinkedPath
                {
                    OriginalPath = file,
                    GitPath = file.Replace(directory, "")
                        .TrimStart(Path.DirectorySeparatorChar)
                });
        }
    }
}