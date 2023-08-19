namespace WpfProjectView;

using System;
using System.IO;
using System.Xaml;

/// <summary>
/// Implements a xaml parser.
/// </summary>
internal static class XamlParser
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

            Result.Root = ParseElement(Reader);
        }

        return Result;
    }

    private static int Rounds = 0;

    private static XamlElement ParseElement(XamlXmlReader reader)
    {
        Rounds++;

        XamlNamespaceCollection Namespaces = new();

        while (reader.NodeType == XamlNodeType.NamespaceDeclaration)
        {
            NamespaceDeclaration Namespace = reader.Namespace;
            Namespaces.Add(new XamlNamespace(Namespace.Prefix, Namespace.Namespace));

            if (!reader.Read())
                throw new NotImplementedException();
        }

        XamlElementCollection Children = new();
        XamlAttributeCollection Attributes = new();
        string Name;

        if (reader.NodeType != XamlNodeType.StartObject)
            throw new NotImplementedException();

        Name = reader.Type.Name;
        ParseElementContent(reader, Children, Attributes);

        return new XamlElement(Name, Namespaces, Children, Attributes);
    }

    private static void ParseElementContent(XamlXmlReader reader, XamlElementCollection children, XamlAttributeCollection attributes)
    {
        Rounds++;

        while (reader.Read() && reader.NodeType == XamlNodeType.StartMember)
        {
            if (reader.Member == XamlLanguage.Initialization)
                ParseElementMemberInitialization(reader, attributes);
            else if (reader.Member == XamlLanguage.UnknownContent)
                ParseElementMemberUnknownContent(reader, children, attributes);
            else if (reader.Member == XamlLanguage.PositionalParameters)
                ParseElementMemberPositionalParameter(reader, attributes);
            else if (reader.Member is XamlDirective AsDirective)
                ParseElementMemberDirective(reader, AsDirective, attributes);
            else
                ParseElementMember(reader, reader.Member, attributes);
        }

        if (reader.NodeType != XamlNodeType.EndObject)
            throw new NotImplementedException();
    }

    private static void ParseElementMemberInitialization(XamlXmlReader reader, XamlAttributeCollection attributes)
    {
        if (!reader.Read())
            throw new NotImplementedException();

        if (reader.NodeType != XamlNodeType.Value)
            throw new NotImplementedException();

        if (reader.Value is not string StringValue)
            throw new NotImplementedException();

        XamlAttribute Attribute = new(string.Empty, StringValue);
        attributes.Add(Attribute);

        if (!reader.Read())
            throw new NotImplementedException();

        if (reader.NodeType != XamlNodeType.EndMember)
            throw new NotImplementedException();
    }

    private static void ParseElementMemberUnknownContent(XamlXmlReader reader, XamlElementCollection children, XamlAttributeCollection attributes)
    {
        Rounds++;

        if (!reader.Read())
            throw new NotImplementedException();

        switch (reader.NodeType)
        {
            case XamlNodeType.Value:
                ParseElementMemberUnknownContentSimpleValue(reader, attributes);
                break;
            case XamlNodeType.StartObject:
                ParseElementMemberUnknownContentObjects(reader, children);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private static void ParseElementMemberUnknownContentObjects(XamlXmlReader reader, XamlElementCollection children)
    {
        Rounds++;

        for (; ;)
        {
            IXamlElement Child = ParseElement(reader);
            children.Add(Child);

            if (Rounds == 134)
            {
            }

            if (reader.NodeType != XamlNodeType.EndObject)
                throw new NotImplementedException();

            if (!reader.Read())
                throw new NotImplementedException();

            SkipIndentation(reader);

            if (reader.NodeType == XamlNodeType.EndMember)
                break;
        }
    }

    private static void ParseElementMemberUnknownContentSimpleValue(XamlXmlReader reader, XamlAttributeCollection attributes)
    {
        Rounds++;

        if (reader.Value is string StringValue)
        {
            XamlAttribute Attribute = new(string.Empty, StringValue);
            attributes.Add(Attribute);

            if (!reader.Read())
                throw new NotImplementedException();

            if (reader.NodeType != XamlNodeType.EndMember)
                throw new NotImplementedException();

            return;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static void ParseElementMemberPositionalParameter(XamlXmlReader reader, XamlAttributeCollection attributes)
    {
        Rounds++;

        if (!reader.Read())
            throw new NotImplementedException();

        object? AttributeValue;

        switch (reader.NodeType)
        {
            case XamlNodeType.Value:
                AttributeValue = reader.Value;
                break;
            case XamlNodeType.StartObject:
                AttributeValue = ParseElement(reader);
                break;
            default:
                throw new NotImplementedException();
        }

        XamlAttribute Attribute = new(string.Empty, AttributeValue);
        attributes.Add(Attribute);

        if (!reader.Read())
            throw new NotImplementedException();

        if (reader.NodeType != XamlNodeType.EndMember)
            throw new NotImplementedException();
    }

    private static void ParseElementMemberDirective(XamlXmlReader reader, XamlDirective directive, XamlAttributeCollection attributes)
    {
        Rounds++;

        if (!directive.IsNameValid)
            throw new NotImplementedException();

        string AttributeName = directive.Name;

        if (!reader.Read())
            throw new NotImplementedException();

        if (reader.NodeType != XamlNodeType.Value)
            throw new NotImplementedException();

        object? AttributeValue = reader.Value;

        XamlAttribute Attribute = new(AttributeName, AttributeValue);
        attributes.Add(Attribute);

        if (!reader.Read())
            throw new NotImplementedException();

        if (reader.NodeType != XamlNodeType.EndMember)
            throw new NotImplementedException();
    }

    private static void ParseElementMember(XamlXmlReader reader, XamlMember member, XamlAttributeCollection attributes)
    {
        Rounds++;

        if (!member.IsNameValid)
            throw new NotImplementedException();

        string AttributeName = member.Name;

        if (!reader.Read())
            throw new NotImplementedException();

        switch (reader.NodeType)
        {
            case XamlNodeType.Value:
                ParseElementMemberSimpleValue(reader, AttributeName, attributes);
                break;
            case XamlNodeType.StartObject:
                ParseElementMemberObjectsValue(reader, AttributeName, attributes);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private static void ParseElementMemberSimpleValue(XamlXmlReader reader, string attributeName, XamlAttributeCollection attributes)
    {
        Rounds++;

        object? AttributeValue = reader.Value;

        XamlAttribute Attribute = new(attributeName, AttributeValue);
        attributes.Add(Attribute);

        if (!reader.Read())
            throw new NotImplementedException();

        if (reader.NodeType != XamlNodeType.EndMember)
            throw new NotImplementedException();
    }

    private static void SkipIndentation(XamlXmlReader reader)
    {
        if (reader.NodeType == XamlNodeType.Value && reader.Value is string StringValue && StringValue.Trim() == string.Empty)
        {
            if (!reader.Read())
                throw new NotImplementedException();
        }
    }

    private static void ParseElementMemberObjectsValue(XamlXmlReader reader, string attributeName, XamlAttributeCollection attributes)
    {
        Rounds++;

        for (; ;)
        {
            object? AttributeValue = ParseElement(reader);

            XamlAttribute Attribute = new(attributeName, AttributeValue);
            attributes.Add(Attribute);

            if (!reader.Read())
                throw new NotImplementedException();

            SkipIndentation(reader);

            if (reader.NodeType == XamlNodeType.EndMember)
                break;

            if (reader.NodeType != XamlNodeType.StartObject)
                throw new NotImplementedException();
        }
    }
}
