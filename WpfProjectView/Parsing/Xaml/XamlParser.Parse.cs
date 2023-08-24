﻿namespace WpfProjectView;

using System;
using System.Diagnostics;
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

        Debug.Assert(context.NodeType == XamlNodeType.StartObject);

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

        Debug.Assert(context.NodeType == XamlNodeType.EndObject);
    }

    private static void ParseElementMemberInitialization(XamlParsingContext context, XamlAttributeCollection attributes)
    {
        context.Read();

        Debug.Assert(context.NodeType == XamlNodeType.Value);
        Debug.Assert(context.Value is string);

        string StringValue = (string)context.Value;

        XamlAttributeSimpleValue Attribute = new(StringValue);
        attributes.Add(Attribute);

        context.Read();

        Debug.Assert(context.NodeType == XamlNodeType.EndMember);
    }

    private static void ParseElementMemberUnknownContent(XamlParsingContext context, XamlElementCollection children, XamlAttributeCollection attributes)
    {
        context.Read();

        XamlNodeType NodeType = context.NodeType;
        bool IsParsed = false;

        if (NodeType == XamlNodeType.Value)
        {
            ParseElementMemberUnknownContentSimpleValue(context, attributes);
            IsParsed = true;
        }

        if (NodeType == XamlNodeType.StartObject)
        {
            ParseElementMemberUnknownContentObjects(context, children);
            IsParsed = true;
        }

        Debug.Assert(IsParsed);
    }

    private static void ParseElementMemberUnknownContentSimpleValue(XamlParsingContext context, XamlAttributeCollection attributes)
    {
        Debug.Assert(context.Value is string);

        string StringValue = (string)context.Value;
        XamlAttributeSimpleValue Attribute = new(StringValue);
        attributes.Add(Attribute);

        context.Read();

        Debug.Assert(context.NodeType == XamlNodeType.EndMember);
    }

    private static void ParseElementMemberUnknownContentObjects(XamlParsingContext context, XamlElementCollection children)
    {
        do
        {
            IXamlElement Child = ParseElement(context);
            children.Add(Child);

            SkipIndentation(context);
        }
        while (context.NodeType != XamlNodeType.EndMember);
    }

    private static void ParseElementMemberPositionalParameter(XamlParsingContext context, XamlAttributeCollection attributes)
    {
        context.Read();

        object? AttributeValue = null;
        XamlNodeType NodeType = context.NodeType;

        if (NodeType == XamlNodeType.Value)
            AttributeValue = context.Value;

        if (NodeType == XamlNodeType.StartObject)
            AttributeValue = ParseElement(context);

        Debug.Assert(AttributeValue is not null);

        XamlAttributeMember Attribute = new(string.Empty, AttributeValue);
        attributes.Add(Attribute);

        context.Read();

        Debug.Assert(context.NodeType == XamlNodeType.EndMember);
    }

    private static void ParseElementMemberDirective(XamlParsingContext context, XamlDirective directive, XamlAttributeCollection attributes)
    {
        Debug.Assert(directive.IsNameValid);

        IXamlNamespace AttributeNamespace = context.MemberNamespace;
        string AttributeName = context.MemberName;

        context.Read();

        XamlNodeType NodeType = context.NodeType;
        bool IsParsed = false;

        if (NodeType == XamlNodeType.Value)
        {
            ParseElementMemberDirectiveSimpleValue(context, AttributeNamespace, AttributeName, attributes);
            IsParsed = true;
        }

        if (NodeType == XamlNodeType.StartObject)
        {
            ParseElementMemberDirectiveContentObject(context, AttributeNamespace, AttributeName, attributes);
            IsParsed = true;
        }

        Debug.Assert(IsParsed);
    }

    private static void ParseElementMemberDirectiveSimpleValue(XamlParsingContext context, IXamlNamespace attributeNamespace, string attributeName, XamlAttributeCollection attributes)
    {
        object? AttributeValue = context.Value;

        XamlAttributeDirective Attribute = new(attributeNamespace, attributeName, AttributeValue);
        attributes.Add(Attribute);

        context.Read();

        Debug.Assert(context.NodeType == XamlNodeType.EndMember);
    }

    private static void ParseElementMemberDirectiveContentObject(XamlParsingContext context, IXamlNamespace attributeNamespace, string attributeName, XamlAttributeCollection attributes)
    {
        XamlElement Child = ParseElement(context);

        XamlAttributeDirective Attribute = new(attributeNamespace, attributeName, Child);
        attributes.Add(Attribute);

        SkipIndentation(context);

        Debug.Assert(context.NodeType == XamlNodeType.EndMember);
    }

    private static void ParseElementMember(XamlParsingContext context, XamlAttributeCollection attributes, out bool checkMultiLine)
    {
        checkMultiLine = false;

        Debug.Assert(context.Member.IsNameValid);

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

        context.Read();

        IXamlAttribute? Attribute = null;
        XamlNodeType NodeType = context.NodeType;

        if (NodeType is XamlNodeType.Value)
        {
            object? AttributeValue = ParseElementMemberSimpleValue(context);
            Attribute = new XamlAttributeMember(AttributeName, AttributeValue);
            checkMultiLine = true;
        }

        if (NodeType is XamlNodeType.StartObject)
        {
            XamlElementCollection ElementCollection = ParseElementMemberObjectsValue(context);
            bool IsOneLine = AttributeLineNumber == context.LineNumber;
            Attribute = new XamlAttributeElementCollection(AttributeName, ElementCollection, IsOneLine);
            checkMultiLine = IsOneLine;
        }

        Debug.Assert(Attribute is not null);

        attributes.Add(Attribute);
    }

    private static object? ParseElementMemberSimpleValue(XamlParsingContext context)
    {
        object? AttributeValue = context.Value;

        context.Read();

        Debug.Assert(context.NodeType == XamlNodeType.EndMember);

        return AttributeValue;
    }

    private static XamlElementCollection ParseElementMemberObjectsValue(XamlParsingContext context)
    {
        XamlElementCollection Children = new();

        for (; ;)
        {
            XamlElement Child = ParseElement(context);
            Children.Add(Child);

            SkipIndentation(context);

            if (context.NodeType == XamlNodeType.EndMember)
                break;

            Debug.Assert(context.NodeType == XamlNodeType.StartObject);
        }

        return Children;
    }

    private static void SkipIndentation(XamlParsingContext context)
    {
        context.Read();

        if (context.NodeType == XamlNodeType.Value && context.Value is string StringValue && StringValue.Trim() == string.Empty)
            context.Read();
    }
}
