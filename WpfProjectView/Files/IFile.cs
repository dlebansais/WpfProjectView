namespace WpfProjectView;

using FolderView;

/// <summary>
/// Abstraction of a file.
/// </summary>
public interface IFile
{
    /// <summary>
    /// Gets the path to the file.
    /// </summary>
    IPath Path { get; }
}
