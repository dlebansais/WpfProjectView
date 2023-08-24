namespace WpfProjectView;

/// <summary>
/// Implements the result of the xaml parser.
/// </summary>
internal class XamlParsingResult : IXamlParsingResult
{
    /// <inheritdoc/>
    public IXamlElement? Root { get; internal set; }
}
