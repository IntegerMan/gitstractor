namespace GitStractor.Model;

[Flags]
public enum FileState
{
    Added = 1,
    Deleted = 2,
    Modified = 4,
    Unmodified = 8,
    Final = 16,
    Renamed = 32,
    Copied = 64,
    Conflicted = 128,
    Ignored = 256,
    Untracked = 512,
    TypeChanged = 1024,
    Unreadable = 2048,
}