namespace WpfProjectView;

using System.Xaml;

/// <summary>
/// Implements a context for the xaml parser.
/// </summary>
internal record XamlParsingContext(XamlXmlReader Reader, XamlNamespaceCollection Namespaces)
{
    /// <summary>
    /// Gets the reader node type.
    /// </summary>
    public XamlNodeType NodeType { get => Reader.NodeType; }

    /// <summary>
    /// Gets the reader namespace.
    /// </summary>
    public NamespaceDeclaration Namespace { get => Reader.Namespace; }

    /// <summary>
    /// Gets the reader member.
    /// </summary>
    public XamlMember Member { get => Reader.Member; }

    /// <summary>
    /// Gets the reader object type.
    /// </summary>
    public XamlType Type { get => Reader.Type; }

    /// <summary>
    /// Gets the reader value.
    /// </summary>
    public object Value { get => Reader.Value; }

    /// <summary>
    /// Reads once.
    /// </summary>
    public bool Read() => Reader.Read();
}
