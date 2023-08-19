namespace WpfProjectView;

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
        IXamlElement? Result = null;

        while (Reader.Read())
        {
            switch (Reader.NodeType)
            {
                case XamlNodeType.None:
                    break;
                case XamlNodeType.StartObject:
                    break;
                case XamlNodeType.GetObject:
                    break;
                case XamlNodeType.EndObject:
                    break;
                case XamlNodeType.StartMember:
                    break;
                case XamlNodeType.EndMember:
                    break;
                case XamlNodeType.Value:
                    break;
                case XamlNodeType.NamespaceDeclaration:
                    break;
            }
        }

        return Result;
    }
}
