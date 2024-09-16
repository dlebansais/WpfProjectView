namespace WpfProjectView;

using System;
using System.Diagnostics;
using System.Xaml;
using Contracts;

/// <summary>
/// Implements a context for the xaml parser.
/// </summary>
internal record XamlParsingContext(XamlXmlReader Reader, XamlNamespaceCollection Namespaces) : IDisposable
{
    /// <summary>
    /// Gets the reader node type.
    /// </summary>
    public XamlNodeType NodeType { get => Reader.NodeType; }

    /// <summary>
    /// Gets the reader namespace path.
    /// </summary>
    public NamespaceDeclaration NamespacePath { get => Reader.Namespace; }

    /// <summary>
    /// Gets the reader member.
    /// </summary>
    public XamlMember Member { get => Reader.Member; }

    /// <summary>
    /// Gets the reader object namespace.
    /// Can only be queried when starting to parse an object.
    /// </summary>
    public IXamlNamespace ObjectNamespace
    {
        get
        {
            Contract.Assert(NodeType == XamlNodeType.StartObject);

            XamlType Object = Reader.Type;
            return FindNamespaceMatch(Object.PreferredXamlNamespace, Object.UnderlyingType);
        }
    }

    /// <summary>
    /// Gets the reader object name.
    /// Can only be queried when starting to parse an object.
    /// </summary>
    public string ObjectName
    {
        get
        {
            Contract.Assert(NodeType == XamlNodeType.StartObject);

            return Reader.Type.Name;
        }
    }

    /// <summary>
    /// Gets the reader member namespace.
    /// Can only be queried when starting to parse a member.
    /// </summary>
    public IXamlNamespace MemberNamespace
    {
        get
        {
            Contract.Assert(NodeType == XamlNodeType.StartMember);

            XamlMember Member = Reader.Member;

            Contract.Assert(Member.UnderlyingMember is null);

            return FindNamespaceMatch(Member.PreferredXamlNamespace, null);
        }
    }

    /// <summary>
    /// Gets the reader member name.
    /// Can only be queried when starting to parse a member.
    /// </summary>
    public string MemberName
    {
        get
        {
            Contract.Assert(NodeType == XamlNodeType.StartMember);

            return Reader.Member.Name;
        }
    }

    /// <summary>
    /// Gets the reader value.
    /// </summary>
    public object Value { get => Reader.Value; }

    /// <summary>
    /// Gets the reader line number.
    /// </summary>
    public int LineNumber { get => Reader.LineNumber; }

    /// <summary>
    /// Gets the reader current object name.
    /// </summary>
    public string? CurrentObjectName { get; init; }

    /// <summary>
    /// Reads once.
    /// </summary>
    public bool Read() => Reader.Read();

    /// <summary>
    /// Finds a match between a parsed namespace and a type.
    /// </summary>
    /// <param name="preferredXamlNamespace">The parsed namespace.</param>
    /// <param name="underlyingType">The type.</param>
    public IXamlNamespace FindNamespaceMatch(string preferredXamlNamespace, Type? underlyingType)
    {
        string? TypeNamespace = underlyingType?.Namespace;
        string? TypeAssembly = underlyingType?.Assembly.GetName().Name;

        IXamlNamespace? PreferredNamespace = null;

        foreach (IXamlNamespace Namespace in Namespaces)
            UpdatePreferredNamespace(Namespace, preferredXamlNamespace, TypeNamespace, TypeAssembly, ref PreferredNamespace);

        return Contract.AssertNotNull(PreferredNamespace);
    }

    private static void UpdatePreferredNamespace(IXamlNamespace candidateNamespace, string preferredXamlNamespace, string? typeNamespace, string? typeAssembly, ref IXamlNamespace? preferredNamespace)
    {
        if (candidateNamespace is XamlNamespaceDefault Default)
        {
            if (preferredXamlNamespace == XamlNamespaceDefault.DefaultPath)
                preferredNamespace = Default;
        }
        else if (candidateNamespace is XamlNamespaceExtension Extension)
        {
            if (preferredXamlNamespace == XamlNamespaceExtension.ExtensionPath)
                preferredNamespace = Extension;
        }
        else if (candidateNamespace is XamlNamespaceLocal Local)
        {
            if (preferredXamlNamespace == Local.AssemblyPath)
                preferredNamespace = Local;
        }
        else
        {
            XamlNamespace Other = (XamlNamespace)candidateNamespace;

            if (preferredXamlNamespace == Other.AssemblyPath)
                preferredNamespace = Other;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        ((IDisposable)Reader).Dispose();
    }
}
