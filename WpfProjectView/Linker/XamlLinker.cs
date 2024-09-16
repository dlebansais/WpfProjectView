namespace WpfProjectView;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Markup;
using Contracts;
using Microsoft.CodeAnalysis;

/// <summary>
/// Implements a linker that resolve types in Xaml files.
/// </summary>
/// <param name="project">The project to link.</param>
public class XamlLinker(IProject project)
{
    /// <summary>
    /// Gets the project to link.
    /// </summary>
    public IProject Project { get; } = project;

    /// <summary>
    /// Gets the list of errors reported by <see cref="LinkAsync"/>.
    /// </summary>
    public IReadOnlyList<XamlLinkerError> Errors { get => InternalErrors.AsReadOnly(); }

    /// <summary>
    /// Links the XAML and C# sources.
    /// </summary>
    public async Task LinkAsync()
    {
        InternalErrors.Clear();

        List<SyntaxTree> ParsedSyntaxTrees = new();
        int UnparsedCount = 0;

        foreach (IFile Item in Project.Files)
            if (Item is IXamlCodeFile AsXamlCodeFile)
            {
                if (AsXamlCodeFile.SyntaxTree is SyntaxTree CodeBehindSyntaxTree)
                    ParsedSyntaxTrees.Add(CodeBehindSyntaxTree);
                else
                    UnparsedCount++;
            }
            else if (Item is ICodeFile AsCodeFile)
            {
                if (AsCodeFile.SyntaxTree is SyntaxTree OtherCodeSyntaxTree)
                    ParsedSyntaxTrees.Add(OtherCodeSyntaxTree);
                else
                    UnparsedCount++;
            }

        if (UnparsedCount > 0)
        {
            ReportError($"{UnparsedCount} file(s) found unparsed.");
            return;
        }

        await TypeManager.FillCodeTypes(ParsedSyntaxTrees, Project.PathToExternalDlls).ConfigureAwait(false);

        foreach (IFile Item in Project.Files)
        {
            IXamlElement? XamlRoot = null;

            if (Item is IXamlCodeFile XamlCodeFile)
            {
                IXamlParsingResult XamlParsingResult = Contract.AssertNotNull(XamlCodeFile.XamlParsingResult);
                IXamlElement NonResourceXamlRoot = Contract.AssertNotNull(XamlParsingResult.Root);

                LastXamlSourceFile = XamlCodeFile.XamlSourceFile;
                XamlRoot = NonResourceXamlRoot;
            }

            if (Item is IXamlResourceFile XamlResourceFile && XamlResourceFile.XamlParsingResult?.Root is IXamlElement ResourceXamlRoot)
            {
                LastXamlSourceFile = XamlResourceFile.SourceFile;
                XamlRoot = ResourceXamlRoot;
            }

            if (XamlRoot is not null)
            {
                Dictionary<string, IXamlNamespace> NamespaceTable = new();
                ParseElement(XamlRoot, NamespaceTable);
            }
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
            ReportError("Failed to parse type for element.", xamlElement);
    }

    private void ParseElementChildren(IXamlElement xamlElement, NamedType elementType, Dictionary<string, IXamlNamespace> namespaceTable)
    {
        foreach (IXamlElement Child in xamlElement.Children)
            ParseElement(Child, namespaceTable);
    }

    private bool TryGetElementType(IXamlElement xamlElement, Dictionary<string, IXamlNamespace> namespaceTable, out NamedType elementType)
    {
        if (TryGetFullTypeName(xamlElement.NameWithPrefix, namespaceTable, out string FullName))
        {
            foreach (IXamlAttribute Attribute in xamlElement.Attributes)
                if (Attribute is IXamlAttributeDirective AttributeDirective)
                {
                    if (AttributeDirective.Namespace is IXamlNamespaceExtension)
                    {
                        if (AttributeDirective.Name == "Class")
                        {
                            string? StringValue = Attribute.Value as string;
                            FullName = Contract.AssertNotNull(StringValue);
                            break;
                        }
                    }
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
        }

        Contract.Unused(out elementType);
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
            else if (XamlAttribute is IXamlAttributeSimpleValue AttributeSimpleValue)
            {
                if (HasDefaultAttributeName(elementType, out string ContentPropertyName))
                {
                    string Name = ContentPropertyName;

                    if (elementType.TryGetProperty(Name, out NamedProperty DefaultNamedProperty))
                    {
                        AddAttributeToTable(xamlElement, AttributeSimpleValue, DefaultNamedProperty, PropertyTable);
                    }
                    else
                        ReportError($"Failed to find default property with name '{Name}' and value '{AttributeToString(XamlAttribute)}'.", xamlElement);
                }
            }
            else
                ReportError($"Failed to parse attribute {AttributeToString(XamlAttribute)} for element.", xamlElement);
        }
    }

    private static string AttributeToString(IXamlAttribute attribute)
    {
        string? Result = null;

        if (attribute is IXamlAttributeDirective DirectiveAttribute)
            Result = $"{DirectiveAttribute.Namespace.Prefix}:{DirectiveAttribute.Name}";
        if (attribute is IXamlAttributeMember MemberAttribute)
            Result = MemberAttribute.Name;
        if (attribute is IXamlAttributeElementCollection ElementCollectionAttribute)
            Result = $"{ElementCollectionAttribute.Name} ({ElementCollectionAttribute.Children.Count} children)";
        if (attribute is IXamlAttributeSimpleValue SimpleValueAttribute)
        {
            object Value = Contract.AssertNotNull(SimpleValueAttribute.Value);
            Result = $"(no name, value type is {Value.GetType().Name})";
        }

        return Contract.AssertNotNull(Result);
    }

    private static void AddAttributeToTable<TKey>(IXamlElement xamlElement, IXamlAttribute xamlAttribute, TKey key, IDictionary<TKey, object> table)
    {
        object? Value = xamlAttribute.Value;

        Contract.Assert(!table.ContainsKey(key));
        table[key] = Contract.AssertNotNull(Value);
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

        Contract.Unused(out directive);
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

        Contract.Unused(out namedProperty);
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

        Contract.Unused(out namedAttachedProperty);
        return false;
    }

    private bool TryGetAttachedProperty(string fullPropertyName, Dictionary<string, IXamlNamespace> namespaceTable, out NamedAttachedProperty namedAttachedProperty)
    {
        string[] Splitted = fullPropertyName.Split('.');
        if (Splitted.Length == 2)
        {
            string TypeNameWithPrefix = Splitted[0].Trim();
            string PropertyName = Splitted[1].Trim();

            if (TryGetFullTypeName(TypeNameWithPrefix, namespaceTable, out string FullTypeName))
            {
                // Add the WPF if not already known.
                _ = TypeManager.TryFindWpfNamedType(FullTypeName, out _);

                // Now get the type.
                bool IsTypeFound = TypeManager.TryFindCodeType(FullTypeName, out NamedType ParsedNamedType);
                Contract.Assert(IsTypeFound);

                if (ParsedNamedType.TryFindAttachedProperty(PropertyName, out namedAttachedProperty))
                    return true;
            }
        }

        Contract.Unused(out namedAttachedProperty);
        return false;
    }

    private static bool TryGetDirectEvent(IXamlAttribute xamlAttribute, NamedType elementType, out NamedEvent namedEvent)
    {
        if (xamlAttribute is IXamlAttributeMember AttributeMember)
            if (elementType.TryGetEvent(AttributeMember.Name, out namedEvent))
                return true;

        Contract.Unused(out namedEvent);
        return false;
    }

    private static bool HasDefaultAttributeName(NamedType namedType, out string contentPropertyName)
    {
        contentPropertyName = string.Empty;

        bool Result = false;
        bool IsHandled = false;

        if (namedType.FromGetType is Type ElementType)
        {
            Result = HasDefaultAttributeName(ElementType, out contentPropertyName);
            IsHandled = true;
        }

        if (namedType.FromTypeSymbol is INamedTypeSymbol NamedTypeSymbol)
        {
            Result = HasDefaultAttributeName(NamedTypeSymbol, out contentPropertyName);
            IsHandled = true;
        }

        Contract.Assert(IsHandled);

        return Result;
    }

    private static bool HasDefaultAttributeName(Type elementType, out string contentPropertyName)
    {
        contentPropertyName = string.Empty;

        bool Result = false;
        object[] CustomAttributes = elementType.GetCustomAttributes(false);

        foreach (object Attribute in CustomAttributes)
            if (Attribute is ContentPropertyAttribute ContentPropertyAttribute)
            {
                contentPropertyName = ContentPropertyAttribute.Name;
                Result = true;
            }

        return Result;
    }

    private static bool HasDefaultAttributeName(INamedTypeSymbol namedTypeSymbol, out string contentPropertyName)
    {
        contentPropertyName = string.Empty;

        bool Result = false;
        var CustomAttributes = namedTypeSymbol.GetAttributes();

        foreach (AttributeData AttributeData in CustomAttributes)
        {
            INamedTypeSymbol AttributeClass = Contract.AssertNotNull(AttributeData.AttributeClass);

            if (AttributeClass.Name == nameof(ContentPropertyAttribute))
                Result = HasDefaultAttributeName(AttributeData, out contentPropertyName);
        }

        return Result;
    }

    private static bool HasDefaultAttributeName(AttributeData attributeData, out string contentPropertyName)
    {
        if (attributeData.ConstructorArguments.Length == 1)
        {
            TypedConstant Argument = attributeData.ConstructorArguments.First();
            contentPropertyName = $"{Argument.Value}";
            return true;
        }

        contentPropertyName = string.Empty;
        return false;
    }

    private static bool TryGetFullTypeName(string typeNameWithPrefix, Dictionary<string, IXamlNamespace> namespaceTable, out string fullTypeName)
    {
        string[] Splitted = typeNameWithPrefix.Split(':');

        if (Splitted.Length == 1)
        {
            string TypeName = Splitted[0];

            if (WpfTypeManager.TryFindType(TypeName, out string FullWpfTypeName))
            {
                fullTypeName = FullWpfTypeName;
                return true;
            }
        }
        else
        {
            string Prefix = Splitted[0];
            string TypeName = Splitted[1];

            Contract.Assert(namespaceTable.ContainsKey(Prefix));

            IXamlNamespace XamlNamespace = namespaceTable[Prefix];
            fullTypeName = XamlNamespace.Namespace + "." + TypeName;
            return true;
        }

        Contract.Unused(out fullTypeName);
        return false;
    }

    private void ReportError(string message, IXamlElement xamlElement)
    {
        ReportError(message + $" Name: {xamlElement.NameWithPrefix}, Line {xamlElement.LineNumber}, Position {xamlElement.LinePosition}.");
    }

    private void ReportError(string message)
    {
        if (LastXamlSourceFile is not null)
            message += $" (File: {string.Join("/", LastXamlSourceFile.Path.Ancestors)}/{LastXamlSourceFile.Name})";

        InternalErrors.Add(new XamlLinkerError(message));
    }

    private readonly NamedTypeManager TypeManager = new();
    private readonly List<XamlLinkerError> InternalErrors = new();
    private FolderView.IFile? LastXamlSourceFile;
}
