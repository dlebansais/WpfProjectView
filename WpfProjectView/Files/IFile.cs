namespace WpfProjectView;

using System.Threading.Tasks;
using FolderView;

/// <summary>
/// Abstraction of a file.
/// </summary>
public interface IFile
{
    /// <summary>
    /// Gets the source file.
    /// </summary>
    FolderView.IFile SourceFile { get; }

    /// <summary>
    /// Gets the path to the file.
    /// </summary>
    IPath Path { get; }

    /// <summary>
    /// Loads the file content.
    /// </summary>
    /// <param name="rootFolder">The project root folder.</param>
    Task LoadAsync(IFolder rootFolder);

    /// <summary>
    /// Gets a value indicating whether the file has been parsed.
    /// </summary>
    bool IsParsed { get; }

    /// <summary>
    /// Parses the content.
    /// </summary>
    void Parse();
}
