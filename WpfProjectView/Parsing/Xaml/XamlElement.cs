namespace WpfProjectView;

/// <summary>
/// Implements a xaml element.
/// </summary>
/// <inheritdoc/>
internal record XamlElement(IXamlNamespaceCollection Namespaces, IXamlNamespace Namespace, string Name, IXamlElementCollection Children, IXamlAttributeCollection Attributes, bool IsMultiLine) : IXamlElement
{
}
