namespace GitStractor.Tests
{
    public class GitStractorAnalyzerTests
    {
        [Fact]
        public void AnalyzingALocalGitRepositoryShouldNotError()
        {
            // Arrange
            GitStractor sut = new();
            string repoPath = GetParentGitDirectory();
            string outputPath = Path.GetTempFileName();

            // Act
            sut.ExtractCommitInformation(repoPath, outputPath);

            // Assert
            Assert.True(true);
        }

        /// <summary>
        /// Starts in the current directory and walks upwards towards the drive root
        /// until it finds a .git directory. Once one is found, it returns the path.
        /// If a .git directory is not found, the test will fail via <c>Assert.Fail</c>.
        /// </summary>
        /// <returns>The git folder containing the active repository</returns>
        private static string GetParentGitDirectory()
        {
            DirectoryInfo? dirInfo = new(Environment.CurrentDirectory);
            while (dirInfo != null && !dirInfo.EnumerateDirectories(".git").Any())
            {
                dirInfo = dirInfo.Parent;
            }

            if (dirInfo == null)
            {
                Assert.Fail("Could not find a .git directory in the current directory or any parent directory.");
            }

            return dirInfo.FullName;
        }
    }
}