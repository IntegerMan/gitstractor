namespace GitStractor.Model;

[Flags]
public enum FileState
{
    Added = 1,
    Deleted = 2,
    Modified = 4,
    Unmodified = 8,
    Final = 16,
}