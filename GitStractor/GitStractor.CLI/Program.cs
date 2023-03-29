﻿using GitStractor.Model;
using GitStractor.Writers;
using LibGit2Sharp;

namespace GitStractor.CLI;

public static class Program
{
    private static void Main(string[] args)
    {
        // First parameter is path, but current directory is used in its absence
        string repositoryPath = args.FirstOrDefault() ?? Environment.CurrentDirectory;
        
        // For now, let's just always dump into the current directory
        string outputDirectory = Environment.CurrentDirectory;
        
        // Analyze the git repository
        try
        {
            GitExtractionOptions options = BuildExtractionOptions(repositoryPath, outputDirectory);
            using GitDataExtractor extractor = new(options);

            extractor.ExtractInformation();
        }
        catch (RepositoryNotFoundException)
        {
            Console.WriteLine($"The repository at {repositoryPath} could not be found.");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"A storage-related error occurred while extracting information: {ex.Message}");
        }
    }

    private static GitExtractionOptions BuildExtractionOptions(string repositoryPath, string outputDirectory) 
        => new()
        {
            RepositoryPath = repositoryPath,
            FileWriter = new FileConsoleDataWriter(),
            CommitWriter = new CommitCompoundDataWriter(new CommitDataWriter[] {
                new CommitConsoleDataWriter(),
                new CommitCsvDataWriter(Path.Combine(outputDirectory, "Commits.csv")),
            }),
            AuthorWriter = new AuthorCompoundDataWriter(new AuthorDataWriter[] {
                new AuthorConsoleDataWriter(),
                new AuthorCsvDataWriter(Path.Combine(outputDirectory, "Authors.csv")),
            })
        };
}