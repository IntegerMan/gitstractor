using MattEland.GitStractor;

public class Program
{
    private static void Main(string[] args)
    {
        string path = "C:\\Dev\\GitStractor";
        string outputPath = Path.Combine(Environment.CurrentDirectory, "output.csv");
        
        GitStractor gitstractor = new();
        gitstractor.ExtractCommitInformation(path, outputPath);
    }
}