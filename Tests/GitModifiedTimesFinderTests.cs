using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ApprovalTests.Reporters;
using GitModTimes;
using LibGit2Sharp;
using NUnit.Framework;
using ObjectApproval;

[TestFixture]
[UseReporter(typeof(DiffReporter), typeof(AllFailingTestsClipboardReporter))]
public class GitModifiedTimesFinderTests
{
    DateTimeOffset epoch = new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    public GitModifiedTimesFinderTests()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "GitModTimes");
        Directory.CreateDirectory(testDir);
        foreach (var directory in Directory.EnumerateDirectories(testDir))
        {
            DirectoryHelper.DeleteDirectory(directory);
        }
    }

    [Test]
    [Explicit]
    public void SetLastModifiedTimeSpecificDir()
    {
        GitModifiedTimesFixer.FixTimes(@"C:\Code\docs.particular.net", DateTimeOffset.UtcNow);
    }

    [Test]
    [Explicit]
    public void SpecificDir()
    {
        using (var repository = new Repository(@"C:\Code\docs.particular.net"))
        {
            var sw = Stopwatch.StartNew();
            var list = repository.GetTimes(@"C:\Code\docs.particular.net");
            sw.Stop();
            Trace.WriteLine($"GetTimes: {sw.ElapsedMilliseconds} ms. Items: {list.FoundFiles.Count}");
        }
    }

    [Test]
    public void Can_get_last_modified_dates()
    {
        var testDir = CreateTestDir("Can_get_last_modified_dates");
        using (var repository = RepoBuilder.BuildTestRepository(testDir))
        {
            var modifiedTimes = repository.GetTimes(testDir);
            ObjectApprover.VerifyWithJson(modifiedTimes, Scrubber(testDir));
        }
    }

    [Test]
    public void Can_fix_dates()
    {
        var testDir = CreateTestDir("Can_fix_dates");
        using (var repository = RepoBuilder.BuildSimpleTestRepository(testDir))
        {
            repository.FixTimes(testDir, epoch);
        }
        ObjectApprover.VerifyWithJson(GetNonGitFiles(testDir), Scrubber(testDir));
    }

    static string CreateTestDir(string suffix)
    {
        return Path.Combine(Path.GetTempPath(), "GitModTimes", suffix);
    }

    [Test]
    public void Can_fix_dates_with_cutoff()
    {
        var testDir = CreateTestDir("Can_fix_dates_with_cutoff");
        using (var repository = RepoBuilder.BuildSimpleTestRepository(testDir))
        {
            var commit = repository.Commits
                .Skip(2)
                .First();
            repository.FixTimes(testDir, epoch, commit.Author.When);
            ObjectApprover.VerifyWithJson(GetNonGitFiles(testDir), Scrubber(testDir));
        }
    }

    static IEnumerable<Tuple<string, DateTime>> GetNonGitFiles(string directory)
    {
        return from file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
            where !file.Contains(".git")
            select new Tuple<string, DateTime>(file, File.GetLastWriteTimeUtc(file));
    }

    [Test]
    public void Can_get_last_modified_dates_with_cutoff()
    {
        var testDir = CreateTestDir("Can_get_last_modified_dates_with_cutoff");
        using (var repository = RepoBuilder.BuildSimpleTestRepository(testDir))
        {
            var commit = repository.Commits
                .Skip(2)
                .First();
            var modifiedTimes = repository.GetTimes(testDir, commit.Author.When);
            ObjectApprover.VerifyWithJson(modifiedTimes, Scrubber(testDir));
        }
    }

    static Func<string, string> Scrubber(string testDir)
    {
        return s => s.Replace(@"\\", @"\")
            .Replace(testDir, "C:");
    }
}