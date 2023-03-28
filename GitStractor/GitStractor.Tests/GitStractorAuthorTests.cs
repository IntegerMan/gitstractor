using GitStractor.Writers;
using Shouldly;

namespace GitStractor.Tests;

public class GitStractorAuthorTests
{
    [Fact]
    public void AnalyzingAddsAuthorsCorrectly()
    {
        // Arrange
        AuthorInMemoryDataWriter authorWriter = new();
        GitExtractionOptions options = new()
        {
            RepositoryPath = Environment.CurrentDirectory,
            CommitWriter = new CommitConsoleDataWriter(),
            AuthorWriter = authorWriter
        };
        using GitDataExtractor extractor = new(options);

        // Act
        extractor.ExtractInformation();

        // Assert
        authorWriter.Authors.ShouldNotBeEmpty();
        authorWriter.Authors.ShouldContain(a => a.Name == "Matt Eland");
        authorWriter.Authors.ShouldContain(a => a.Email == "matt.eland@gmail.com");
    }
}