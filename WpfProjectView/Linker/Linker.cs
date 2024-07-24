namespace WpfProjectView;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Implements a C# code parser.
/// </summary>
internal class Linker
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Linker"/> class.
    /// </summary>
    /// <param name="project">The project to link.</param>
    public Linker(Project project)
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
        await GetAllCodeTypes().ConfigureAwait(false);

        foreach (IFile Item in Project.Files)
            if (Item is IXamlCodeFile XamlCodeFile && XamlCodeFile.XamlParsingResult?.Root is IXamlElement XamlRoot)
            {
                Dictionary<string, IXamlNamespace> NamespaceTable = new();
                ParseElement(XamlRoot, NamespaceTable);
            }
    }

    private async Task GetAllCodeTypes()
    {
        List<SyntaxTree> ParsedSyntaxTrees = new();

        foreach (IFile Item in Project.Files)
        {
            if (Item is IXamlCodeFile AsXamlCodeFile && AsXamlCodeFile.SyntaxTree is SyntaxTree CodeBehindSyntaxTree)
                ParsedSyntaxTrees.Add(CodeBehindSyntaxTree);
            else if (Item is ICodeFile AsCodeFile && AsCodeFile.SyntaxTree is SyntaxTree OtherCodeSyntaxTree)
                ParsedSyntaxTrees.Add(OtherCodeSyntaxTree);
        }

        Dictionary<SyntaxTree, IEnumerable<SyntaxNode>> SyntaxTreeTable = new();
        foreach (SyntaxTree Item in ParsedSyntaxTrees)
        {
            SyntaxNode TreeRoot = await Item.GetRootAsync().ConfigureAwait(false);
            IEnumerable<SyntaxNode> Nodes = TreeRoot.DescendantNodes(node => true);
            SyntaxTreeTable.Add(TreeRoot.SyntaxTree, Nodes);
        }

        const string RuntimePath = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\{0}.dll";

        List<MetadataReference> DefaultReferences = new()
        {
            MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "mscorlib")),
            MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "System")),
            MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "System.Core")),
            MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "System.Xaml")),
            MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "PresentationCore")),
            MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "PresentationFramework")),
        };

        foreach (string PathToExternalDll in Project.PathToExternalDlls)
        {
            MetadataReference Reference = MetadataReference.CreateFromFile(PathToExternalDll);
            DefaultReferences.Add(Reference);
        }

        CSharpCompilation Compilation = CSharpCompilation.Create(assemblyName: "LocalAssembly", syntaxTrees: SyntaxTreeTable.Keys.ToArray(), references: DefaultReferences.ToArray(), options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        foreach (KeyValuePair<SyntaxTree, IEnumerable<SyntaxNode>> Entry in SyntaxTreeTable)
        {
            SemanticModel SemanticModel = Compilation.GetSemanticModel(Entry.Key);
            SyntaxNode[] SyntaxNodes = Entry.Value.ToArray();

            // IdentifierNameSyntax:
            //  - var keyword
            //  - identifiers of any kind (including type names)
            INamedTypeSymbol[] NamedTypes = SyntaxNodes.OfType<IdentifierNameSyntax>()
                                                       .Select(id => SemanticModel.GetSymbolInfo(id).Symbol)
                                                       .OfType<INamedTypeSymbol>()
                                                       .ToArray();
            AddToCodeTypes(NamedTypes);

            // BaseTypeDeclarationSyntax:
            //  - declared types (class...)
            INamedTypeSymbol[] DeclaredTypes = SyntaxNodes.OfType<BaseTypeDeclarationSyntax>()
                                                          .Select(id => SemanticModel.GetDeclaredSymbol(id))
                                                          .OfType<INamedTypeSymbol>()
                                                          .ToArray();
            AddToCodeTypes(DeclaredTypes);

            // ExpressionSyntax:
            //  - method calls
            //  - property uses
            //  - field uses
            //  - all kinds of composite expressions
            INamedTypeSymbol[] ExpressionTypes = SyntaxNodes.OfType<ExpressionSyntax>()
                                                            .Select(ma => SemanticModel.GetTypeInfo(ma).Type)
                                                            .OfType<INamedTypeSymbol>()
                                                            .ToArray();
            AddToCodeTypes(ExpressionTypes);
        }

        foreach (string PathToExternalDll in Project.PathToExternalDlls)
        {
            Assembly LoadedAssembly = Assembly.LoadFile(PathToExternalDll);
            Type[] ExportedTypes = LoadedAssembly.GetExportedTypes();
            AddToCodeTypes(ExportedTypes);
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

        if (TryFindWpfNamedType(FullName, out NamedType WpfNamedType))
        {
            elementType = WpfNamedType;
            return true;
        }
        else if (TryFindCodeType(FullName, out NamedType LocalNamedType))
        {
            elementType = LocalNamedType;
            return true;
        }

        elementType = null!;
        return false;
    }

    private bool TryFindWpfNamedType(string name, out NamedType wpfNamedType)
    {
        List<Assembly> WpfAssemblies = new() { typeof(Application).Assembly };

        Type? WpfType = WpfAssemblies.Where(a => !a.IsDynamic)
                                     .SelectMany(a => a.GetTypes())
                                     .FirstOrDefault(t => t.FullName is string FullName && FullName.Equals(name, StringComparison.Ordinal));
        if (WpfType is Type ExistingWpfType)
        {
            AddToCodeTypes(ExistingWpfType, out NamedType NamedType);

            wpfNamedType = NamedType;
            return true;
        }

        wpfNamedType = null!;
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

    /// <summary>
    /// Tries to get the name of an attribute.
    /// </summary>
    /// <param name="xamlAttribute">The attribute.</param>
    /// <param name="name">The name upon return.</param>
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

    /// <summary>
    /// The Class directive.
    /// </summary>
    public static readonly Directive ClassDirective = new("Class");

    /// <summary>
    /// The ClassModifier directive.
    /// </summary>
    public static readonly Directive ClassModifierDirective = new("ClassModifier");

    /// <summary>
    /// The DeferLoadStrategy directive.
    /// </summary>
    public static readonly Directive DeferLoadStrategyDirective = new("DeferLoadStrategy");

    /// <summary>
    /// The FieldModifier directive.
    /// </summary>
    public static readonly Directive FieldModifierDirective = new("FieldModifier");

    /// <summary>
    /// The Name directive.
    /// </summary>
    public static readonly Directive NameDirective = new("Name");

    /// <summary>
    /// The Subclass directive.
    /// </summary>
    public static readonly Directive SubclassDirective = new("Subclass");

    /// <summary>
    /// The Uid directive.
    /// </summary>
    public static readonly Directive UidDirective = new("Uid");

    private static readonly List<Directive> SupportedDirectives = new()
    {
        ClassDirective,
        ClassModifierDirective,
        DeferLoadStrategyDirective,
        FieldModifierDirective,
        NameDirective,
        SubclassDirective,
        UidDirective,
    };

    private static bool TryGetDirective(IXamlAttribute xamlAttribute, out Directive directive)
    {
        if (xamlAttribute is IXamlAttributeDirective AttributeDirective)
        {
            if (AttributeDirective.Namespace is IXamlNamespaceExtension)
            {
                if (SupportedDirectives.Find(directive => directive.Name == AttributeDirective.Name) is Directive MatchDirective)
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

            if (TryFindCodeType(FullTypeName, out NamedType NamedType))
                if (NamedType.TryGetAttachedProperty(PropertyName, out namedAttachedProperty))
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

    private void AddToCodeTypes(IEnumerable<INamedTypeSymbol> typeSymbols)
    {
        foreach (INamedTypeSymbol Item in typeSymbols)
            if (IsValidTypeSymbol(Item))
                AddToCodeTypes(Item, out _);
    }

    private void AddToCodeTypes(INamedTypeSymbol typeSymbol, out NamedType namedType)
    {
        Debug.Assert(IsValidTypeSymbol(typeSymbol));

        string FullName = typeSymbol.ToString()!;

        if (TryFindCodeType(FullName, out NamedType ExistingNamedType))
            namedType = ExistingNamedType;
        else
        {
            namedType = new NamedType(FullName, FromGetType: null, FromTypeSymbol: typeSymbol);
            CodeTypes.Add(namedType);
        }
    }

    private static bool IsValidTypeSymbol(INamedTypeSymbol typeSymbol)
    {
        var Members = typeSymbol.GetMembers();
        return Members.Any();
    }

    private void AddToCodeTypes(IEnumerable<Type> types)
    {
        foreach (Type Item in types)
            AddToCodeTypes(Item, out _);
    }

    private void AddToCodeTypes(Type type, out NamedType namedType)
    {
        string FullName = type.FullName!;

        if (TryFindCodeType(FullName, out NamedType ExistingNamedType))
            namedType = ExistingNamedType;
        else
        {
            namedType = new NamedType(FullName, FromGetType: type, FromTypeSymbol: null);
            CodeTypes.Add(namedType);
        }
    }

    private bool TryFindCodeType(string fullName, out NamedType namedType)
    {
        foreach (NamedType Item in CodeTypes)
            if (Item.FullName.Equals(fullName, StringComparison.Ordinal))
            {
                namedType = Item;
                return true;
            }

        namedType = null!;
        return false;
    }

    private static readonly Dictionary<string, string> HardcodedWpfTypes = new()
    {
    };

    private static readonly List<Assembly> WpfAssemblies = new()
    {
        typeof(Application).Assembly, // PresentationFramework
        typeof(AccessKeyPressedEventArgs).Assembly, // PresentationCore
        typeof(ActivatingKeyTipEventArgs).Assembly, // Winform ribbons
    };

    private static readonly List<string> WpfNamespaces = new()
    {
        "Microsoft.Windows.Controls",
        "Microsoft.Windows.Input",
        "System.ComponentModel",
        "System.IO",
        "System.IO.Packaging",
        "System.Windows",
        "System.Windows.Automation",
        "System.Windows.Controls",
        "System.Windows.Controls.Primitives",
        "System.Windows.Controls.Ribbon",
        "System.Windows.Controls.Ribbon.Primitives",
        "System.Windows.Data",
        "System.Windows.Diagnostics",
        "System.Windows.Documents",
        "System.Windows.Ink",
        "System.Windows.Input",
        "System.Windows.Input.StylusPlugIns",
        "System.Windows.Input.StylusPointer",
        "System.Windows.Input.StylusWisp",
        "System.Windows.Input.Tracing",
        "System.Windows.Media",
        "System.Windows.Media.Animation",
        "System.Windows.Media.Composition",
        "System.Windows.Media.Effects",
        "System.Windows.Media.Imaging",
        "System.Windows.Media.Media3D",
        "System.Windows.Media.Media3D.Converters",
        "System.Windows.Media.TextFormatting",
        "System.Windows.Navigation",
        "System.Windows.Resources",
        "System.Windows.Shapes",
    };

    private static bool IsValidWpfType(Type t)
    {
        if ((!t.IsClass && !t.IsEnum) || t.IsGenericType || t.IsAbstract || t.IsNotPublic || t.Name[0] == '_')
            return false;

        if (t.DeclaringType is not null)
            return false;

        ConstructorInfo[] Constructors = t.GetConstructors();
        bool HasParameterlessConstructor = false;
        foreach (ConstructorInfo Constructor in Constructors)
            if (Constructor.GetParameters().Length == 0)
                HasParameterlessConstructor = true;

        if (t.IsClass && !HasParameterlessConstructor)
            return false;

        if (t.BaseType == typeof(MulticastDelegate))
            return false;

        if (typeof(Attribute).IsAssignableFrom(t))
            return false;

        return true;
    }

    private static void InitializesWpfTypes()
    {
        List<Type> CombinedAssemblyTypes = new();
        foreach (Assembly WpfAssembly in WpfAssemblies)
            CombinedAssemblyTypes.AddRange(WpfAssembly.GetTypes());

        List<string> WpfTypeNames = new();

        foreach (Type Item in CombinedAssemblyTypes)
        {
            if (!IsValidWpfType(Item))
                continue;

            if (Item.Namespace is string TypeNamespace && WpfNamespaces.Contains(TypeNamespace) && !WpfTypeNames.Contains(Item.Name))
                WpfTypeNames.Add(Item.Name);
        }

        WpfTypeNames.Sort();

        HardcodedWpfTypes.Clear();
        foreach (string name in WpfTypeNames)
            HardcodedWpfTypes.Add(name, "*");

        foreach (Type Item in CombinedAssemblyTypes)
            if (Item.Namespace is string TypeNamespace && WpfNamespaces.Contains(TypeNamespace) && WpfTypeNames.Contains(Item.Name))
                HardcodedWpfTypes[Item.Name] = TypeNamespace;
    }

    private static string GetFullTypeName(string typeNameWithPrefix, Dictionary<string, IXamlNamespace> namespaceTable)
    {
        if (HardcodedWpfTypes.Count == 0)
            InitializesWpfTypes();

        string[] Splitted = typeNameWithPrefix.Split(':');

        if (Splitted.Length == 1)
        {
            string TypeName = Splitted[0];

            if (!HardcodedWpfTypes.TryGetValue(TypeName, out string? Namespace))
                throw new NotSupportedException();

            string FullTypeName = Namespace + "." + TypeName;

            return FullTypeName;
        }
        else
        {
            string Prefix = Splitted[0];
            string TypeName = Splitted[1];

            if (!namespaceTable.TryGetValue(Prefix, out IXamlNamespace? XamlNamespace))
                throw new NotSupportedException();

            string FullTypeName = XamlNamespace.Namespace + "." + TypeName;

            return FullTypeName;
        }
    }

    private readonly List<NamedType> CodeTypes = new();
}
