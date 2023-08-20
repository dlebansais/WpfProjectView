namespace WpfProjectView;

/// <summary>
/// Implements a xaml element attribute.
/// </summary>
internal record XamlAttributeElementCollection(string Name, XamlElementCollection Children) : IXamlAttribute
{
    /// <inheritdoc/>
    public object? Value { get => Children; }
}
