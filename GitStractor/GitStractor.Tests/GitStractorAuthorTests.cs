using System.Diagnostics.CodeAnalysis;
using GitStractor.Model;
using GitStractor.Utilities;
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
            AuthorWriter = authorWriter
        };
        using GitDataExtractor extractor = new(options);

        // Act
        extractor.ExtractInformation();

        // Assert
        authorWriter.Authors.ShouldNotBeEmpty();
        authorWriter.Authors.ShouldContain(a => a.Name == "Matt Eland");
        authorWriter.Authors.ShouldContain(a => a.Name == "matt.eland@gmail.com");
    }
}