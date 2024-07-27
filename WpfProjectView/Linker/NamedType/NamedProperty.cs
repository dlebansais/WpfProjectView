namespace WpfProjectView;

using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a property of a type.
/// </summary>
/// <param name="Name">The property name.</param>
/// <param name="FromPropertyInfo">The <see cref="PropertyInfo"/> object if coming from a <see cref="Type.GetProperty(string)"/> call.</param>
/// <param name="FromPropertySymbol">The <see cref="IPropertySymbol"/> object if coming from code parsing.</param>
public record NamedProperty(string Name, PropertyInfo? FromPropertyInfo, IPropertySymbol? FromPropertySymbol) : INamedProperty
{
}
