using System;
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
            GitModifiedTimesFinder.GetTimes(testDir);
            sw.Stop();
            Trace.WriteLine($"DecorateWithLastModifiedCommitMetaData: {sw.ElapsedMilliseconds} ms");
        }
    }

    [Test]
    [Explicit]
    public void SetLastModifiedTime()
    {
        GitModifiedTimesFixer.FixTimes(@"C:\Code\docs.particular.net");
    }

    [Test]
    [Explicit]
    public void SpecificDir()
    {
        using (var repository = new Repository(@"C:\Code\docs.particular.net"))
        {
            var sw = Stopwatch.StartNew();
            var commit = repository.Commits.Skip(2).First();
            var stopBefore = commit.Author.When;
            var list = repository.GetTimes( @"C:\Code\docs.particular.net", stopBefore);
            sw.Stop();
            Trace.WriteLine($"DecorateWithLastModifiedCommitMetaData: {sw.ElapsedMilliseconds} ms. Items: {list.Count}");
            //foreach (var item in list)
            //{
            //    Trace.WriteLine(item.RelativePath);
            //}
        }

    }

    [Test]
    [Explicit]
    public void Foo()
    {
        var sw = Stopwatch.StartNew();
        Process.Start(@"C:\Users\simon\AppData\Local\GitHub\PortableGit_cf76fc1621ac41ad4fe86c420ab5ff403f1808b9\cmd\git.exe", "log -1 --format=\"%ad\" -- Snippets/Edmx/EdmxSnippets/EfEdmx/MySample.Context.tt").WaitForExit();
        sw.Stop();
        Trace.WriteLine($"DecorateWithLastModifiedCommitMetaData: {sw.ElapsedMilliseconds} ms.");
    }

    [Test]
    public void Can_get_last_modified_dates()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "Can_get_last_modified_dates");
        if (Directory.Exists(testDir))
        {
            DirectoryHelper.DeleteDirectory(testDir);
        }

        using (var repository = RepoBuilder.BuildTestRepository(testDir))
        {
            var modifiedTimes = repository.GetTimes(testDir);
            ObjectApprover.VerifyWithJson(modifiedTimes, Scrubber(testDir));
        }
    }

    [Test]
    public void Can_get_last_modified_dates_with_cutoff()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "Can_get_last_modified_dates_with_cutoff");
        if (Directory.Exists(testDir))
        {
            DirectoryHelper.DeleteDirectory(testDir);
        }

        using (var repository = RepoBuilder.BuildSimpleTestRepository(testDir))
        {
            var first = repository.Commits
                .Skip(2)
                .First();
            var modifiedTimes = repository.GetTimes(testDir, first.Author.When);
            ObjectApprover.VerifyWithJson(modifiedTimes, Scrubber(testDir));
        }
    }

    static Func<string, string> Scrubber(string testDir)
    {
        return s => s.Replace(@"\\", @"\")
            .Replace(testDir, "C:");
    }
}