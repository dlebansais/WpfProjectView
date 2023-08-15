namespace WpfProjectView;

using FolderView;

/// <summary>
/// Represents a file of unknown type.
/// </summary>
internal record OtherFile(IPath Path) : File(Path)
{
}
