using GitStractor.Model;

namespace GitStractor;

public interface IGitObserver {
    void OnBeginningIteration(int totalCommits, string outputPath);
    void OnCompletedIteration(string outputPath);
    void OnNewAuthor(AuthorInfo author);
    void OnProcessingCommit(CommitInfo commit, bool isLast);
    void UpdateProgress(double percent, int commitNum, double totalCommits);
}
