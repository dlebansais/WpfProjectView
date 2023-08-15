namespace WpfProjectView;

using FolderView;

/// <summary>
/// Represents a file.
/// </summary>
internal abstract record File(IPath Path) : IFile
{
}
