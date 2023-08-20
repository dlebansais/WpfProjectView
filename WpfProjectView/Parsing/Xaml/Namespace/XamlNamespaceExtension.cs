﻿namespace WpfProjectView;

/// <summary>
/// Implements the extension xaml namespace.
/// </summary>
/// <inheritdoc/>
internal record XamlNamespaceExtension(string Prefix) : IXamlNamespace
{
    /// <summary>
    /// Gets the extension path.
    /// </summary>
    public const string ExtensionPath = "http://schemas.microsoft.com/winfx/2006/xaml";

    /// <summary>
    /// Gets the assembly path.
    /// </summary>
    public string AssemblyPath => ExtensionPath;
}
