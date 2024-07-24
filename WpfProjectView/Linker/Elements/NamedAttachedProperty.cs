namespace WpfProjectView;

using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an attached property of a type.
/// </summary>
/// <param name="Name">The property name.</param>
/// <param name="FromMethodInfo">The <see cref="MethodInfo"/> object if coming from a <see cref="Type.GetMethod(string)"/> call.</param>
/// <param name="FromMethodSymbol">The <see cref="IMethodSymbol"/> object if coming from code parsing.</param>
public record NamedAttachedProperty(string Name, MethodInfo? FromMethodInfo, IMethodSymbol? FromMethodSymbol) : INamedProperty
{
}
