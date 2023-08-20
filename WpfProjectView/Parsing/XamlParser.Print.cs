namespace WpfProjectView;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Implements a xaml parser.
/// </summary>
internal static partial class XamlParser
{
    private static void Print(IXamlElement element, int indentation)
    {
        SplitElementFields(element, out List<string> AttributeDirectiveList, out List<string> NamespaceList, out List<string> AttributeMemberList, out string? ValueString, out List<XamlAttributeElementCollection> ElementCollectionAttributeList);

        if (element.Children.Count == 0 && ElementCollectionAttributeList.Count == 0 && (NamespaceList.Count == 0 || (NamespaceList.Count == 1 && AttributeDirectiveList.Count == 0 && AttributeMemberList.Count == 0) || ValueString is not null))
        {
            PrintAttributeOnSameLine(element, indentation, AttributeDirectiveList, NamespaceList, AttributeMemberList, ValueString, true);
        }
        else if (element.Children.Count == 0 && ElementCollectionAttributeList.Count == 0)
        {
            PrintAttributeOnMultipleLines(element, indentation, AttributeDirectiveList, NamespaceList, AttributeMemberList, true);
        }
        else
        {
            if (NamespaceList.Count == 0)
            {
                PrintAttributeOnSameLine(element, indentation, AttributeDirectiveList, NamespaceList, AttributeMemberList, null, false);
                PrintChildren(element, indentation, ElementCollectionAttributeList);
            }
            else
            {
                PrintAttributeOnMultipleLines(element, indentation, AttributeDirectiveList, NamespaceList, AttributeMemberList, false);
                PrintChildren(element, indentation, ElementCollectionAttributeList);
            }
        }
    }

    private static void SplitElementFields(IXamlElement element, out List<string> attributeDirectiveList, out List<string> namespaceList, out List<string> attributeMemberList, out string? valueString, out List<XamlAttributeElementCollection> elementCollectionAttributeList)
    {
        attributeDirectiveList = new();
        namespaceList = new();
        attributeMemberList = new();
        valueString = null;
        elementCollectionAttributeList = new();

        foreach (IXamlNamespace Namespace in element.Namespaces)
            if (Namespace.Prefix == string.Empty)
                namespaceList.Add($"xmlns=\"{Namespace.AssemblyPath}\"");
            else
                namespaceList.Add($"xmlns:{Namespace.Prefix}=\"{Namespace.AssemblyPath}\"");

        foreach (IXamlAttribute Attribute in element.Attributes)
        {
            switch (Attribute)
            {
                case XamlAttributeDirective Directive:
                    attributeDirectiveList.Add($"{NameWithPrefix(Directive.Namespace, Directive.Name)}=\"{Directive.Value}\"");
                    break;
                case XamlAttributeSimpleValue SimpleValue:
                    valueString = SimpleValue.StringValue;
                    break;
                case XamlAttributeMember Member:
                    attributeMemberList.Add($"{Member.Name}=\"{Member.Value}\"");
                    break;
                case XamlAttributeElementCollection ElementCollection:
                    elementCollectionAttributeList.Add(ElementCollection);
                    break;
            }
        }
    }

    private static void PrintAttributeOnSameLine(IXamlElement element, int indentation, List<string> attributeDirectiveList, List<string> namespaceList, List<string> attributeMemberList, string? valueString, bool endTag)
    {
        Debug.Assert(namespaceList.Count <= 1);
        Debug.Assert(valueString is null || endTag);

        string ElementName = NameWithPrefix(element.Namespace, element.Name);

        string AttributeDirectiveString = string.Join(' ', attributeDirectiveList);
        if (AttributeDirectiveString != string.Empty)
            AttributeDirectiveString = " " + AttributeDirectiveString;

        string NamespaceString = namespaceList.Count > 0 ? " " + namespaceList[0] : string.Empty;

        string AttributeMemberString = string.Join(' ', attributeMemberList);
        if (AttributeMemberString != string.Empty)
            AttributeMemberString = " " + AttributeMemberString;

        if (valueString is null)
        {
            string Termination = endTag ? "/>" : ">";
            Debug.WriteLine($"{Whitespaces(indentation)}<{ElementName}{AttributeDirectiveString}{NamespaceString}{AttributeMemberString}{Termination}");
        }
        else
        {
            Debug.WriteLine($"{Whitespaces(indentation)}<{ElementName}{AttributeDirectiveString}{NamespaceString}{AttributeMemberString}>{valueString}</{ElementName}");
        }
    }

    private static void PrintAttributeOnMultipleLines(IXamlElement element, int indentation, List<string> attributeDirectiveList, List<string> namespaceList, List<string> attributeMemberList, bool endTag)
    {
        string ElementName = NameWithPrefix(element.Namespace, element.Name);

        string FirstDirectiveString = (attributeDirectiveList.Count > 0) ? attributeDirectiveList[0] : namespaceList[0];
        string LastDirectiveString = (attributeMemberList.Count > 0) ? attributeMemberList[attributeMemberList.Count - 1] : namespaceList[namespaceList.Count - 1];

        Debug.WriteLine($"{Whitespaces(indentation)}<{ElementName} {FirstDirectiveString}");

        string ElementIndentation = Whitespaces(indentation) + "  ";
        for (int Index = 0; Index < ElementName.Length; Index++)
            ElementIndentation += " ";

        for (int Index = 1; Index < attributeDirectiveList.Count; Index++)
            Debug.WriteLine($"{ElementIndentation}{attributeDirectiveList[Index]}");

        int NamespaceStartIndex = attributeDirectiveList.Count > 0 ? 0 : 1;
        int NamespaceEndIndex = attributeMemberList.Count > 0 ? namespaceList.Count : namespaceList.Count - 1;

        for (int Index = NamespaceStartIndex; Index < NamespaceEndIndex; Index++)
            Debug.WriteLine($"{ElementIndentation}{namespaceList[Index]}");

        for (int Index = 0; Index + 1 < attributeMemberList.Count; Index++)
            Debug.WriteLine($"{ElementIndentation}{attributeMemberList[Index]}");

        string Termination = endTag ? "/>" : ">";

        Debug.WriteLine($"{ElementIndentation}{LastDirectiveString}{Termination}");
    }

    private static void PrintChildren(IXamlElement element, int indentation, List<XamlAttributeElementCollection> elementCollectionAttributeList)
    {
        string ElementName = NameWithPrefix(element.Namespace, element.Name);

        foreach (XamlAttributeElementCollection ElementCollectionAttribute in elementCollectionAttributeList)
        {
            Debug.WriteLine($"{Whitespaces(indentation + 1)}<{ElementName}.{ElementCollectionAttribute.Name}>");

            IXamlElementCollection ElementCollection = ElementCollectionAttribute.Children;
            foreach (IXamlElement Child in ElementCollection)
                Print(Child, indentation + 2);

            Debug.WriteLine($"{Whitespaces(indentation + 1)}</{ElementName}.{ElementCollectionAttribute.Name}>");
        }

        foreach (IXamlElement Child in element.Children)
            Print(Child, indentation + 1);

        Debug.WriteLine($"{Whitespaces(indentation)}<{ElementName}/>");
    }

    private static string Whitespaces(int indentation)
    {
        string Result = string.Empty;

        for (int Index = 0; Index < indentation; Index++)
            Result += "    ";

        return Result;
    }

    private static string NameWithPrefix(IXamlNamespace @namespace, string name)
    {
        if (@namespace.Prefix == string.Empty)
            return name;
        else
            return $"{@namespace.Prefix}:{name}";
    }
}
