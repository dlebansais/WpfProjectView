namespace WpfProjectView;

/// <summary>
/// Implements a xaml element attribute.
/// </summary>
internal record XamlAttributeDirective(IXamlNamespace Namespace, string Name, object? Value) : IXamlAttribute
{
}
