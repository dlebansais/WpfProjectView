namespace WpfProjectView;

/// <summary>
/// Implements a xaml element.
/// </summary>
/// <inheritdoc/>
internal record XamlElement(string Name, IXamlNamespaceCollection Namespaces, IXamlElementCollection Children, IXamlAttributeCollection Attributes) : IXamlElement
{
}
