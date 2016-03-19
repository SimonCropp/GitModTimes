using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

static class Extensions
{
    internal static Dictionary<string, ObjectId> GetTipBlobIds(this Repository repository, IEnumerable<string> paths)
    {
        var tip = repository.Head.Tip;
        return paths.ToDictionary(
            article => article,
            article => tip.RetrieveBlobObjectId(article));
    }

    internal static void ThrowIfUnborn(this Repository repository)
    {
        if (repository.Info.IsHeadUnborn)
        {
            throw new UnbornBranchException();
        }
    }
    internal static ObjectId RetrieveBlobObjectId(this Commit commit, string path)
    {
        var treeEntry = commit[path];
        if (treeEntry == null)
        {
            return null;
        }

        if (treeEntry.TargetType == TreeEntryTargetType.Blob)
        {
            return treeEntry.Target.Id;
        }

        throw new Exception($"Path '{path}' doesn't lead to a Blob in commit {commit.Id.ToString(7)}.");
    }

}