namespace WpfProjectView;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

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
        else if (context.IsSingleLineWithChildren)
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
            context.NamespaceList.Add(NamespaceToString(Namespace));

        foreach (IXamlAttribute Attribute in context.Element.Attributes)
        {
            switch (Attribute)
            {
                case XamlAttributeDirective Directive:
                    if (Directive.Value is IXamlElement AsChild)
                    {
                        string ChildString = OneLineElement(AsChild);
                        context.AttributeDirectiveList.Add($"{NameWithPrefix(Directive.Namespace, Directive.Name)}=\"{ChildString}\"");
                    }
                    else
                    {
                        context.AttributeDirectiveList.Add($"{NameWithPrefix(Directive.Namespace, Directive.Name)}=\"{Directive.Value}\"");
                    }

                    break;
                case XamlAttributeSimpleValue SimpleValue:
                    context.ValueString.Clear();
                    context.ValueString.Add(SimpleValue.StringValue);
                    bool StringValueIsValue = SimpleValue.StringValue == (string)SimpleValue.Value!;
                    Debug.Assert(StringValueIsValue);
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
            bool ChildrenIsValue = ElementCollection == (IXamlElementCollection)ElementCollectionAttribute.Value!;
            Debug.Assert(ChildrenIsValue);

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

        List<string> Parts = new();
        Parts.AddRange(context.AttributeDirectiveList);
        Parts.AddRange(context.NamespaceList);
        Parts.AddRange(context.AttributeMemberList.ConvertAll(item => AttributeMemberToString(context, item)));

        string AttributeString = string.Empty;
        foreach (string Part in Parts)
            AttributeString += $" {Part}";

        if (context.ValueString.Count == 0)
        {
            string Termination = endTag ? "/>" : ">";
            context.AppendLine($"{Whitespaces(context.Indentation)}<{ElementName}{AttributeString}{Termination}");
        }
        else
        {
            context.AppendLine($"{Whitespaces(context.Indentation)}<{ElementName}{AttributeString}>{context.ValueString[0]}</{ElementName}>");
        }
    }

    private static void PrintAttributeOnMultipleLines(XamlPrintingContext context, bool endTag)
    {
        string ElementName = NameWithPrefix(context.Element.Namespace, context.Element.Name);

        List<string> Parts = new();
        Parts.AddRange(context.AttributeDirectiveList);
        Parts.AddRange(context.NamespaceList);
        Parts.AddRange(context.AttributeMemberList.ConvertAll(item => AttributeMemberToString(context, item)));

        Debug.Assert(Parts.Count >= 2);

        string FirstDirectiveString = Parts[0];
        string LastDirectiveString = Parts[Parts.Count - 1];
        Parts.RemoveAt(0);
        Parts.RemoveAt(Parts.Count - 1);

        context.AppendLine($"{Whitespaces(context.Indentation)}<{ElementName} {FirstDirectiveString}");

        string ElementIndentation = Whitespaces(context.Indentation) + "  ";
        for (int Index = 0; Index < ElementName.Length; Index++)
            ElementIndentation += " ";

        foreach (string Part in Parts)
            context.AppendLine($"{ElementIndentation}{Part}");

        string Termination = endTag ? "/>" : ">";
        context.AppendLine($"{ElementIndentation}{LastDirectiveString}{Termination}");
    }

    private static void PrintChildren(XamlPrintingContext context)
    {
        string ElementName = NameWithPrefix(context.Element.Namespace, context.Element.Name);
        List<XamlAttributeElementCollection> MultiLineElementCollectionAttributeList = context.MultiLineElementCollectionAttributeList;
        IXamlElementCollection Children = context.Element.Children;
        int Indentation = context.Indentation + 1;

        foreach (XamlAttributeElementCollection ElementCollectionAttribute in MultiLineElementCollectionAttributeList)
        {
            bool IsVisible = ElementCollectionAttribute.IsVisible;

            if (IsVisible)
            {
                context.AppendLine($"{Whitespaces(Indentation)}<{ElementName}.{ElementCollectionAttribute.Name}>");
                Indentation++;
            }

            IXamlElementCollection ElementCollection = ElementCollectionAttribute.Children;

            foreach (IXamlElement Child in ElementCollection)
            {
                XamlPrintingContext ChildContext = new(context.Builder, Child, Indentation);
                Print(ChildContext);
            }

            if (IsVisible)
            {
                Indentation--;
                context.AppendLine($"{Whitespaces(Indentation)}</{ElementName}.{ElementCollectionAttribute.Name}>");
            }
        }

        foreach (IXamlElement Child in Children)
        {
            XamlPrintingContext ChildContext = new(context.Builder, Child, context.Indentation + 1);
            Print(ChildContext);
        }

        context.AppendLine($"{Whitespaces(context.Indentation)}</{ElementName}>");
    }

    private static void AppendLine(this XamlPrintingContext context, string line)
    {
#if NET6_0_OR_GREATER
        _ = context.Builder.AppendLine(CultureInfo.InvariantCulture, $"{line}");
#else
        _ = context.Builder.AppendLine(line);
#endif
    }

    private static string OneLineElement(IXamlElement element)
    {
        string ElementName = NameWithPrefix(element.Namespace, element.Name);

        string AttributeListString = string.Empty;

        foreach (IXamlAttribute Attribute in element.Attributes)
        {
            if (AttributeListString.Length > 0)
                AttributeListString += ", ";

            AttributeListString += OneLineElementAttribute(Attribute);
        }

        if (AttributeListString.Length > 0)
            AttributeListString = " " + AttributeListString;

        string Result = $"{{{ElementName}{AttributeListString}}}";

        return Result;
    }

    private static string OneLineElementAttribute(IXamlAttribute attribute)
    {
        string? AttributeString = null;

        if (attribute is XamlAttributeMember Member)
            AttributeString = OneLineElementAttributeMember(Member);

        if (attribute is XamlAttributeElementCollection ElementCollection)
            AttributeString = OneLineElementAttributeElementCollection(ElementCollection);

        Debug.Assert(AttributeString is not null);

        return AttributeString!;
    }

    private static string OneLineElementAttributeMember(XamlAttributeMember member)
    {
        string? MemberString = null;

        if (member.Value is string ValueString)
            MemberString = ValueString;

        if (member.Value is IXamlElement NestedElement)
            MemberString = OneLineElement(NestedElement);

        Debug.Assert(MemberString is not null);

        if (member.Name != string.Empty)
            MemberString = $"{member.Name}=" + MemberString;

        return MemberString!;
    }

    private static string OneLineElementAttributeElementCollection(XamlAttributeElementCollection elementCollection)
    {
        IXamlElementCollection Children = elementCollection.Children;

        Debug.Assert(Children.Count > 0);

        string ElementCollectionString = $"{elementCollection.Name}=";

        foreach (IXamlElement Child in Children)
            ElementCollectionString += OneLineElement(Child);

        return ElementCollectionString;
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
        else if (@namespace is XamlNamespaceExtension && name.EndsWith("Extension", StringComparison.Ordinal))
            return $"{@namespace.Prefix}:{name.Substring(0, name.Length - 9)}";
        else
            return $"{@namespace.Prefix}:{name}";
    }

    private static string NamespaceToString(IXamlNamespace @namespace)
    {
        if (@namespace.Prefix == string.Empty)
            return $"xmlns=\"{@namespace.AssemblyPath}\"";
        else
            return $"xmlns:{@namespace.Prefix}=\"{@namespace.AssemblyPath}\"";
    }
}
