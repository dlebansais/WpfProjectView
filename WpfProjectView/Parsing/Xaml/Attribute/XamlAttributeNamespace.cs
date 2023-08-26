namespace WpfProjectView;

/// <summary>
/// Implements a xaml element attribute.
/// </summary>
internal record XamlAttributeNamespace(IXamlNamespace Namespace) : IXamlAttribute
{
    /// <inheritdoc/>
    public string Name { get; } = string.Empty;

    /// <inheritdoc/>
    public object? Value { get => Namespace; }
}
