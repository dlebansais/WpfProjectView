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
        SplitElementFields(element, out List<string> AttributeDirectiveList, out List<string> NamespaceList, out List<string> AttributeMemberList, out List<IXamlAttribute> ElementCollectionAttributeList);

        if (element.Children.Count == 0 && ElementCollectionAttributeList.Count == 0 && (NamespaceList.Count == 0 || (NamespaceList.Count == 1 && AttributeDirectiveList.Count == 0 && AttributeMemberList.Count == 0)))
        {
            PrintAttributeOnSameLine(element, indentation, AttributeDirectiveList, NamespaceList, AttributeMemberList, true);
        }
        else if (element.Children.Count == 0 && ElementCollectionAttributeList.Count == 0)
        {
            PrintAttributeOnMultipleLines(element, indentation, AttributeDirectiveList, NamespaceList, AttributeMemberList, "/>");
        }
        else
        {
            if (NamespaceList.Count == 0)
            {
                PrintAttributeOnSameLine(element, indentation, AttributeDirectiveList, NamespaceList, AttributeMemberList, false);
                PrintChildren(element, indentation, ElementCollectionAttributeList);
            }
            else
            {
                PrintAttributeOnMultipleLines(element, indentation, AttributeDirectiveList, NamespaceList, AttributeMemberList, ">");
                PrintChildren(element, indentation, ElementCollectionAttributeList);
            }
        }
    }

    private static void SplitElementFields(IXamlElement element, out List<string> attributeDirectiveList, out List<string> namespaceList, out List<string> attributeMemberList, out List<IXamlAttribute> elementCollectionAttributeList)
    {
        attributeDirectiveList = new();
        namespaceList = new();
        attributeMemberList = new();
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
                    attributeDirectiveList.Add($"{Directive.Prefix}{Directive.Name}=\"{Directive.Value}\"");
                    break;
                case XamlAttributeSimpleValue SimpleValue:
                    attributeMemberList.Add(SimpleValue.StringValue);
                    break;
                case XamlAttributeMember Member:
                    attributeMemberList.Add($"{Member.Name}=\"{Member.Value}\"");
                    break;
                case XamlAttributeElementCollection:
                    elementCollectionAttributeList.Add(Attribute);
                    break;
            }
        }
    }

    private static void PrintAttributeOnSameLine(IXamlElement element, int indentation, List<string> attributeDirectiveList, List<string> namespaceList, List<string> attributeMemberList, bool endTag)
    {
        if (attributeDirectiveList.Count == 0 && namespaceList.Count == 0 && attributeMemberList.Count == 1 && endTag)
        {
            string MemberString = attributeMemberList[0];
            Debug.WriteLine($"{Whitespaces(indentation)}<{element.Name}>{MemberString}</{element.Name}>");
        }
        else
        {
            string AttributeDirectiveString = string.Join(' ', attributeDirectiveList);
            if (AttributeDirectiveString != string.Empty)
                AttributeDirectiveString = " " + AttributeDirectiveString;

            string NamespaceString = namespaceList.Count > 0 ? " " + namespaceList[0] : string.Empty;

            string AttributeMemberString = string.Join(' ', attributeMemberList);
            if (AttributeMemberString != string.Empty)
                AttributeMemberString = " " + AttributeMemberString;

            string Temrination = endTag ? "/>" : ">";

            Debug.WriteLine($"{Whitespaces(indentation)}<{element.Name}{AttributeDirectiveString}{NamespaceString}{AttributeMemberString}{Temrination}");
        }
    }

    private static void PrintAttributeOnMultipleLines(IXamlElement element, int indentation, List<string> attributeDirectiveList, List<string> namespaceList, List<string> attributeMemberList, string termination)
    {
        string FirstDirectiveString = (attributeDirectiveList.Count > 0) ? attributeDirectiveList[0] : namespaceList[0];
        string LastDirectiveString = (attributeMemberList.Count > 0) ? attributeMemberList[attributeMemberList.Count - 1] : namespaceList[namespaceList.Count - 1];

        Debug.WriteLine($"{Whitespaces(indentation)}<{element.Name} {FirstDirectiveString}");

        string ElementIndentation = Whitespaces(indentation) + "  ";
        for (int Index = 0; Index < element.Name.Length; Index++)
            ElementIndentation += " ";

        for (int Index = 1; Index < attributeDirectiveList.Count; Index++)
            Debug.WriteLine($"{ElementIndentation}{attributeDirectiveList[Index]}");

        int NamespaceStartIndex = attributeDirectiveList.Count > 0 ? 0 : 1;
        int NamespaceEndIndex = attributeMemberList.Count > 0 ? namespaceList.Count : namespaceList.Count - 1;

        for (int Index = NamespaceStartIndex; Index < NamespaceEndIndex; Index++)
            Debug.WriteLine($"{ElementIndentation}{namespaceList[Index]}");

        for (int Index = 0; Index + 1 < attributeMemberList.Count; Index++)
            Debug.WriteLine($"{ElementIndentation}{attributeMemberList[Index]}");

        Debug.WriteLine($"{ElementIndentation}{LastDirectiveString}{termination}");
    }

    private static void PrintChildren(IXamlElement element, int indentation, List<IXamlAttribute> elementCollectionAttributeList)
    {
        foreach (IXamlAttribute ElementCollectionAttribute in elementCollectionAttributeList)
        {
            Debug.WriteLine($"{Whitespaces(indentation + 1)}<{element.Name}.{ElementCollectionAttribute.Name}>");

            if (ElementCollectionAttribute.Value is IXamlElementCollection ElementCollection)
                foreach (IXamlElement Child in ElementCollection)
                    Print(Child, indentation + 2);

            Debug.WriteLine($"{Whitespaces(indentation + 1)}</{element.Name}.{ElementCollectionAttribute.Name}>");
        }

        foreach (IXamlElement Child in element.Children)
            Print(Child, indentation + 1);

        Debug.WriteLine($"{Whitespaces(indentation)}<{element.Name}/>");
    }

    private static string Whitespaces(int indentation)
    {
        string Result = string.Empty;

        for (int Index = 0; Index < indentation; Index++)
            Result += "    ";

        return Result;
    }
}
