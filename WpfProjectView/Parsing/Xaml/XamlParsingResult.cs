namespace WpfProjectView;

/// <summary>
/// Implements the result of the xaml parser.
/// </summary>
public class XamlParsingResult : IXamlParsingResult
{
    /// <summary>
    /// Gets the root element.
    /// </summary>
    public IXamlElement? Root { get; internal set; }
}
