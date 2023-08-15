namespace WpfProjectView;

using FolderView;

/// <summary>
/// Represents a C# code file.
/// </summary>
internal record CodeFile(IPath Path) : File(Path)
{
}
