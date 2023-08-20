namespace WpfProjectView;

using System.Collections.Generic;
using System.Text;

/// <summary>
/// Implements a context for the xaml printer.
/// </summary>
internal record XamlPrintingContext(StringBuilder Builder, IXamlElement Element, int Indentation)
{
    /// <summary>
    /// Gets strings corresponding to attribute directives.
    /// </summary>
    public List<string> AttributeDirectiveList { get; } = new();

    /// <summary>
    /// Gets strings corresponding to namespaces.
    /// </summary>
    public List<string> NamespaceList { get; } = new();

    /// <summary>
    /// Gets strings, or element collections that are on one line, corresponding to attribute members.
    /// </summary>
    public List<object> AttributeMemberList { get; } = new();

    /// <summary>
    /// Gets the string corresponding to the value attribute.
    /// </summary>
    public List<string> ValueString { get; } = new();

    /// <summary>
    /// Gets strings corresponding to object attributes on multiple lines.
    /// </summary>
    public List<XamlAttributeElementCollection> MultiLineElementCollectionAttributeList { get; } = new();

    /// <summary>
    /// Gets a value indicating whether the entire element can be printed on one line.
    /// </summary>
    public bool IsFullSingleLine => !Element.IsMultiLine && Element.Children.Count == 0 && MultiLineElementCollectionAttributeList.Count == 0 && (NamespaceList.Count == 0 || (NamespaceList.Count == 1 && AttributeDirectiveList.Count == 0 && AttributeMemberList.Count == 0) || ValueString.Count > 0);

    /// <summary>
    /// Gets a value indicating whether the element can be printed on multiple line without children.
    /// </summary>
    public bool IsMultipleLineNoChildren => Element.Children.Count == 0 && MultiLineElementCollectionAttributeList.Count == 0;

    /// <summary>
    /// Gets a value indicating whether the element can be printed on one line for attributes, followed by lines for children.
    /// </summary>
    public bool IsSingleLineWithChildren => NamespaceList.Count == 0 && !Element.IsMultiLine;
}
