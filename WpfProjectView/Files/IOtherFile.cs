namespace WpfProjectView;

/// <summary>
/// Abstraction of a file of unknown type.
/// </summary>
public interface IOtherFile : IFile
{
    /// <summary>
    /// Gets the file content.
    /// </summary>
    byte[]? Content { get; }
}
