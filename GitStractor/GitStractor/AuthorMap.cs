namespace GitStractor; 

public class AuthorMap {

    public string Name { get; set; }
    public bool Bot { get; set; }

    public List<string> Emails { get; init; } = new();
}