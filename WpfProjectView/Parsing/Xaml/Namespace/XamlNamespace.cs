namespace WpfProjectView;

using System;
using System.Diagnostics;

/// <summary>
/// Implements a xaml namespace in any assembly.
/// </summary>
[DebuggerDisplay("{Prefix,nq}={AssemblyPath,nq}")]
internal record XamlNamespace(string Prefix, string Namespace, string Assembly) : IXamlNamespace
{
    /// <summary>
    /// The namespace header.
    /// </summary>
    public const string ClrNamespaceHeader = "clr-namespace:";

    /// <summary>
    /// The assembly header.
    /// </summary>
    public const string AssemblyHeader = "assembly=";

    /// <summary>
    /// Creates a new instance of a namespace.
    /// </summary>
    /// <param name="prefix">The prefix.</param>
    /// <param name="namespacePath">The namespace path.</param>
    public static IXamlNamespace Create(string prefix, string namespacePath)
    {
        string[] Splitted = namespacePath.Split(';');
        string NamespacePart = Splitted[0];
        bool IsAssemblyEmpty = Splitted.Length < 2;
        string AssemblyPart = IsAssemblyEmpty ? string.Empty : Splitted[1];

        string Namespace;
        bool IsNamespaceValid;

        if (NamespacePart.StartsWith(ClrNamespaceHeader, StringComparison.Ordinal))
        {
            Namespace = NamespacePart.Substring(ClrNamespaceHeader.Length);
            IsNamespaceValid = true;
        }
        else
        {
            Namespace = string.Empty;
            IsNamespaceValid = false;
        }

        string Assembly;
        bool IsAssemblyValid;

        if (AssemblyPart.StartsWith(AssemblyHeader, StringComparison.Ordinal))
        {
            Assembly = AssemblyPart.Substring(AssemblyHeader.Length);
            IsAssemblyValid = true;
        }
        else
        {
            Assembly = string.Empty;
            IsAssemblyValid = false;
        }

        if (namespacePath == XamlNamespaceDefault.DefaultPath)
            return new XamlNamespaceDefault(prefix);

        if (namespacePath == XamlNamespaceExtension.ExtensionPath)
            return new XamlNamespaceExtension(prefix);

        Debug.Assert(IsNamespaceValid);

        if (IsAssemblyEmpty)
            return new XamlNamespaceLocal(prefix, Namespace);

        Debug.Assert(IsAssemblyValid);

        return new XamlNamespace(prefix, Namespace, Assembly);
    }

    /// <summary>
    /// Gets the assembly path.
    /// </summary>
    public string AssemblyPath => $"{ClrNamespaceHeader}{Namespace};{AssemblyHeader}{Assembly}";
}
