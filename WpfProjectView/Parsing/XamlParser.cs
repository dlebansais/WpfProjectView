namespace WpfProjectView;

using System;
using System.Diagnostics;
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
    public static IXamlElement? Parse(byte[]? content)
    {
        if (content is null)
            return null;

        MemoryStream Stream = new(content);
        XamlXmlReader Reader = new(Stream);

        if (!Reader.Read())
            throw new NotImplementedException();

        return ParseElement(Reader);
    }

    private static int Rounds = 0;

    private static IXamlElement ParseElement(XamlXmlReader reader)
    {
        Rounds++;

        XamlNamespaceCollection Namespaces = new();
        XamlElementCollection Children = new();
        XamlAttributeCollection Attributes = new();
        ParseElementWithNamespaces(reader, out string Name, Namespaces, Children, Attributes);

        return new XamlElement(Name, Namespaces, Children, Attributes);
    }

    private static void ParseElementWithNamespaces(XamlXmlReader reader, out string name, XamlNamespaceCollection namespaces, XamlElementCollection children, XamlAttributeCollection attributes)
    {
        Rounds++;

        while (reader.NodeType == XamlNodeType.NamespaceDeclaration)
        {
            NamespaceDeclaration Namespace = reader.Namespace;
            namespaces.Add(new XamlNamespace(Namespace.Prefix, Namespace.Namespace));

            if (!reader.Read())
                throw new NotImplementedException();
        }

        ParseElement(reader, out name, children, attributes);
    }

    private static void ParseElement(XamlXmlReader reader, out string name, XamlElementCollection children, XamlAttributeCollection attributes)
    {
        Rounds++;

        if (reader.NodeType != XamlNodeType.StartObject)
            throw new NotImplementedException();

        name = reader.Type.Name;
        ParseElementContent(reader, children, attributes);
    }

    private static void ParseElementContent(XamlXmlReader reader, XamlElementCollection children, XamlAttributeCollection attributes)
    {
        Rounds++;

        while (reader.Read() && reader.NodeType == XamlNodeType.StartMember)
        {
            if (reader.Member == XamlLanguage.Initialization)
                ParseElementMemberInitialization(reader);
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

    private static void ParseElementMemberInitialization(XamlXmlReader reader)
    {
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
                XamlElementCollection Children = new();
                XamlAttributeCollection Attributes = new();
                ParseElement(reader, out string Name, Children, Attributes);
                AttributeValue = new XamlElement(Name, new XamlNamespaceCollection(), Children, Attributes);
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
            XamlElementCollection Children = new();
            XamlAttributeCollection Attributes = new();
            ParseElement(reader, out string Name, Children, Attributes);
            object? AttributeValue = new XamlElement(Name, new XamlNamespaceCollection(), Children, Attributes);

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
