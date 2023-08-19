namespace WpfProjectView;

/// <summary>
/// Implements a xaml element attribute.
/// </summary>
/// <inheritdoc/>
internal abstract record XamlAttribute(string Name, object? Value) : IXamlAttribute
{
}
