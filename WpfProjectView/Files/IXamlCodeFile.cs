namespace WpfProjectView;

using System.Threading.Tasks;
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
    /// Gets the xaml source file.
    /// </summary>
    FolderView.IFile XamlSourceFile { get; }

    /// <summary>
    /// Gets the C# code file.
    /// </summary>
    FolderView.IFile CodeSourceFile { get; }

    /// <summary>
    /// Gets the result of parsing the xaml file.
    /// </summary>
    IXamlParsingResult? XamlParsingResult { get; }

    /// <summary>
    /// Gets the code syntax tree.
    /// </summary>
    SyntaxTree? SyntaxTree { get; }
}
