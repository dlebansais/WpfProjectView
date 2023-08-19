namespace WpfProjectView;

using System;
using System.IO;
using System.Xaml;

/// <summary>
/// Implements a xaml parser.
/// </summary>
internal static partial class XamlParser
{
    /// <summary>
    /// Parses xaml content and returns the node tree.
    /// </summary>
    /// <param name="content">The content to parse.</param>
    public static IXamlParsingResult Parse(byte[]? content)
    {
        XamlParsingResult Result = new();

        if (content is not null)
        {
            using MemoryStream Stream = new(content);
            using XamlXmlReader Reader = new(Stream);

            if (!Reader.Read())
                throw new NotImplementedException();

            XamlParsingContext Context = new(Reader, new XamlNamespaceCollection());

            Result.Root = ParseElement(Context);
            Print(Result.Root, 0);
        }

        return Result;
    }

    private static int Rounds = 0;

    private static XamlElement ParseElement(XamlParsingContext context)
    {
        Rounds++;

        string Name;
        XamlNamespaceCollection Namespaces = new();
        XamlElementCollection Children = new();
        XamlAttributeCollection Attributes = new();

        while (context.NodeType == XamlNodeType.NamespaceDeclaration)
        {
            NamespaceDeclaration Namespace = context.Namespace;
            Namespaces.Add(new XamlNamespace(Namespace.Prefix, Namespace.Namespace));

            if (!context.Read())
                throw new NotImplementedException();
        }

        XamlNamespaceCollection NewNamespaces = new(context.Namespaces);
        NewNamespaces.AddRange(Namespaces);
        context = context with { Namespaces = NewNamespaces };

        if (context.NodeType != XamlNodeType.StartObject)
            throw new InvalidXamlFormatException("Start object tag expected");

        Name = context.Type.Name;
        ParseElementContent(context, Children, Attributes);

        return new XamlElement(Name, Namespaces, Children, Attributes);
    }

    private static void ParseElementContent(XamlParsingContext context, XamlElementCollection children, XamlAttributeCollection attributes)
    {
        Rounds++;

        while (context.Read() && context.NodeType == XamlNodeType.StartMember)
        {
            if (context.Member == XamlLanguage.Initialization)
                ParseElementMemberInitialization(context, attributes);
            else if (context.Member == XamlLanguage.UnknownContent)
                ParseElementMemberUnknownContent(context, children, attributes);
            else if (context.Member == XamlLanguage.PositionalParameters)
                ParseElementMemberPositionalParameter(context, attributes);
            else if (context.Member is XamlDirective AsDirective)
                ParseElementMemberDirective(context, AsDirective, attributes);
            else
                ParseElementMember(context, attributes);
        }

        if (context.NodeType != XamlNodeType.EndObject)
            throw new NotImplementedException();
    }

    private static void ParseElementMemberInitialization(XamlParsingContext context, XamlAttributeCollection attributes)
    {
        if (!context.Read())
            throw new NotImplementedException();

        if (context.NodeType != XamlNodeType.Value)
            throw new NotImplementedException();

        if (context.Value is not string StringValue)
            throw new NotImplementedException();

        XamlAttributeMember Attribute = new(string.Empty, StringValue);
        attributes.Add(Attribute);

        if (!context.Read())
            throw new NotImplementedException();

        if (context.NodeType != XamlNodeType.EndMember)
            throw new NotImplementedException();
    }

    private static void ParseElementMemberUnknownContent(XamlParsingContext context, XamlElementCollection children, XamlAttributeCollection attributes)
    {
        Rounds++;

        if (!context.Read())
            throw new NotImplementedException();

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
        Rounds++;

        for (; ;)
        {
            IXamlElement Child = ParseElement(context);
            children.Add(Child);

            if (Rounds == 134)
            {
            }

            if (context.NodeType != XamlNodeType.EndObject)
                throw new NotImplementedException();

            if (!context.Read())
                throw new NotImplementedException();

            SkipIndentation(context);

            if (context.NodeType == XamlNodeType.EndMember)
                break;
        }
    }

    private static void ParseElementMemberUnknownContentSimpleValue(XamlParsingContext context, XamlAttributeCollection attributes)
    {
        Rounds++;

        if (context.Value is string StringValue)
        {
            XamlAttributeMember Attribute = new(string.Empty, StringValue);
            attributes.Add(Attribute);

            if (!context.Read())
                throw new NotImplementedException();

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
        Rounds++;

        if (!context.Read())
            throw new NotImplementedException();

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

        if (!context.Read())
            throw new NotImplementedException();

        if (context.NodeType != XamlNodeType.EndMember)
            throw new NotImplementedException();
    }

    private static void ParseElementMemberDirective(XamlParsingContext context, XamlDirective directive, XamlAttributeCollection attributes)
    {
        Rounds++;

        if (!directive.IsNameValid)
            throw new NotImplementedException();

        string AttributePrefix = string.Empty;
        foreach (IXamlNamespace Namespace in context.Namespaces)
            if (Namespace.AssemblyPath == directive.PreferredXamlNamespace)
            {
                AttributePrefix = $"{Namespace.Prefix}:";
                break;
            }

        string AttributeName = directive.Name;

        if (!context.Read())
            throw new NotImplementedException();

        if (context.NodeType != XamlNodeType.Value)
            throw new NotImplementedException();

        object? AttributeValue = context.Value;

        XamlAttributeDirective Attribute = new(AttributePrefix, AttributeName, AttributeValue);
        attributes.Add(Attribute);

        if (!context.Read())
            throw new NotImplementedException();

        if (context.NodeType != XamlNodeType.EndMember)
            throw new NotImplementedException();
    }

    private static void ParseElementMember(XamlParsingContext context, XamlAttributeCollection attributes)
    {
        Rounds++;

        if (!context.Member.IsNameValid)
            throw new NotImplementedException();

        string AttributeName = context.Member.Name;
        XamlAttribute Attribute;

        if (!context.Read())
            throw new NotImplementedException();

        switch (context.NodeType)
        {
            case XamlNodeType.Value:
                object? AttributeValue = ParseElementMemberSimpleValue(context);
                Attribute = new XamlAttributeMember(AttributeName, AttributeValue);
                break;
            case XamlNodeType.StartObject:
                XamlElementCollection ElementCollection = ParseElementMemberObjectsValue(context);
                Attribute = new XamlAttributeElementCollection(AttributeName, ElementCollection);
                break;
            default:
                throw new NotImplementedException();
        }

        attributes.Add(Attribute);
    }

    private static object? ParseElementMemberSimpleValue(XamlParsingContext context)
    {
        Rounds++;

        object? AttributeValue = context.Value;

        if (!context.Read())
            throw new NotImplementedException();

        if (context.NodeType != XamlNodeType.EndMember)
            throw new NotImplementedException();

        return AttributeValue;
    }

    private static void SkipIndentation(XamlParsingContext context)
    {
        if (context.NodeType == XamlNodeType.Value && context.Value is string StringValue && StringValue.Trim() == string.Empty)
        {
            if (!context.Read())
                throw new NotImplementedException();
        }
    }

    private static XamlElementCollection ParseElementMemberObjectsValue(XamlParsingContext context)
    {
        Rounds++;

        XamlElementCollection Children = new();

        for (; ;)
        {
            XamlElement Child = ParseElement(context);
            Children.Add(Child);

            if (!context.Read())
                throw new NotImplementedException();

            SkipIndentation(context);

            if (context.NodeType == XamlNodeType.EndMember)
                break;

            if (context.NodeType != XamlNodeType.StartObject)
                throw new NotImplementedException();
        }

        return Children;
    }
}
