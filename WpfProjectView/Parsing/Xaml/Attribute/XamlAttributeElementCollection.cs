namespace WpfProjectView;

/// <summary>
/// Implements a xaml element attribute.
/// </summary>
internal record XamlAttributeElementCollection(string Name, XamlElementCollection Children) : XamlAttribute(Name, Children)
{
}
