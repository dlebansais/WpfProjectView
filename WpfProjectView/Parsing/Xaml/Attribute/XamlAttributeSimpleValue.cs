﻿namespace WpfProjectView;

using System.Diagnostics;

/// <summary>
/// Implements a xaml element attribute.
/// </summary>
[DebuggerDisplay("{StringValue,nq}")]
internal record XamlAttributeSimpleValue(string StringValue) : IXamlAttributeSimpleValue
{
    /// <summary>
    /// Gets the attribute value.
    /// </summary>
    public object? Value { get => StringValue; }
}
