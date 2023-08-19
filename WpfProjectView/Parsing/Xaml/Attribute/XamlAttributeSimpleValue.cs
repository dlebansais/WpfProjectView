namespace WpfProjectView;

/// <summary>
/// Implements a xaml element attribute.
/// </summary>
internal record XamlAttributeSimpleValue(string StringValue) : XamlAttribute(string.Empty, StringValue)
{
}
