namespace WpfProjectView;

using Microsoft.CodeAnalysis;

/// <summary>
/// Abstraction of a C# code file.
/// </summary>
public interface ICodeFile : IFile
{
    /// <summary>
    /// Gets the code syntax tree.
    /// </summary>
    SyntaxTree? SyntaxTree { get; }
}
