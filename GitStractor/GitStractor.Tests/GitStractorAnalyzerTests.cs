using System.Diagnostics.CodeAnalysis;
using GitStractor.Model;
using GitStractor.Utilities;
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
                GitDataExtractor extractor = new();

                // Act
                _ = extractor.ExtractCommitInformation(null!).ToList();

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
                RepositoryPath = FileUtilities.GetParentGitDirectory()
            };
            GitDataExtractor extractor = new();

            // Act
            IEnumerable<CommitInfo> commits = extractor.ExtractCommitInformation(options);

            // Assert
            commits.ShouldNotBeNull();
            commits.ShouldNotBeEmpty();
        }

    }
}