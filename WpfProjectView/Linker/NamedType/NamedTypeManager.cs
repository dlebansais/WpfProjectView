namespace WpfProjectView;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a manager of named types.
/// </summary>
public class NamedTypeManager
{
    /// <summary>
    /// Fills the list of types from code files and DLLs they reference.
    /// </summary>
    /// <param name="parsedSyntaxTrees">The list of C# syntax trees.</param>
    /// <param name="pathToExternalDlls">The list of path to referenced DLLs.</param>
    public async Task FillCodeTypes(IList<SyntaxTree> parsedSyntaxTrees, IReadOnlyList<string> pathToExternalDlls)
    {
        Contract.RequireNotNull(parsedSyntaxTrees, out List<SyntaxTree> ParsedSyntaxTrees);
        Contract.RequireNotNull(pathToExternalDlls, out IReadOnlyList<string> PathToExternalDlls);

        Dictionary<SyntaxTree, IEnumerable<SyntaxNode>> SyntaxTreeTable = new();
        foreach (SyntaxTree Item in ParsedSyntaxTrees)
        {
            SyntaxNode TreeRoot = await Item.GetRootAsync().ConfigureAwait(false);
            IEnumerable<SyntaxNode> Nodes = TreeRoot.DescendantNodes(node => true);
            SyntaxTreeTable.Add(TreeRoot.SyntaxTree, Nodes);
        }

        string RuntimePath = GetRuntimePath();
        Console.WriteLine(RuntimePath);

        List<MetadataReference> DefaultReferences = new()
        {
            MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "mscorlib")),
            MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "System")),
            MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "System.Core")),
            MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "System.Xaml")),
            MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "PresentationCore")),
            MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "PresentationFramework")),
        };

        foreach (string PathToExternalDll in PathToExternalDlls)
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

        foreach (string PathToExternalDll in PathToExternalDlls)
        {
            Assembly LoadedAssembly = Assembly.LoadFile(PathToExternalDll);
            Type[] ExportedTypes = LoadedAssembly.GetExportedTypes();
            AddToCodeTypes(ExportedTypes);
        }
    }

    private static string GetRuntimePath()
    {
        const string RuntimeDirectoryBase = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework";
        string RuntimeDirectory = string.Empty;

        foreach (string FolderPath in System.IO.Directory.GetDirectories(RuntimeDirectoryBase))
            if (IsValidRuntimeDirectory(FolderPath))
                if (string.Compare(FolderPath, RuntimeDirectory, StringComparison.OrdinalIgnoreCase) > 0)
                    RuntimeDirectory = FolderPath;

        string RuntimePath = RuntimeDirectory + @"\{0}.dll";

        return RuntimePath;
    }

    private static bool IsValidRuntimeDirectory(string folderPath)
    {
        string FolderName = System.IO.Path.GetFileName(folderPath);
        const string Prefix = "v";

        if (!FolderName.StartsWith(Prefix, StringComparison.Ordinal))
            return false;

        string[] Parts = FolderName.Substring(Prefix.Length).Split('.');
        foreach (string Part in Parts)
            if (!int.TryParse(Part, out _))
                return false;

        return true;
    }

    /// <summary>
    /// Tries to find a named type in the list.
    /// </summary>
    /// <param name="fullName">The type name with namespace.</param>
    /// <param name="namedType">The type found upon return.</param>
    public bool TryFindCodeType(string fullName, out NamedType namedType)
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

    /// <summary>
    /// Tries to find a named type among standard WPF types.
    /// </summary>
    /// <param name="name">The full WPF type name.</param>
    /// <param name="wpfNamedType">The named type upon return.</param>
    public bool TryFindWpfNamedType(string name, out NamedType wpfNamedType)
    {
        if (WpfTypes is null)
        {
            List<Assembly> WpfAssemblies = new() { typeof(Application).Assembly };
            WpfTypes = WpfAssemblies.Where(a => !a.IsDynamic).SelectMany(a => a.GetTypes());
        }

        Type? WpfType = WpfTypes.FirstOrDefault(t => t.FullName is string FullName && FullName.Equals(name, StringComparison.Ordinal));

        if (WpfType is Type ExistingWpfType)
        {
            AddToCodeTypes(ExistingWpfType, out NamedType NamedType);

            wpfNamedType = NamedType;
            return true;
        }

        wpfNamedType = null!;
        return false;
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

    private IEnumerable<Type>? WpfTypes;
    private readonly List<NamedType> CodeTypes = new();
}
