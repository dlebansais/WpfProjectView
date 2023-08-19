namespace WpfProjectView;

/// <summary>
/// Implements a xaml element.
/// </summary>
/// <inheritdoc/>
internal record XamlElement(IXamlNamespaceCollection Namespaces, IXamlElementCollection Children, IXamlAttributeCollection Attributes) : IXamlElement
{
}
