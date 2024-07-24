namespace WpfProjectView;

using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a type describing the property of a type.
/// </summary>
public interface INamedProperty
{
    /// <summary>
    /// Gets the property name.
    /// </summary>
    string Name { get; }
}
