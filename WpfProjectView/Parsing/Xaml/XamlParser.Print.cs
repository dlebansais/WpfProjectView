namespace WpfProjectView;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Implements a xaml parser.
/// </summary>
public static partial class XamlParser
{
    private static void Print(XamlPrintingContext context)
    {
        SplitElementFields(context);

        if (context.IsFullSingleLine)
        {
            PrintAttributeOnSameLine(context, true);
        }
        else if (context.IsMultipleLineNoChildren)
        {
            PrintAttributeOnMultipleLines(context, true);
        }
        else if (context.NamespaceList.Count == 0)
        {
            PrintAttributeOnSameLine(context, false);
            PrintChildren(context);
        }
        else
        {
            PrintAttributeOnMultipleLines(context, false);
            PrintChildren(context);
        }
    }

    private static void SplitElementFields(XamlPrintingContext context)
    {
        context.AttributeDirectiveList.Clear();
        context.NamespaceList.Clear();
        context.AttributeMemberList.Clear();
        context.ValueString.Clear();
        context.MultiLineElementCollectionAttributeList.Clear();

        foreach (IXamlNamespace Namespace in context.Element.Namespaces)
            if (Namespace.Prefix == string.Empty)
                context.NamespaceList.Add($"xmlns=\"{Namespace.AssemblyPath}\"");
            else
                context.NamespaceList.Add($"xmlns:{Namespace.Prefix}=\"{Namespace.AssemblyPath}\"");

        foreach (IXamlAttribute Attribute in context.Element.Attributes)
        {
            switch (Attribute)
            {
                case XamlAttributeDirective Directive:
                    context.AttributeDirectiveList.Add($"{NameWithPrefix(Directive.Namespace, Directive.Name)}=\"{Directive.Value}\"");
                    break;
                case XamlAttributeSimpleValue SimpleValue:
                    context.ValueString.Clear();
                    context.ValueString.Add(SimpleValue.StringValue);
                    break;
                case XamlAttributeMember Member:
                    context.AttributeMemberList.Add($"{Member.Name}=\"{Member.Value}\"");
                    break;
                case XamlAttributeElementCollection ElementCollection:
                    if (ElementCollection.IsOneLine)
                        context.AttributeMemberList.Add(ElementCollection);
                    else
                        context.MultiLineElementCollectionAttributeList.Add(ElementCollection);
                    break;
            }
        }
    }

    private static string AttributeMemberToString(XamlPrintingContext context, object item)
    {
        string Result = string.Empty;

        if (item is string AttributeMemberString)
            Result = AttributeMemberString;

        if (item is XamlAttributeElementCollection ElementCollectionAttribute)
        {
            string ChildString = string.Empty;

            IXamlElementCollection ElementCollection = ElementCollectionAttribute.Children;
            foreach (IXamlElement Child in ElementCollection)
                ChildString += OneLineElement(Child);

            Result = $"{ElementCollectionAttribute.Name}=\"{ChildString}\"";
        }

        return Result;
    }

    private static void PrintAttributeOnSameLine(XamlPrintingContext context, bool endTag)
    {
        Debug.Assert(context.NamespaceList.Count <= 1);
        Debug.Assert(context.ValueString.Count == 0 || endTag);

        string ElementName = NameWithPrefix(context.Element.Namespace, context.Element.Name);

        string AttributeDirectiveString = string.Join(' ', context.AttributeDirectiveList);
        if (AttributeDirectiveString != string.Empty)
            AttributeDirectiveString = " " + AttributeDirectiveString;

        string NamespaceString = context.NamespaceList.Count > 0 ? " " + context.NamespaceList[0] : string.Empty;

        string AttributeMembers = string.Empty;
        foreach (object Item in context.AttributeMemberList)
            AttributeMembers += " " + AttributeMemberToString(context, Item);

        if (context.ValueString.Count == 0)
        {
            string Termination = endTag ? "/>" : ">";
            context.Builder.AppendLine($"{Whitespaces(context.Indentation)}<{ElementName}{AttributeDirectiveString}{NamespaceString}{AttributeMembers}{Termination}");
        }
        else
        {
            context.Builder.AppendLine($"{Whitespaces(context.Indentation)}<{ElementName}{AttributeDirectiveString}{NamespaceString}{AttributeMembers}>{context.ValueString[0]}</{ElementName}>");
        }
    }

    private static void PrintAttributeOnMultipleLines(XamlPrintingContext context, bool endTag)
    {
        string ElementName = NameWithPrefix(context.Element.Namespace, context.Element.Name);

        string FirstDirectiveString = (context.AttributeDirectiveList.Count > 0) ? context.AttributeDirectiveList[0] : context.NamespaceList[0];
        string LastDirectiveString = (context.AttributeMemberList.Count > 0) ? AttributeMemberToString(context, context.AttributeMemberList[context.AttributeMemberList.Count - 1]) : context.NamespaceList[context.NamespaceList.Count - 1];

        context.Builder.AppendLine($"{Whitespaces(context.Indentation)}<{ElementName} {FirstDirectiveString}");

        string ElementIndentation = Whitespaces(context.Indentation) + "  ";
        for (int Index = 0; Index < ElementName.Length; Index++)
            ElementIndentation += " ";

        for (int Index = 1; Index < context.AttributeDirectiveList.Count; Index++)
            context.Builder.AppendLine($"{ElementIndentation}{context.AttributeDirectiveList[Index]}");

        int NamespaceStartIndex = context.AttributeDirectiveList.Count > 0 ? 0 : 1;
        int NamespaceEndIndex = context.AttributeMemberList.Count > 0 ? context.NamespaceList.Count : context.NamespaceList.Count - 1;

        for (int Index = NamespaceStartIndex; Index < NamespaceEndIndex; Index++)
            context.Builder.AppendLine($"{ElementIndentation}{context.NamespaceList[Index]}");

        for (int Index = 0; Index + 1 < context.AttributeMemberList.Count; Index++)
            context.Builder.AppendLine($"{ElementIndentation}{context.AttributeMemberList[Index]}");

        string Termination = endTag ? "/>" : ">";

        context.Builder.AppendLine($"{ElementIndentation}{LastDirectiveString}{Termination}");
    }

    private static void PrintChildren(XamlPrintingContext context)
    {
        string ElementName = NameWithPrefix(context.Element.Namespace, context.Element.Name);
        List<XamlAttributeElementCollection> MultiLineElementCollectionAttributeList = context.MultiLineElementCollectionAttributeList;
        IXamlElementCollection Children = context.Element.Children;

        foreach (XamlAttributeElementCollection ElementCollectionAttribute in MultiLineElementCollectionAttributeList)
        {
            context.Builder.AppendLine($"{Whitespaces(context.Indentation + 1)}<{ElementName}.{ElementCollectionAttribute.Name}>");

            IXamlElementCollection ElementCollection = ElementCollectionAttribute.Children;

            foreach (IXamlElement Child in ElementCollection)
            {
                XamlPrintingContext ChildContext = new(context.Builder, Child, context.Indentation + 2);
                Print(ChildContext);
            }

            context.Builder.AppendLine($"{Whitespaces(context.Indentation + 1)}</{ElementName}.{ElementCollectionAttribute.Name}>");
        }

        foreach (IXamlElement Child in Children)
        {
            XamlPrintingContext ChildContext = new(context.Builder, Child, context.Indentation + 1);
            Print(ChildContext);
        }

        context.Builder.AppendLine($"{Whitespaces(context.Indentation)}</{ElementName}>");
    }

    private static string OneLineElement(IXamlElement element)
    {
        string ElementName = NameWithPrefix(element.Namespace, element.Name);

        string AttributeListString = string.Empty;

        foreach (IXamlAttribute Attribute in element.Attributes)
        {
            if (AttributeListString.Length > 0)
                AttributeListString += ", ";

            switch (Attribute)
            {
                case XamlAttributeDirective Directive:
                    AttributeListString += "d";
                    break;
                case XamlAttributeMember Member:
                    string MemberString = $"{Member.Value}";
                    if (Member.Name != string.Empty)
                        MemberString = $"{Member.Name}=" + MemberString;

                    AttributeListString += MemberString;
                    break;
                case XamlAttributeElementCollection ElementCollection:
                    string ElementCollectionString = string.Empty;

                    foreach (IXamlElement Child in ElementCollection.Children)
                        ElementCollectionString += OneLineElement(Child);

                    if (ElementCollection.Name != string.Empty)
                        ElementCollectionString = $"{ElementCollection.Name}=" + ElementCollectionString;

                    AttributeListString += ElementCollectionString;
                    break;
                default:
                    break;
            }
        }

        if (AttributeListString.Length > 0)
            AttributeListString = " " + AttributeListString;

        string Result = $"{{{ElementName}{AttributeListString}}}";

        return Result;
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
        else if (@namespace is XamlNamespaceExtension && name.EndsWith("Extension"))
            return $"{@namespace.Prefix}:{name.Substring(0, name.Length - 9)}";
        else
            return $"{@namespace.Prefix}:{name}";
    }
}
