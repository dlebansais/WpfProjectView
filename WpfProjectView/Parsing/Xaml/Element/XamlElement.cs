namespace WpfProjectView;

using System.Diagnostics;

/// <summary>
/// Implements a xaml element.
/// </summary>
/// <inheritdoc/>
[DebuggerDisplay("{NameWithPrefix,nq}")]
internal record XamlElement(IXamlNamespaceCollection Namespaces, IXamlNamespace Namespace, string Name, IXamlElementCollection Children, IXamlAttributeCollection Attributes, bool IsMultiLine) : IXamlElement
{
    /// <summary>
    /// Gets the name with the namespace prefix.
    /// </summary>
    public string NameWithPrefix => Namespace is XamlNamespaceDefault ? Name : $"{Namespace.Prefix}:{Name}";
}
