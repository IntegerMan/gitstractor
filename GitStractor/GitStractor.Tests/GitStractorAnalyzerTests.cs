using System.Diagnostics.CodeAnalysis;
using GitStractor.Writers;
using Shouldly;

namespace GitStractor.Tests;

[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
public class GitStractorAnalyzerTests
{
    [Fact]
    public void AnalyzingWithoutOptionsShouldThrowArgNull()
    {
        try
        {
            // Arrange

            // Act
            using GitDataExtractor extractor = new(null!);

            // Assert
            Assert.Fail("An exception should have been thrown but was not");
        }
        catch (ArgumentNullException)
        {
            // This exception is expected
        }
    }
        
    [Fact]
    public void AnalyzingALocalGitRepositoryShouldReturnResults()
    {
        // Arrange
        CommitInMemoryDataWriter commitDataWriter = new();
        GitExtractionOptions options = new()
        {
            RepositoryPath = Environment.CurrentDirectory,
            AuthorWriter = new AuthorConsoleDataWriter(),
            FileWriter = new FileInMemoryDataWriter(),
            CommitWriter = commitDataWriter,
        };
        using GitDataExtractor extractor = new(options);

        // Act
        extractor.ExtractInformation();

        // Assert
        commitDataWriter.ShouldNotBeEmpty();
    }        
    
    [Fact]
    public void AnalyzingALocalGitRepositoryShouldReturnAuthors()
    {
        // Arrange
        CommitInMemoryDataWriter commitDataWriter = new();
        DateTime yearStart = new DateTime(2023, 1, 1);
        GitExtractionOptions options = new()
        {
            RepositoryPath = Environment.CurrentDirectory,
            AuthorWriter = new AuthorConsoleDataWriter(),
            FileWriter = new FileInMemoryDataWriter(),
            CommitWriter = commitDataWriter,
        };
        GitDataExtractor extractor = new(options);

        // Act
        extractor.ExtractInformation();

        // Assert
        commitDataWriter.ShouldAllBe(c => c.Author != null);
        commitDataWriter.ShouldAllBe(c => c.AuthorDateLocal > yearStart);
        commitDataWriter.ShouldAllBe(c => c.Committer != null);
        commitDataWriter.ShouldAllBe(c => c.FileNames != null);
        commitDataWriter.ShouldAllBe(c => c.NumFiles > 0);
        commitDataWriter.ShouldAllBe(c => c.Files.Count > 0);
        commitDataWriter.ShouldAllBe(c => c.CommitterDateLocal > yearStart);
    }    
    
    [Fact]
    public void AnalyzingALocalGitRepositoryShouldReturnCommiters()
    {
        // Arrange
        CommitInMemoryDataWriter commitDataWriter = new();
        DateTime yearStart = new DateTime(2023, 1, 1);
        GitExtractionOptions options = new()
        {
            RepositoryPath = Environment.CurrentDirectory,
            AuthorWriter = new AuthorConsoleDataWriter(),
            FileWriter = new FileInMemoryDataWriter(),
            CommitWriter = commitDataWriter,
        };
        using GitDataExtractor extractor = new(options);

        // Act
        extractor.ExtractInformation();

        // Assert
        commitDataWriter.ShouldAllBe(c => c.Committer != null);
        commitDataWriter.ShouldAllBe(c => c.CommitterDateLocal > yearStart);
        commitDataWriter.ShouldAllBe(c => c.FileNames != null);
        commitDataWriter.ShouldAllBe(c => c.NumFiles > 0);
        commitDataWriter.ShouldAllBe(c => c.Files.Count > 0);
    }    
    
    [Fact]
    public void AnalyzingALocalGitRepositoryShouldReturnFiles()
    {
        // Arrange
        CommitInMemoryDataWriter commitDataWriter = new();
        GitExtractionOptions options = new()
        {
            RepositoryPath = Environment.CurrentDirectory,
            AuthorWriter = new AuthorConsoleDataWriter(),
            FileWriter = new FileInMemoryDataWriter(),
            CommitWriter = commitDataWriter,
        };
        using GitDataExtractor extractor = new(options);

        // Act
        extractor.ExtractInformation();

        // Assert
        commitDataWriter.ShouldAllBe(c => c.NumFiles > 0);
        commitDataWriter.ShouldAllBe(c => c.Files.Count > 0);
        commitDataWriter.ShouldAllBe(c => c.FileNames != null);
    }

}