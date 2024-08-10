namespace WpfProjectView;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a non-generic type with a name.
/// </summary>
/// <param name="FullName">The full type name.</param>
/// <param name="FromGetType">The <see cref="Type"/> object if coming from a <see cref="object.GetType"/> call.</param>
/// <param name="FromTypeSymbol">The <see cref="INamedTypeSymbol"/> object if coming from code parsing.</param>
public record NamedType(string FullName, Type? FromGetType, INamedTypeSymbol? FromTypeSymbol)
{
    /// <summary>
    /// Tries to get a property of this type by its name.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="namedProperty">The property upon return.</param>
    public bool TryGetProperty(string name, out NamedProperty namedProperty)
    {
        if (FromGetType is not null)
        {
            Console.WriteLine($"property {name} from {FromGetType.Name}");

            if (FromGetType.GetProperty(name) is PropertyInfo PropertyInfo)
            {
                Console.WriteLine(name);

                namedProperty = new NamedProperty(name, PropertyInfo, null);
                return true;
            }
        }
        else if (FromTypeSymbol is not null)
        {
            Console.WriteLine($"property {name} from {FromTypeSymbol.Name}");

            if (GetAllMemberSymbols().OfType<IPropertySymbol>().FirstOrDefault(symbol => symbol.Name == name) is IPropertySymbol PropertySymbol)
            {
                Console.WriteLine(name);

                namedProperty = new NamedProperty(name, null, PropertySymbol);
                return true;
            }
        }
        else
            Console.WriteLine($"property {name} from no type");

        namedProperty = null!;
        return false;
    }

    /// <summary>
    /// Tries to get an attached property of this type by its name.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="namedAttachedProperty">The property upon return.</param>
    public bool TryFindAttachedProperty(string name, out NamedAttachedProperty namedAttachedProperty)
    {
        string Setter = $"Set{name}";

        if (FromGetType is not null)
        {
            if (FromGetType.GetMethod(Setter) is MethodInfo MethodInfo)
            {
                namedAttachedProperty = new NamedAttachedProperty(name, MethodInfo, null);
                return true;
            }
        }
        else if (FromTypeSymbol is not null)
        {
            if (GetAllMemberSymbols().OfType<IMethodSymbol>().FirstOrDefault(symbol => symbol.Name == Setter) is IMethodSymbol MethodSymbol)
            {
                namedAttachedProperty = new NamedAttachedProperty(name, null, MethodSymbol);
                return true;
            }
        }

        namedAttachedProperty = null!;
        return false;
    }

    /// <summary>
    /// Tries to get an event of this type by its name.
    /// </summary>
    /// <param name="name">The event name.</param>
    /// <param name="namedEvent">The event upon return.</param>
    public bool TryGetEvent(string name, out NamedEvent namedEvent)
    {
        if (FromGetType is not null)
        {
            if (FromGetType.GetEvent(name) is EventInfo EventInfo)
            {
                namedEvent = new NamedEvent(name, EventInfo, null);
                return true;
            }
        }
        else if (FromTypeSymbol is not null)
        {
            if (GetAllMemberSymbols().OfType<IEventSymbol>().FirstOrDefault(symbol => symbol.Name == name) is IEventSymbol EventSymbol)
            {
                namedEvent = new NamedEvent(name, null, EventSymbol);
                return true;
            }
        }

        namedEvent = null!;
        return false;
    }

    private IEnumerable<ISymbol> GetAllMemberSymbols()
    {
        AllMemberSymbols = AllMemberSymbols ?? FromTypeSymbol?.GetBaseTypesAndThis()?.SelectMany(n => n.GetMembers());

        Debug.Assert(AllMemberSymbols is not null);
        return AllMemberSymbols!;
    }

    private IEnumerable<ISymbol>? AllMemberSymbols;
}
