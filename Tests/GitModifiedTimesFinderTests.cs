using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GitModTimes;
using LibGit2Sharp;
using NUnit.Framework;

[TestFixture]
public class GitModifiedTimesFinderTests
{
    [Test]
    [Explicit]
    public void PerformanceMeasurement()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "PerformanceMeasurement");
        if (Directory.Exists(testDir))
        {
            DirectoryHelper.DeleteDirectory(testDir);
        }
        Trace.WriteLine("Cloning...");
        var sw = Stopwatch.StartNew();
        Repository.Clone("https://github.com/Particular/docs.particular.net/", testDir);
        sw.Stop();
        Trace.WriteLine($"Clone: {sw.ElapsedMilliseconds} ms");

        for (var i = 0; i < 5; i++)
        {
            Trace.WriteLine("Decorating...");
            sw.Restart();
            GitModifiedTimesFinder.GetModifiedDates(testDir, path => path.EndsWith(".md"));
            sw.Stop();
            Trace.WriteLine($"DecorateWithLastModifiedCommitMetaData: {sw.ElapsedMilliseconds} ms");
        }
    }

    [Test]
    [Explicit]
    public void Foo()
    {
        var sw = Stopwatch.StartNew();
        var repository = new Repository(@"C:\Code\docs.particular.net");
        Commit previous = null;
        foreach (var commit in repository.Commits)
        {
            if (previous != null)
            {
                var patch = repository.Diff.Compare<Patch>(commit.Tree, previous.Tree);
                foreach (var ptc in patch)
                {
                    //Debug.WriteLine(ptc.Status + " -> " + ptc.Path); // Status -> File Path
                }
            }
            Debug.WriteLine(commit.Author.When);
            previous = commit;
        }
        sw.Stop();
        Trace.WriteLine($"DecorateWithLastModifiedCommitMetaData: {sw.ElapsedMilliseconds} ms");
    }
    [Test]
    [Explicit]
    public void SpecificDir()
    {
        var sw = Stopwatch.StartNew();

        var list = GitModifiedTimesFinder.GetModifiedDates(@"C:\Code\docs.particular.net", path => path.EndsWith(".cs"));
        sw.Stop();
        Trace.WriteLine($"DecorateWithLastModifiedCommitMetaData: {sw.ElapsedMilliseconds} ms");
        foreach (var item in list)
        {
            Debug.WriteLine(item.Time);
        }
    }

    [Test]
    public void Can_get_last_modified_dates()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "Can_get_last_modified_dates");
        if (Directory.Exists(testDir))
        {
            DirectoryHelper.DeleteDirectory(testDir);
        }

        BuildTestRepository(testDir);

        AssertLastModifiedAt(testDir,
            new[]
            {
                new[]
                {
                    "One.md",
                    "2012-04-17T08:16:35+02:00"
                },
            });

        AssertLastModifiedAt(testDir,
            new[]
            {
                new[]
                {
                    "One.md",
                    "2012-04-17T08:16:35+02:00"
                },
                new[]
                {
                    "Another.md",
                    "2012-04-17T08:12:35+02:00"
                },
            });

        AssertLastModifiedAt(testDir,
            new[]
            {
                new[]
                {
                    "Two.md",
                    "2012-04-17T08:20:35+02:00"
                },
                new[]
                {
                    "Three.md",
                    "2012-04-17T08:22:35+02:00"
                },
            });

        AssertLastModifiedAt(testDir,
            new[]
            {
                new[]
                {
                    "Two.md",
                    "2012-04-17T08:20:35+02:00"
                },
                new[]
                {
                    "Three.md",
                    "2012-04-17T08:22:35+02:00"
                },
                new[]
                {
                    "One.md",
                    "2012-04-17T08:16:35+02:00"
                },
                new[]
                {
                    "Another.md",
                    "2012-04-17T08:12:35+02:00"
                },
            });

        AssertLastModifiedAt(testDir,
            new[]
            {
                new[]
                {
                    @"productA\toc.md",
                    "2012-04-17T08:23:39+02:00"
                },
                new[]
                {
                    @"productB\toc.md",
                    "2012-04-17T08:23:39+02:00"
                },
                new[]
                {
                    @"productB\subproduct\toc.md",
                    "2012-04-17T08:23:42+02:00"
                }
            });
    }

    void AssertLastModifiedAt(string gitDirectory, string[][] expectations)
    {

        var lastModifiedTimes = GitModifiedTimesFinder.GetModifiedDates(gitDirectory, path => path.EndsWith(".md"))
            .ToList();

        foreach (var expectation in expectations)
        {
            var expectedPath = expectation[0];
            var lastModified = lastModifiedTimes.Single(a => a.RelativePath == expectedPath);

            Assert.NotNull(lastModified);

            var date = DateTimeOffset.Parse(expectation[1]);
            Assert.AreEqual(date, lastModified.Time);
        }
    }

    void BuildTestRepository(string directory)
    {
        Repository.Init(directory);

        using (var repository = new Repository(directory))
        {
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
        }

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
        var sign = new Signature(name, email, DateTimeOffset.Parse(date));

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

                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                }
            }

            var filePath = Path.Combine(repository.Info.WorkingDirectory, change[0]);

            File.AppendAllText(filePath, change[1]);

            repository.Stage(change[0]);
        }

        return repository.Commit("Doc update", sign, sign);
    }
}
