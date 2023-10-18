namespace GitStractor.Acquire;

public class Program  {

    public static int Main(string[] args) {
        GitStractorAcquire program = new();
        return program.Run(args);
    }
}