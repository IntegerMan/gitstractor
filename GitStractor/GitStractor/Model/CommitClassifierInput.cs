namespace GitStractor.Model;

public class CommitClassifierInput {
    public string Message { get; set; }
    public bool IsMerge {get; set;}
    public float WorkItems {get; set;}
    public float TotalFiles {get; set;}
    public float ModifiedFiles {get; set;}
    public float AddedFiles {get; set;}
    public float DeletedFiles {get; set;}
    public float TotalLines {get; set;}
    public float NetLines {get; set;}
    public float AddedLines {get; set;}
    public float DeletedLines {get; set;}
    public bool HasAddedFiles {get; set;}
    public bool HasDeletedFiles {get; set;}
    public float MessageLength {get; set;}
    public float WordCount {get; set;}
    // Note: this doesn't include the Source property since we're not using it in our model. Later steps in the pipeline explicitly ignore it, but not transmitting it here adds a safety layer to prevent that from leaking
}