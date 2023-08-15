namespace WpfProjectView;

using FolderView;

/// <summary>
/// Abstraction of a Xaml file with code behind.
/// </summary>
public interface IXamlCodeFile : IFile
{
    /// <summary>
    /// Gets the path to the code behind file.
    /// </summary>
    IPath CodeBehindPath { get; }
}
