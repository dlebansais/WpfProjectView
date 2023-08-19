namespace WpfProjectView;

/// <summary>
/// Abstraction of a Xaml file with no code behind.
/// </summary>
public interface IXamlResourceFile : IFile
{
    /// <summary>
    /// Gets the result of parsing the xaml file.
    /// </summary>
    IXamlParsingResult? XamlParsingResult { get; }
}
