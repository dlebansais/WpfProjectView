namespace WpfProjectView;

using System;
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

            IXamlNamespace Result = null!;
            IXamlNamespace Preferred = null!;

            XamlType Object = Reader.Type;
            Type? UnderlyingType = Object.UnderlyingType;
            string? TypeNamespace = UnderlyingType?.Namespace;
            string? TypeAssembly = UnderlyingType?.Assembly.GetName().Name;

            if (TypeAssembly == "System.Private.CoreLib")
                TypeAssembly = "mscorlib";

            foreach (IXamlNamespace Namespace in Namespaces)
            {
                switch (Namespace)
                {
                    case XamlNamespaceDefault Default:
                        if (Object.PreferredXamlNamespace == XamlNamespaceDefault.DefaultPath)
                            Preferred = Default;
                        break;
                    case XamlNamespaceExtension Extension:
                        if (Object.PreferredXamlNamespace == XamlNamespaceExtension.ExtensionPath)
                            Preferred = Extension;
                        break;
                    case XamlNamespaceLocal Local:
                        if (Object.PreferredXamlNamespace == Local.AssemblyPath)
                            Preferred = Local;
                        if (Local.Namespace == TypeNamespace)
                            Result = Namespace;
                        break;
                    case XamlNamespace Other:
                        if (Object.PreferredXamlNamespace == Other.AssemblyPath)
                            Preferred = Other;
                        if (Other.Namespace == TypeNamespace && Other.Assembly == TypeAssembly)
                            Result = Namespace;
                        break;
                }

                if (Result is not null)
                    break;
            }

            if (Result is null)
                Result = Preferred;

            Debug.Assert(Result is not null);

            return Result;
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

            IXamlNamespace Result = null!;
            IXamlNamespace Preferred = null!;

            XamlMember Member = Reader.Member;
            Type? UnderlyingType = Member.UnderlyingMember?.DeclaringType;
            string? TypeNamespace = UnderlyingType?.Namespace;
            string? TypeAssembly = UnderlyingType?.Name;

            foreach (IXamlNamespace Namespace in Namespaces)
            {
                switch (Namespace)
                {
                    case XamlNamespaceDefault Default:
                        if (Member.PreferredXamlNamespace == XamlNamespaceDefault.DefaultPath)
                            Preferred = Default;
                        break;
                    case XamlNamespaceExtension Extension:
                        if (Member.PreferredXamlNamespace == XamlNamespaceExtension.ExtensionPath)
                            Preferred = Extension;
                        break;
                    case XamlNamespaceLocal Local:
                        if (Local.Namespace == TypeNamespace)
                            Result = Namespace;
                        break;
                    case XamlNamespace Other:
                        if (Other.Namespace == TypeNamespace && Other.Assembly == TypeAssembly)
                            Result = Namespace;
                        break;
                }

                if (Result is not null)
                    break;
            }

            if (Result is null)
                Result = Preferred;

            Debug.Assert(Result is not null);

            return Result;
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
    /// Reads once.
    /// </summary>
    public bool Read() => Reader.Read();
}
