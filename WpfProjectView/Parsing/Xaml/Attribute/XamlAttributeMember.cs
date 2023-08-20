namespace WpfProjectView;

/// <summary>
/// Implements a xaml element attribute.
/// </summary>
internal record XamlAttributeMember(string Name, object? Value) : IXamlAttribute
{
}
