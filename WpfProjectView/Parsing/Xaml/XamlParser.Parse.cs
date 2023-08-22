namespace WpfProjectView;

using System;
using System.Xaml;

/// <summary>
/// Implements a xaml parser.
/// </summary>
public static partial class XamlParser
{
    private static XamlElement ParseElement(XamlParsingContext context)
    {
        IXamlNamespace ElementNamespace;
        string ElementName;
        XamlNamespaceCollection Namespaces = new();
        XamlElementCollection Children = new();
        XamlAttributeCollection Attributes = new();

        ParseNamespaceDeclarations(context, Namespaces);

        XamlNamespaceCollection NewNamespaces = new(context.Namespaces);
        NewNamespaces.AddRange(Namespaces);
        context = context with { Namespaces = NewNamespaces };

        if (context.NodeType != XamlNodeType.StartObject)
            throw new InvalidXamlFormatException("Start object tag expected.");

        ElementNamespace = context.ObjectNamespace;
        ElementName = context.ObjectName;
        context = context with { CurrentObjectName = ElementName };

        context.Read();

        ParseElementContent(context, Children, Attributes, out bool IsMultiLine);

        return new XamlElement(ElementNamespace, ElementName, Namespaces, Children, Attributes, IsMultiLine);
    }

    private static void ParseNamespaceDeclarations(XamlParsingContext context, XamlNamespaceCollection namespaces)
    {
        while (context.NodeType == XamlNodeType.NamespaceDeclaration)
        {
            NamespaceDeclaration Namespace = context.NamespacePath;
            IXamlNamespace NewNamespace = XamlNamespace.Create(Namespace.Prefix, Namespace.Namespace);
            namespaces.Add(NewNamespace);

            context.Read();
        }
    }

    private static void ParseElementContent(XamlParsingContext context, XamlElementCollection children, XamlAttributeCollection attributes, out bool isMultiLine)
    {
        isMultiLine = false;

        int CurrentLineNumber = context.LineNumber;

        while (context.NodeType == XamlNodeType.StartMember)
        {
            if (context.Member == XamlLanguage.Initialization)
            {
                ParseElementMemberInitialization(context, attributes);
            }
            else if (context.Member == XamlLanguage.UnknownContent)
            {
                ParseElementMemberUnknownContent(context, children, attributes);
            }
            else if (context.Member == XamlLanguage.PositionalParameters)
            {
                ParseElementMemberPositionalParameter(context, attributes);
            }
            else if (context.Member is XamlDirective AsDirective)
            {
                ParseElementMemberDirective(context, AsDirective, attributes);
            }
            else
            {
                ParseElementMember(context, attributes, out bool CheckMultiLine);

                if (CheckMultiLine && context.LineNumber > CurrentLineNumber && attributes.Count > 1)
                    isMultiLine = true;
            }

            context.Read();
        }

        if (context.NodeType != XamlNodeType.EndObject)
            throw new NotImplementedException();
    }

    private static void ParseElementMemberInitialization(XamlParsingContext context, XamlAttributeCollection attributes)
    {
        context.Read();

        if (context.NodeType != XamlNodeType.Value)
            throw new NotImplementedException();

        if (context.Value is not string StringValue)
            throw new NotImplementedException();

        XamlAttributeSimpleValue Attribute = new(StringValue);
        attributes.Add(Attribute);

        context.Read();

        if (context.NodeType != XamlNodeType.EndMember)
            throw new NotImplementedException();
    }

    private static void ParseElementMemberUnknownContent(XamlParsingContext context, XamlElementCollection children, XamlAttributeCollection attributes)
    {
        context.Read();

        switch (context.NodeType)
        {
            case XamlNodeType.Value:
                ParseElementMemberUnknownContentSimpleValue(context, attributes);
                break;
            case XamlNodeType.StartObject:
                ParseElementMemberUnknownContentObjects(context, children);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private static void ParseElementMemberUnknownContentObjects(XamlParsingContext context, XamlElementCollection children)
    {
        for (; ;)
        {
            IXamlElement Child = ParseElement(context);
            children.Add(Child);

            if (context.NodeType != XamlNodeType.EndObject)
                throw new NotImplementedException();

            context.Read();

            SkipIndentation(context);

            if (context.NodeType == XamlNodeType.EndMember)
                break;
        }
    }

    private static void ParseElementMemberUnknownContentSimpleValue(XamlParsingContext context, XamlAttributeCollection attributes)
    {
        if (context.Value is string StringValue)
        {
            XamlAttributeSimpleValue Attribute = new(StringValue);
            attributes.Add(Attribute);

            context.Read();

            if (context.NodeType != XamlNodeType.EndMember)
                throw new NotImplementedException();

            return;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static void ParseElementMemberPositionalParameter(XamlParsingContext context, XamlAttributeCollection attributes)
    {
        context.Read();

        object? AttributeValue;

        switch (context.NodeType)
        {
            case XamlNodeType.Value:
                AttributeValue = context.Value;
                break;
            case XamlNodeType.StartObject:
                AttributeValue = ParseElement(context);
                break;
            default:
                throw new NotImplementedException();
        }

        XamlAttributeMember Attribute = new(string.Empty, AttributeValue);
        attributes.Add(Attribute);

        context.Read();

        if (context.NodeType != XamlNodeType.EndMember)
            throw new NotImplementedException();
    }

    private static void ParseElementMemberDirective(XamlParsingContext context, XamlDirective directive, XamlAttributeCollection attributes)
    {
        if (!directive.IsNameValid)
            throw new NotImplementedException();

        IXamlNamespace AttributeNamespace = context.MemberNamespace;
        string AttributeName = context.MemberName;

        context.Read();

        if (context.NodeType != XamlNodeType.Value)
            throw new NotImplementedException();

        object? AttributeValue = context.Value;

        XamlAttributeDirective Attribute = new(AttributeNamespace, AttributeName, AttributeValue);
        attributes.Add(Attribute);

        context.Read();

        if (context.NodeType != XamlNodeType.EndMember)
            throw new NotImplementedException();
    }

    private static void ParseElementMember(XamlParsingContext context, XamlAttributeCollection attributes, out bool checkMultiLine)
    {
        checkMultiLine = false;

        if (!context.Member.IsNameValid)
            throw new NotImplementedException();

        string AttributeName = context.Member.Name;
        int AttributeLineNumber = context.LineNumber;

        string DeclaringTypeName = context.Member.DeclaringType.Name;
        if (DeclaringTypeName != context.CurrentObjectName)
        {
            string DeclaringTypeNamespace = context.Member.DeclaringType.PreferredXamlNamespace;
            IXamlNamespace DeclaringNamespace = context.FindNamespaceMatch(DeclaringTypeNamespace, context.Member.DeclaringType.UnderlyingType);
            string DeclaringTypeString = DeclaringNamespace.Prefix == string.Empty ? DeclaringTypeName : $"{DeclaringNamespace.Prefix}:{DeclaringTypeName}";
            AttributeName = DeclaringTypeString + "." + AttributeName;
        }

        IXamlAttribute Attribute;

        context.Read();

        switch (context.NodeType)
        {
            case XamlNodeType.Value:
                object? AttributeValue = ParseElementMemberSimpleValue(context);
                Attribute = new XamlAttributeMember(AttributeName, AttributeValue);
                checkMultiLine = true;
                break;
            case XamlNodeType.StartObject:
                XamlElementCollection ElementCollection = ParseElementMemberObjectsValue(context);
                bool IsOneLine = AttributeLineNumber == context.LineNumber;
                Attribute = new XamlAttributeElementCollection(AttributeName, ElementCollection, IsOneLine);
                checkMultiLine = IsOneLine;
                break;
            default:
                throw new NotImplementedException();
        }

        attributes.Add(Attribute);
    }

    private static object? ParseElementMemberSimpleValue(XamlParsingContext context)
    {
        object? AttributeValue = context.Value;

        context.Read();

        if (context.NodeType != XamlNodeType.EndMember)
            throw new NotImplementedException();

        return AttributeValue;
    }

    private static void SkipIndentation(XamlParsingContext context)
    {
        if (context.NodeType == XamlNodeType.Value && context.Value is string StringValue && StringValue.Trim() == string.Empty)
            context.Read();
    }

    private static XamlElementCollection ParseElementMemberObjectsValue(XamlParsingContext context)
    {
        XamlElementCollection Children = new();

        for (; ;)
        {
            XamlElement Child = ParseElement(context);
            Children.Add(Child);

            context.Read();

            SkipIndentation(context);

            if (context.NodeType == XamlNodeType.EndMember)
                break;

            if (context.NodeType != XamlNodeType.StartObject)
                throw new NotImplementedException();
        }

        return Children;
    }
}
