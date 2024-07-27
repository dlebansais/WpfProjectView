namespace WpfProjectView;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Markup;
using Microsoft.CodeAnalysis;

/// <summary>
/// Implements a linker that resolve types in Xaml files.
/// </summary>
internal class XamlLinker
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XamlLinker"/> class.
    /// </summary>
    /// <param name="project">The project to link.</param>
    public XamlLinker(Project project)
    {
        Project = project;
    }

    /// <summary>
    /// Gets the project to link.
    /// </summary>
    public Project Project { get; }

    /// <summary>
    /// Links the XAML and C# sources.
    /// </summary>
    public async Task LinkAsync()
    {
        await TypeManager.FillCodeTypes(Project.Files, Project.PathToExternalDlls).ConfigureAwait(false);

        foreach (IFile Item in Project.Files)
            if (Item is IXamlCodeFile XamlCodeFile && XamlCodeFile.XamlParsingResult?.Root is IXamlElement XamlRoot)
            {
                Dictionary<string, IXamlNamespace> NamespaceTable = new();
                ParseElement(XamlRoot, NamespaceTable);
            }
    }

    private void ParseElement(IXamlElement xamlElement, Dictionary<string, IXamlNamespace> namespaceTable)
    {
        foreach (IXamlNamespace XamlNamespace in xamlElement.Namespaces)
            namespaceTable.Add(XamlNamespace.Prefix, XamlNamespace);

        if (TryGetElementType(xamlElement, namespaceTable, out NamedType ElementType))
        {
            ParseElementAttributes(xamlElement, ElementType, namespaceTable);
            ParseElementChildren(xamlElement, ElementType, namespaceTable);
        }
        else
            throw new NotSupportedException();
    }

    private void ParseElementChildren(IXamlElement xamlElement, NamedType elementType, Dictionary<string, IXamlNamespace> namespaceTable)
    {
        foreach (IXamlElement Child in xamlElement.Children)
            ParseElement(Child, namespaceTable);
    }

    private bool TryGetElementType(IXamlElement xamlElement, Dictionary<string, IXamlNamespace> namespaceTable, out NamedType elementType)
    {
        string FullName = GetFullTypeName(xamlElement.NameWithPrefix, namespaceTable);

        foreach (IXamlAttribute Attribute in xamlElement.Attributes)
            if (Attribute is IXamlAttributeDirective AttributeDirective && AttributeDirective.Namespace is IXamlNamespaceExtension && AttributeDirective.Name == "Class")
                if (Attribute.Value is string StringValue)
                {
                    FullName = StringValue;
                    break;
                }

        if (TypeManager.TryFindWpfNamedType(FullName, out NamedType WpfNamedType))
        {
            elementType = WpfNamedType;
            return true;
        }
        else if (TypeManager.TryFindCodeType(FullName, out NamedType LocalNamedType))
        {
            elementType = LocalNamedType;
            return true;
        }

        elementType = null!;
        return false;
    }

    private void ParseElementAttributes(IXamlElement xamlElement, NamedType elementType, Dictionary<string, IXamlNamespace> namespaceTable)
    {
        Dictionary<Directive, object> DirectiveTable = new();
        Dictionary<NamedProperty, object> PropertyTable = new();
        Dictionary<NamedAttachedProperty, object> AttachedPropertyTable = new();
        Dictionary<NamedEvent, object> EventTable = new();

        foreach (IXamlAttribute XamlAttribute in xamlElement.Attributes)
        {
            if (TryGetDirective(XamlAttribute, out Directive Directive))
            {
                AddAttributeToTable(xamlElement, XamlAttribute, Directive, DirectiveTable);
            }
            else if (TryGetDirectProperty(XamlAttribute, elementType, out NamedProperty NamedProperty))
            {
                AddAttributeToTable(xamlElement, XamlAttribute, NamedProperty, PropertyTable);
            }
            else if (TryGetDirectEvent(XamlAttribute, elementType, out NamedEvent NamedEvent))
            {
                AddAttributeToTable(xamlElement, XamlAttribute, NamedEvent, EventTable);
            }
            else if (TryGetAttachedProperty(XamlAttribute, namespaceTable, out NamedAttachedProperty NamedAttachedProperty))
            {
                AddAttributeToTable(xamlElement, XamlAttribute, NamedAttachedProperty, AttachedPropertyTable);
            }
            else if (HasDefaultAttributeName(elementType, out string ContentPropertyName))
            {
                string Name = ContentPropertyName;

                if (elementType.TryGetProperty(Name, out NamedProperty DefaultNamedProperty))
                {
                    AddAttributeToTable(xamlElement, XamlAttribute, DefaultNamedProperty, PropertyTable);
                }
                else
                    throw new NotSupportedException();
            }
            else
                throw new NotSupportedException();
        }
    }

    private static void AddAttributeToTable<TKey>(IXamlElement xamlElement, IXamlAttribute xamlAttribute, TKey key, IDictionary<TKey, object> table)
    {
        if (xamlAttribute.Value is not object Value)
            throw new NotSupportedException();

        if (table.ContainsKey(key))
            throw new NotSupportedException($"Duplicate attribute {key} within declaration of {xamlElement}.");

        table[key] = Value;
    }

    private static bool TryGetAttributeName(IXamlAttribute xamlAttribute, out string name)
    {
        if (xamlAttribute is IXamlAttributeMember AttributeMember)
        {
            name = AttributeMember.Name;
            return true;
        }
        else if (xamlAttribute is IXamlAttributeDirective AttributeDirective)
        {
            name = $"{AttributeDirective.Namespace.Prefix}:{AttributeDirective.Name}";
            return true;
        }
        else if (xamlAttribute is IXamlAttributeElementCollection AttributeElementCollection)
        {
            name = AttributeElementCollection.Name;
            return true;
        }
        else
        {
            name = string.Empty;
            return false;
        }
    }

    private static bool TryGetDirective(IXamlAttribute xamlAttribute, out Directive directive)
    {
        if (xamlAttribute is IXamlAttributeDirective AttributeDirective)
        {
            if (AttributeDirective.Namespace is IXamlNamespaceExtension)
            {
                if (Directive.TryParse(AttributeDirective.Name, out Directive MatchDirective))
                {
                    directive = MatchDirective;
                    return true;
                }
            }
        }

        directive = null!;
        return false;
    }

    private static bool TryGetDirectProperty(IXamlAttribute xamlAttribute, NamedType elementType, out NamedProperty namedProperty)
    {
        if (xamlAttribute is IXamlAttributeMember AttributeMember)
        {
            if (elementType.TryGetProperty(AttributeMember.Name, out namedProperty))
            {
                return true;
            }
        }
        else if (xamlAttribute is IXamlAttributeElementCollection AttributeElementCollection)
        {
            if (elementType.TryGetProperty(AttributeElementCollection.Name, out namedProperty))
            {
                return true;
            }
        }

        namedProperty = null!;
        return false;
    }

    private bool TryGetAttachedProperty(IXamlAttribute xamlAttribute, Dictionary<string, IXamlNamespace> namespaceTable, out NamedAttachedProperty namedAttachedProperty)
    {
        if (xamlAttribute is IXamlAttributeMember AttributeMember)
        {
            return TryGetAttachedProperty(AttributeMember.Name, namespaceTable, out namedAttachedProperty);
        }
        else if (xamlAttribute is IXamlAttributeElementCollection AttributeElementCollection)
        {
            return TryGetAttachedProperty(AttributeElementCollection.Name, namespaceTable, out namedAttachedProperty);
        }

        namedAttachedProperty = null!;
        return false;
    }

    private bool TryGetAttachedProperty(string fullPropertyName, Dictionary<string, IXamlNamespace> namespaceTable, out NamedAttachedProperty namedAttachedProperty)
    {
        string[] Splitted = fullPropertyName.Split('.');
        if (Splitted.Length == 2)
        {
            string TypeNameWithPrefix = Splitted[0].Trim();
            string PropertyName = Splitted[1].Trim();
            string FullTypeName = GetFullTypeName(TypeNameWithPrefix, namespaceTable);

            if (TypeManager.TryFindCodeType(FullTypeName, out NamedType NamedType))
                if (NamedType.TryFindAttachedProperty(PropertyName, out namedAttachedProperty))
                    return true;
        }

        namedAttachedProperty = null!;
        return false;
    }

    private static bool TryGetDirectEvent(IXamlAttribute xamlAttribute, NamedType elementType, out NamedEvent namedEvent)
    {
        if (xamlAttribute is IXamlAttributeMember AttributeMember)
        {
            if (elementType.TryGetEvent(AttributeMember.Name, out namedEvent))
            {
                return true;
            }
        }

        namedEvent = null!;
        return false;
    }

    private static bool HasDefaultAttributeName(NamedType namedType, out string contentPropertyName)
    {
        if (namedType.FromGetType is Type ElementType)
            return HasDefaultAttributeName(ElementType, out contentPropertyName);
        else if (namedType.FromTypeSymbol is INamedTypeSymbol NamedTypeSymbol)
            return HasDefaultAttributeName(NamedTypeSymbol, out contentPropertyName);

        contentPropertyName = string.Empty;
        return false;
    }

    private static bool HasDefaultAttributeName(Type elementType, out string contentPropertyName)
    {
        object[] CustomAttributes = elementType.GetCustomAttributes(false);

        foreach (object Attribute in CustomAttributes)
            if (Attribute is ContentPropertyAttribute ContentPropertyAttribute)
            {
                contentPropertyName = ContentPropertyAttribute.Name;
                return true;
            }

        contentPropertyName = string.Empty;
        return false;
    }

    private static bool HasDefaultAttributeName(INamedTypeSymbol namedTypeSymbol, out string contentPropertyName)
    {
        var CustomAttributes = namedTypeSymbol.GetAttributes();

        foreach (AttributeData AttributeData in CustomAttributes)
            if (AttributeData.AttributeClass?.Name == nameof(ContentPropertyAttribute))
                return HasDefaultAttributeName(AttributeData, out contentPropertyName);

        contentPropertyName = string.Empty;
        return false;
    }

    private static bool HasDefaultAttributeName(AttributeData attributeData, out string contentPropertyName)
    {
        if (attributeData.ConstructorArguments.Length == 1)
        {
            TypedConstant Argument = attributeData.ConstructorArguments.First();
            if (Argument.Value is string StringArgument)
            {
                contentPropertyName = StringArgument;
                return true;
            }
        }

        contentPropertyName = string.Empty;
        return false;
    }

    /// <summary>
    /// Tries to get the value of an attribute as a string.
    /// </summary>
    /// <param name="xamlAttribute">The attribute.</param>
    /// <param name="stringValue">The string value upon return.</param>
    private static bool TryGetAttributeValueAsString(IXamlAttribute xamlAttribute, out string stringValue)
    {
        if (xamlAttribute?.Value is string Value)
        {
            stringValue = Value;
            return true;
        }

        IXamlElement Child;

        if (xamlAttribute?.Value is IXamlElement AsElement)
        {
            Child = AsElement;
        }
        else if (xamlAttribute?.Value is IList<IXamlElement> AsElementList)
        {
            if (AsElementList.Count != 1)
            {
                stringValue = string.Empty;
                return false;
            }

            Child = AsElementList[0];
        }
        else
            throw new NotSupportedException($"Attribute value of type {xamlAttribute?.Value?.GetType()} is not supported.");

        if (Child.Children.Count != 0)
        {
            stringValue = string.Empty;
            return false;
        }

        string ChildName = Child.NameWithPrefix;

        if (ChildName.EndsWith("Extension", StringComparison.Ordinal))
            ChildName = ChildName.Substring(0, ChildName.Length - 9);

        string AttributeList = string.Empty;

        foreach (IXamlAttribute ChildAttribute in Child.Attributes)
        {
            if (AttributeList.Length > 0)
                AttributeList += ", ";

            if (TryGetAttributeName(ChildAttribute, out string ChildAttributeName) && ChildAttributeName.Length > 0)
                AttributeList += $"{ChildAttributeName}=";

            if (!TryGetAttributeValueAsString(ChildAttribute, out string ChildAttributeValue))
            {
                stringValue = string.Empty;
                return false;
            }

            AttributeList += ChildAttributeValue;
        }

        stringValue = $"{{{ChildName} {AttributeList}}}";
        return true;
    }

    private static bool IsValidTypeSymbol(INamedTypeSymbol typeSymbol)
    {
        var Members = typeSymbol.GetMembers();
        return Members.Any();
    }

    private static string GetFullTypeName(string typeNameWithPrefix, Dictionary<string, IXamlNamespace> namespaceTable)
    {
        string[] Splitted = typeNameWithPrefix.Split(':');

        if (Splitted.Length == 1)
        {
            string TypeName = Splitted[0];

            if (WpfTypeManager.TryFindType(TypeName, out string FullTypeName))
                return FullTypeName;
        }
        else
        {
            string Prefix = Splitted[0];
            string TypeName = Splitted[1];

            if (namespaceTable.TryGetValue(Prefix, out IXamlNamespace? XamlNamespace))
            {
                string FullTypeName = XamlNamespace.Namespace + "." + TypeName;
                return FullTypeName;
            }
        }

        throw new NotSupportedException();
    }

    private readonly NamedTypeManager TypeManager = new();
}
