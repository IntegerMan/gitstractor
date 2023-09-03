using GitStractor.Model;

namespace GitStractor.GitObservers;

public interface IGitObserver
{
    void OnBeginningIteration(int totalCommits, string outputPath);
    void OnCompletedIteration(string outputPath);
    void OnNewAuthor(AuthorInfo author);
    void OnProcessingCommit(string sha, bool isLast);
    void OnProcessedCommit(CommitInfo commit);
    void OnProcessingFile(RepositoryFileInfo fileInfo, CommitInfo commitInfo);
    void UpdateProgress(double percent, int commitNum, double totalCommits);
}
