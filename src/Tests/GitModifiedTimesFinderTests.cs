using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitModTimes;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

public class GitModifiedTimesFinderTests:
    VerifyBase
{
    DateTimeOffset epoch = new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    public GitModifiedTimesFinderTests(ITestOutputHelper output) :
        base(output)
    {
        var testDir = Path.Combine(Path.GetTempPath(), "GitModTimes");
        Directory.CreateDirectory(testDir);
        foreach (var directory in Directory.EnumerateDirectories(testDir))
        {
            DirectoryHelper.DeleteDirectory(directory);
        }
    }

    [Fact]
    public Task Can_get_last_modified_dates()
    {
        var testDir = CreateTestDir("Can_get_last_modified_dates");
        AddScrubber(Scrubber(testDir));
        using var repository = RepoBuilder.BuildTestRepository(testDir);
        var modifiedTimes = repository.GetTimes(testDir);
        return Verify(modifiedTimes);
    }

    [Fact]
    public Task Can_fix_dates()
    {
        var testDir = CreateTestDir("Can_fix_dates");
        AddScrubber(Scrubber(testDir));
        using (var repository = RepoBuilder.BuildSimpleTestRepository(testDir))
        {
            repository.FixTimes(testDir, epoch);
        }
        return Verify(GetNonGitFiles(testDir));
    }

    static string CreateTestDir(string suffix)
    {
        return Path.Combine(Path.GetTempPath(), "GitModTimes", suffix);
    }

    [Fact]
    public Task Can_fix_dates_with_cutoff()
    {
        var testDir = CreateTestDir("Can_fix_dates_with_cutoff");
        AddScrubber(Scrubber(testDir));
        using var repository = RepoBuilder.BuildSimpleTestRepository(testDir);
        var commit = repository.Commits
            .Skip(2)
            .First();
        repository.FixTimes(testDir, epoch, null, commit.Author.When);
        return Verify(GetNonGitFiles(testDir));
    }

    static IEnumerable<Tuple<string, DateTime>> GetNonGitFiles(string directory)
    {
        return from file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
            where !file.Contains(".git")
            select new Tuple<string, DateTime>(file, File.GetLastWriteTimeUtc(file));
    }

    [Fact]
    public Task Can_get_last_modified_dates_with_cutoff()
    {
        var testDir = CreateTestDir("Can_get_last_modified_dates_with_cutoff");
        AddScrubber(Scrubber(testDir));
        using var repository = RepoBuilder.BuildSimpleTestRepository(testDir);
        var commit = repository.Commits
            .Skip(2)
            .First();
        var modifiedTimes = repository.GetTimes(testDir, null, commit.Author.When);

        return Verify(modifiedTimes);
    }

    static Func<string, string> Scrubber(string testDir)
    {
        return s => s.Replace(@"\\", @"\")
            .Replace(testDir, "C:");
    }
}