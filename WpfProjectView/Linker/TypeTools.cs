namespace WpfProjectView;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

/// <summary>
/// Provide tools to manipulate types.
/// </summary>
public static class TypeTools
{
    /// <summary>
    /// Gets the recursive list of base types.
    /// </summary>
    /// <param name="type">The type.</param>
    public static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol type)
    {
        var current = type;
        while (current is not null)
        {
            yield return current;
            current = current.BaseType;
        }
    }
}
