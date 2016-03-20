using System;
using System.Collections.Generic;
using System.IO;
using LibGit2Sharp;

public class RepoBuilder
{

    public static Repository BuildSimpleTestRepository(string directory)
    {
        Repository.Init(directory);

        var repository = new Repository(directory);
            AddCommit(repository, "One", "a@a", "2012-04-17T08:12:35+02:00",
                new[]
                    {
                        new[] {"One.md", "a"},
                    });

            AddCommit(repository, "Two", "b@b", "2012-04-17T08:14:35+02:00",
                new[]
                    {
                        new[] {"Two.md", "a"},
                    });

            AddCommit(repository, "Three", "c@c", "2012-04-17T08:22:35+02:00",
                new[]
                    {
                        new[] {"Three.md", "a"},
                    });
        return repository;
    }
    public static Repository BuildTestRepository(string directory)
    {
        Repository.Init(directory);

        var repository = new Repository(directory);
            AddCommit(repository, "A", "a@a", "2012-04-17T08:12:35+02:00",
                new[]
                    {
                        new[] {"One.md", "a"},
                        new[] {"Another.md", "a"},
                    });

            AddCommit(repository, "B", "b@b", "2012-04-17T08:14:35+02:00",
                new[]
                    {
                        new[] {"Two.md", "a"},
                    });

            AddCommit(repository, "C", "c@c", "2012-04-17T08:16:35+02:00",
                new[]
                    {
                        new[] {"One.md", "b"},
                    });

            AddCommit(repository, "A", "a@a", "2012-04-17T08:18:35+02:00",
                new[]
                    {
                        new[] {"Orphan.md", "b"},
                    });

            AddMergeCommit(repository, "B", "b@b", "2012-04-17T08:20:35+02:00",
                new[]
                    {
                        new[] {"Two.md", "b"},
                    });

            AddCommit(repository, "C", "c@c", "2012-04-17T08:22:35+02:00",
                new[]
                    {
                        new[] {"Three.md", "a"},
                    });

            AddCommit(repository, "D", "d@d", "2012-04-17T08:23:39+02:00",
                new[]
                {
                    new[] {@"productA\toc.md", "a"},
                    new[] {@"productB\toc.md", "a"}
                });

            AddCommit(repository, "D", "d@d", "2012-04-17T08:23:42+02:00",
                new[]
                {
                    new[] {@"productB\subproduct\toc.md", "a"}
                });
        return repository;
    }

    static void AddMergeCommit(IRepository repository, string name, string email, string date, IEnumerable<string[]> content)
    {
        var formerHead = repository.Head.Tip;
        var c = AddCommit(repository, name, email, date, content);

        var sign = new Signature(c.Author.Name, c.Author.Email, c.Author.When.AddSeconds(17));
        var m = repository.ObjectDatabase.CreateCommit(sign, sign, "merge", c.Tree, new[] { formerHead, c }, false);

        repository.Refs.UpdateTarget(repository.Refs.Head.ResolveToDirectReference(), m.Id);
    }

    static Commit AddCommit(IRepository repository, string name, string email, string date, IEnumerable<string[]> content)
    {
        var dateTimeOffset = DateTimeOffset.Parse(date);
        var sign = new Signature(name, email, dateTimeOffset);

        foreach (var change in content)
        {
            // check if the change is in a sub directory...
            var pathList = change[0].Split(Path.DirectorySeparatorChar);

            if (pathList.Length > 1)
            {
                var directory = string.Empty;

                for (var i = 0; i < pathList.Length - 1; i++)
                {
                    directory += directory.Length == 0 ? pathList[i] : @"\" + pathList[i];

                    var dir = Path.Combine(repository.Info.WorkingDirectory, directory);

                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
            }

            var filePath = Path.Combine(repository.Info.WorkingDirectory, change[0]);

            File.AppendAllText(filePath, change[1]);

            File.SetLastWriteTimeUtc(filePath, dateTimeOffset.DateTime);
            repository.Stage(change[0]);
        }

        return repository.Commit(name, sign, sign);
    }
}
