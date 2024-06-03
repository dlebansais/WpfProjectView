namespace WpfProjectView;

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Gets the reader namespace path.
    /// </summary>
    public NamespaceDeclaration NamespacePath { get => Reader.Namespace; }

    /// <summary>
    /// Gets the reader member.
    /// </summary>
    public XamlMember Member { get => Reader.Member; }

    /// <summary>
    /// Gets the reader object namespace.
    /// </summary>
    public IXamlNamespace ObjectNamespace
    {
        get
        {
            Debug.Assert(NodeType == XamlNodeType.StartObject);

            XamlType Object = Reader.Type;
            return FindNamespaceMatch(Object.PreferredXamlNamespace, Object.UnderlyingType);
        }
    }

    /// <summary>
    /// Gets the reader object name.
    /// </summary>
    public string ObjectName
    {
        get
        {
            Debug.Assert(NodeType == XamlNodeType.StartObject);

            return Reader.Type.Name;
        }
    }

    /// <summary>
    /// Gets the reader member namespace.
    /// </summary>
    public IXamlNamespace MemberNamespace
    {
        get
        {
            Debug.Assert(NodeType == XamlNodeType.StartMember);

            XamlMember Member = Reader.Member;

            Debug.Assert(Member.UnderlyingMember is null);

            return FindNamespaceMatch(Member.PreferredXamlNamespace, null);
        }
    }

    /// <summary>
    /// Gets the reader member name.
    /// </summary>
    public string MemberName
    {
        get
        {
            Debug.Assert(NodeType == XamlNodeType.StartMember);

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

        if (TypeAssembly == "System.Private.CoreLib")
            TypeAssembly = "mscorlib";

        IXamlNamespace Result = null!;
        IXamlNamespace Preferred = null!;

        foreach (IXamlNamespace Namespace in Namespaces)
            if (IsNamespaceFound(Namespace, preferredXamlNamespace, TypeNamespace, TypeAssembly, ref Preferred, out Result))
                break;

        if (Result is null)
            Result = Preferred;

        Debug.Assert(Result is not null);

        return Result!;
    }

    private static bool IsNamespaceFound(IXamlNamespace candidateNamespace, string preferredXamlNamespace, string? typeNamespace, string? typeAssembly, ref IXamlNamespace preferredNamespace, out IXamlNamespace foundNamespace)
    {
        if (candidateNamespace is XamlNamespaceDefault Default)
        {
            if (preferredXamlNamespace == XamlNamespaceDefault.DefaultPath)
                preferredNamespace = Default;

            foundNamespace = null!;
            return false;
        }
        else if (candidateNamespace is XamlNamespaceExtension Extension)
        {
            if (preferredXamlNamespace == XamlNamespaceExtension.ExtensionPath)
                preferredNamespace = Extension;

            foundNamespace = null!;
            return false;
        }
        else if (candidateNamespace is XamlNamespaceLocal Local)
        {
            if (preferredXamlNamespace == Local.AssemblyPath)
                preferredNamespace = Local;
            /*if (Local.Namespace == typeNamespace) // Couldn't find a test case.
                foundNamespace = candidateNamespace;*/

            foundNamespace = null!;
            return false;
        }

        XamlNamespace Other = (XamlNamespace)candidateNamespace;

        if (preferredXamlNamespace == Other.AssemblyPath)
            preferredNamespace = Other;
        if (Other.Namespace == typeNamespace && Other.Assembly == typeAssembly)
        {
            foundNamespace = candidateNamespace;
            return true;
        }
        else
        {
            foundNamespace = null!;
            return false;
        }
    }
}
