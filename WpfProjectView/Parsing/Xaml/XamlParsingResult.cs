namespace WpfProjectView;

/// <summary>
/// Implements the result of the xaml parser.
/// </summary>
public class XamlParsingResult : IXamlParsingResult
{
    /// <inheritdoc/>
    public IXamlElement? Root { get; internal set; }
}
