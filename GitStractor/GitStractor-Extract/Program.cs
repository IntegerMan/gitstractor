namespace GitStractor.Extract;

public class Program {

    public static int Main(string[] args) {
        GitStractorExtract program = new();
        return program.Run(args);
    }
}