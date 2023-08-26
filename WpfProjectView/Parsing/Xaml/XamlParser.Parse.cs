namespace WpfProjectView;

using System.ComponentModel;
using System.Diagnostics;
using System.Xaml;

/// <summary>
/// Implements a xaml parser.
/// </summary>
public static partial class XamlParser
{
    private static XamlElement ParseElement(XamlParsingContext context)
    {
        XamlNamespaceCollection Namespaces = ParseNamespaceDeclarations(context);

        if (Namespaces.Count > 0)
        {
            XamlNamespaceCollection NewNamespaces = new(context.Namespaces);
            NewNamespaces.AddRange(Namespaces);
            context = context with { Namespaces = NewNamespaces };
        }

        IXamlNamespace ElementNamespace = context.ObjectNamespace;
        string ElementName = context.ObjectName;

        context = context with { CurrentObjectName = ElementName };

        context.Read();
        Debug.Assert(context.NodeType == XamlNodeType.StartMember || context.NodeType == XamlNodeType.EndObject);

        (XamlElementCollection Children, XamlAttributeCollection Attributes, bool IsMultiLine) = ParseElementContent(context);

        return new XamlElement(Namespaces, ElementNamespace, ElementName, Children, Attributes, IsMultiLine);
    }

    private static XamlNamespaceCollection ParseNamespaceDeclarations(XamlParsingContext context)
    {
        XamlNamespaceCollection Namespaces = new();

        while (context.NodeType == XamlNodeType.NamespaceDeclaration)
        {
            NamespaceDeclaration Namespace = context.NamespacePath;
            IXamlNamespace NewNamespace = XamlNamespace.Create(Namespace.Prefix, Namespace.Namespace);
            Namespaces.Add(NewNamespace);

            context.Read();
        }

        Debug.Assert(context.NodeType == XamlNodeType.StartObject);

        return Namespaces;
    }

    private static (XamlElementCollection Children, XamlAttributeCollection Attributes, bool IsMultiLine) ParseElementContent(XamlParsingContext context)
    {
        XamlElementCollection Children = new();
        XamlAttributeCollection Attributes = new();
        bool IsMultiLine = false;

        int StartingLineNumber = context.LineNumber;

        while (context.NodeType == XamlNodeType.StartMember)
        {
            if (context.Member == XamlLanguage.Initialization)
                ParseElementMemberInitialization(context, Attributes);
            else if (context.Member == XamlLanguage.UnknownContent)
                ParseElementMemberUnknownContent(context, Children, Attributes);
            else if (context.Member == XamlLanguage.PositionalParameters)
                ParseElementMemberPositionalParameter(context, Attributes);
            else if (context.Member == XamlLanguage.Items)
                ParseElementMemberUnknownContent(context, Children, Attributes);
            else if (context.Member is XamlDirective AsDirective)
                ParseElementMemberDirective(context, AsDirective, Attributes);
            else
                ParseElementMember(context, Attributes, StartingLineNumber, ref IsMultiLine);

            context.Read();
        }

        Debug.Assert(context.NodeType == XamlNodeType.EndObject);

        return (Children, Attributes, IsMultiLine);
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

        if (NodeType == XamlNodeType.StartObject || NodeType == XamlNodeType.NamespaceDeclaration)
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
        while (context.NodeType == XamlNodeType.StartObject || context.NodeType == XamlNodeType.NamespaceDeclaration);
    }

    private static void ParseElementMemberPositionalParameter(XamlParsingContext context, XamlAttributeCollection attributes)
    {
        context.Read();

        object? AttributeValue = null;
        XamlNodeType NodeType = context.NodeType;

        if (NodeType == XamlNodeType.Value)
            AttributeValue = context.Value;

        if (NodeType == XamlNodeType.StartObject || NodeType == XamlNodeType.NamespaceDeclaration)
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

        if (NodeType == XamlNodeType.StartObject || NodeType == XamlNodeType.NamespaceDeclaration)
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

        XamlAttributeDirective AttributeDirective = new(attributeNamespace, attributeName, Child);
        attributes.Add(AttributeDirective);

        SkipIndentation(context);

        Debug.Assert(context.NodeType == XamlNodeType.EndMember);
    }

    private static void ParseElementMember(XamlParsingContext context, XamlAttributeCollection attributes, int startingLineNumber, ref bool isMultiLine)
    {
        bool CheckMultiLine = false;

        Debug.Assert(context.Member.IsNameValid);

        string AttributeName = context.Member.Name;
        int AttributeLineNumber = context.LineNumber;
        bool IsVisible = context.Member.SerializationVisibility == DesignerSerializationVisibility.Visible;

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

        if (NodeType == XamlNodeType.Value)
        {
            object? AttributeValue = ParseElementMemberSimpleValue(context);
            Attribute = new XamlAttributeMember(AttributeName, AttributeValue);
            CheckMultiLine = true;
        }

        if (NodeType == XamlNodeType.StartObject || NodeType == XamlNodeType.NamespaceDeclaration)
        {
            XamlElementCollection ElementCollection = ParseElementMemberObjectsValue(context);
            bool IsOneLine = AttributeLineNumber == context.LineNumber;
            Attribute = new XamlAttributeElementCollection(AttributeName, ElementCollection, IsVisible, IsOneLine);
            CheckMultiLine = IsOneLine;
        }

        if (NodeType == XamlNodeType.GetObject)
        {
            XamlElementCollection ElementCollection = ParseImplicitElementMemberObjectsValue(context);
            bool IsOneLine = AttributeLineNumber == context.LineNumber;
            Attribute = new XamlAttributeElementCollection(AttributeName, ElementCollection, IsVisible, IsOneLine);
            CheckMultiLine = IsOneLine;
        }

        Debug.Assert(Attribute is not null);

        attributes.Add(Attribute);

        if (CheckMultiLine && context.LineNumber > startingLineNumber && attributes.Count > 1)
            isMultiLine = true;
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

        do
        {
            XamlElement Child = ParseElement(context);
            Children.Add(Child);

            SkipIndentation(context);
        }
        while (context.NodeType == XamlNodeType.StartObject || context.NodeType == XamlNodeType.NamespaceDeclaration);

        return Children;
    }

    private static XamlElementCollection ParseImplicitElementMemberObjectsValue(XamlParsingContext context)
    {
        context.Read();
        Debug.Assert(context.NodeType == XamlNodeType.StartMember);

        (XamlElementCollection Children, _, _) = ParseElementContent(context);

        context.Read();
        Debug.Assert(context.NodeType == XamlNodeType.EndMember);

        return Children;
    }

    private static void SkipIndentation(XamlParsingContext context)
    {
        context.Read();

        if (context.NodeType == XamlNodeType.Value && context.Value is string StringValue && StringValue.Trim() == string.Empty)
            context.Read();
    }
}
