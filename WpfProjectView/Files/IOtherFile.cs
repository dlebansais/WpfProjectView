namespace WpfProjectView;

using System.IO;

/// <summary>
/// Abstraction of a file of unknown type.
/// </summary>
public interface IOtherFile : IFile
{
    /// <summary>
    /// Gets the file content.
    /// </summary>
    Stream? Content { get; }
}
