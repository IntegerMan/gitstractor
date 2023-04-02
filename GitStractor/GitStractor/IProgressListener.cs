namespace GitStractor;

public interface IProgressListener
{
    void Started(string statusText);
    void UpdateProgress(double percentComplete, string statusText);
    void Completed();
}