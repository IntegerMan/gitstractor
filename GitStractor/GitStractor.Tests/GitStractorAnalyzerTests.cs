using System.Diagnostics.CodeAnalysis;
using GitStractor.Model;
using GitStractor.Utilities;
using GitStractor.Writers;
using Shouldly;

namespace GitStractor.Tests
{
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
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public void AnalyzingALocalGitRepositoryShouldReturnResults()
        {
            // Arrange
            GitExtractionOptions options = new()
            {
                RepositoryPath = Environment.CurrentDirectory,
                AuthorWriter = new AuthorConsoleDataWriter()
            };
            using GitDataExtractor extractor = new(options);

            // Act
            IEnumerable<CommitInfo> commits = extractor.ExtractCommitInformation();

            // Assert
            commits.ShouldNotBeNull();
            commits.ShouldNotBeEmpty();
        }

    }
}