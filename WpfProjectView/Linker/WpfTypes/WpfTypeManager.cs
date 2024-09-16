namespace WpfProjectView;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Contracts;

/// <summary>
/// Represents a manager of WPF types.
/// </summary>
public static class WpfTypeManager
{
    /// <summary>
    /// Tries to find a WPF type by its name.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <param name="fullTypeName">The full type name with namespace if found.</param>
    public static bool TryFindType(string typeName, out string fullTypeName)
    {
        if (HardcodedWpfTypes.Count == 0)
            InitializesWpfTypes();

        if (HardcodedWpfTypes.TryGetValue(typeName, out string? Namespace))
        {
            fullTypeName = Namespace + "." + typeName;
            return true;
        }

        Contract.Unused(out fullTypeName);
        return false;
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

            string TypeNamespace = Contract.AssertNotNull(Item.Namespace);

            if (WpfNamespaces.Contains(TypeNamespace))
            {
                Contract.Assert(!WpfTypeNames.Contains(Item.Name));
                WpfTypeNames.Add(Item.Name);
            }
        }

        WpfTypeNames.Sort();

        HardcodedWpfTypes.Clear();
        foreach (string name in WpfTypeNames)
            HardcodedWpfTypes.Add(name, "*");

        foreach (Type Item in CombinedAssemblyTypes)
            if (Item.Namespace is string TypeNamespace && WpfNamespaces.Contains(TypeNamespace) && WpfTypeNames.Contains(Item.Name))
                HardcodedWpfTypes[Item.Name] = TypeNamespace;
    }

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

        if (typeof(Attribute).IsAssignableFrom(t))
            return false;

        return true;
    }

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

    private static readonly Dictionary<string, string> HardcodedWpfTypes = new();
}
