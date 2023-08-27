namespace WpfProjectView;

using System.Diagnostics;

/// <summary>
/// Implements the default xaml namespace.
/// </summary>
/// <inheritdoc/>
[DebuggerDisplay("(default)")]
internal record XamlNamespaceDefault(string Prefix) : IXamlNamespace
{
    /// <summary>
    /// Gets the default path.
    /// </summary>
    public const string DefaultPath = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

    /// <summary>
    /// Gets the assembly path.
    /// </summary>
    public string AssemblyPath => DefaultPath;
}
