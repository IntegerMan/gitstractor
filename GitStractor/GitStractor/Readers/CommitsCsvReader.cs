using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using GitStractor.Model;

namespace GitStractor.Readers;

public static class CommitsCsvReader
{
    public static IEnumerable<GitCommitRow> ReadCommits(string csvFilePath)
    {
        using (StreamReader reader = new(csvFilePath))
        {
            using (CsvReader csv = new(reader, CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<GitCommitRow>().ToList();
            }
        }
    }
}

public class GitCommitRow
{
    // Sha,ParentSha,Parent2Sha,IsMerge,AuthorId,AuthorDateUtc,CommitterId,CommitterDateUtc,Message,Work Items,Total Files,
    // Modified Files,Added Files,Deleted Files,Total Lines,Net Lines,Added Lines,Deleted Lines
    public string Message { get; set; }
    [Name("Work Items")]
    public int WorkItems { get; set; }
    [Name("Total Files")]
    public int TotalFiles { get; set; }
    [Name("Modified Files")]
    public int ModifiedFiles { get; set; }
    [Name("Added Files")]
    public int AddedFiles { get; set; }
    [Name("Deleted Files")]
    public int DeletedFiles { get; set; }
    [Name("Total Lines")]
    public int TotalLines { get; set; }
    [Name("Net Lines")]
    public int NetLines { get; set; }
    [Name("Added Lines")]
    public int AddedLines { get; set; }
    [Name("Deleted Lines")]
    public int DeletedLines { get; set; }
    public bool IsMerge { get; set; }
}