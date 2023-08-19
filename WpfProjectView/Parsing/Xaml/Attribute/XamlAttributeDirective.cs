namespace WpfProjectView;

/// <summary>
/// Implements a xaml element attribute.
/// </summary>
internal record XamlAttributeDirective(string Prefix, string Name, object? Value) : XamlAttribute(Name, Value)
{
}
