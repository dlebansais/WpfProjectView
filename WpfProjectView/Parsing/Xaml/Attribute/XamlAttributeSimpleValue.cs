namespace WpfProjectView;

/// <summary>
/// Implements a xaml element attribute.
/// </summary>
internal record XamlAttributeSimpleValue(string StringValue) : IXamlAttribute
{
    /// <inheritdoc/>
    public string Name { get; } = string.Empty;

    /// <inheritdoc/>
    public object? Value { get => StringValue; }
}
