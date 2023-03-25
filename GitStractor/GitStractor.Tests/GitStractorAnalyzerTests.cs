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
                GitDataExtractor.ExtractCommitInformation(null!);

                // Assert
                Assert.Fail("An exception should have been thrown but was not");
            }
            catch (ArgumentNullException)
            {
                // This exception is expected
            }
        }
        
        [Fact]
        public void AnalyzingALocalGitRepositoryShouldNotError()
        {
            // Arrange
            GitExtractionOptions options = new()
            {
                RepositoryPath = FileUtilities.GetParentGitDirectory()
            };

            // Act
            GitDataExtractor.ExtractCommitInformation(options);

            // Assert
            Assert.True(true);
        }

    }
}