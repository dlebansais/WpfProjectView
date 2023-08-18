namespace WpfProjectView;

/// <summary>
/// Abstraction of a Xaml file with no code behind.
/// </summary>
public interface IXamlResourceFile : IFile
{
    /// <summary>
    /// Gets the file content as a node tree.
    /// </summary>
    object? NoteTree { get; }
}
