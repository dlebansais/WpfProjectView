namespace WpfProjectView;

using FolderView;
using Microsoft.CodeAnalysis;

/// <summary>
/// Abstraction of a Xaml file with code behind.
/// </summary>
public interface IXamlCodeFile : IFile
{
    /// <summary>
    /// Gets the path to the code behind file.
    /// </summary>
    IPath CodeBehindPath { get; }

    /// <summary>
    /// Gets the file content as a tree of xaml elements.
    /// </summary>
    IXamlElement? RootElement { get; }

    /// <summary>
    /// Gets the code syntax tree.
    /// </summary>
    SyntaxTree? SyntaxTree { get; }
}
