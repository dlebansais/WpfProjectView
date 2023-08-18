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
    public static object? Parse(byte[]? content)
    {
        if (content is null)
            return null;

        MemoryStream Stream = new(content);
        XamlXmlReader Reader = new(Stream);
        string? RootItemName = null;

        while (Reader.Read())
        {
            if (Reader.NodeType == XamlNodeType.StartObject)
            {
                RootItemName = Reader.Type.Name;
                break;
            }
        }

        return RootItemName;
    }
}
