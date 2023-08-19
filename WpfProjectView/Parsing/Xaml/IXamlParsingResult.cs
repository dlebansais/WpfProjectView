namespace WpfProjectView;

/// <summary>
/// Abstraction of the result of the xaml parser.
/// </summary>
public interface IXamlParsingResult
{
    /// <summary>
    /// Gets the root element.
    /// </summary>
    public IXamlElement? Root { get; }
}
