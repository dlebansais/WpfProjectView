namespace WpfProjectView;

/// <summary>
/// Abstraction of a Xaml file with no code behind.
/// </summary>
public interface IXamlResourceFile : IFile
{
    /// <summary>
    /// Gets the file content.
    /// </summary>
    byte[]? Content { get; }
}
