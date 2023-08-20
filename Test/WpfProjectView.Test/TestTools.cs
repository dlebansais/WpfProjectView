namespace WpfProjectView.Test;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using FolderView;

public static class TestTools
{
    private const string ProjectRepositoryName = "PgCompletionist";
    private const string ProjectName = "PgCompletionist";

    public static ILocation GetLocalLocation()
    {
        string ProjectRootPath = TestTools.GetExecutingProjectRootPath();
        string TestProjectRootPath = @$"{ProjectRootPath}\..\..\..\{ProjectRepositoryName}\{ProjectName}";
        LocalLocation Location = new(TestProjectRootPath);

        return Location;
    }

    private static string GetExecutingProjectRootPath()
    {
        string? CurrentDirectory = Environment.CurrentDirectory;
        bool Continue = true;

        while (Continue)
        {
            string? ParentFolder = System.IO.Path.GetDirectoryName(CurrentDirectory);
            string? FileName = System.IO.Path.GetFileName(CurrentDirectory);

            switch (FileName)
            {
                case "net7.0":
                case "net7.0-windows7.0":
                case "Debug":
                case "Release":
                case "x64":
                case "bin":
                    CurrentDirectory = ParentFolder;
                    continue;
                default:
                    Continue = false;
                    break;
            }
        }

        Debug.Assert(CurrentDirectory is not null);

        return CurrentDirectory!;
    }

    public static void CompareXamlParingResultWithOriginalContent(byte[] content, IXamlParsingResult xamlParsingResult)
    {
        bool HasUf8Bom = content.Length >= 3 && content[0] == 0xEF && content[1] == 0xBB && content[2] == 0xBF;
        string ContentString = HasUf8Bom ? Encoding.UTF8.GetString(content, 3, content.Length - 3) : Encoding.UTF8.GetString(content);
        ContentString = ContentString.Trim();

        StringBuilder Builder = XamlParser.Print(xamlParsingResult);
        string PrintResultString =  Builder.ToString();
        PrintResultString = PrintResultString.Trim();

        if (ContentString == PrintResultString)
            return;

        var Differences = InlineDiffBuilder.Diff(ContentString, PrintResultString, ignoreWhiteSpace: false);

        var SavedColor = Console.ForegroundColor;
        foreach (var Line in Differences.Lines)
        {
            switch (Line.Type)
            {
                case ChangeType.Inserted:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Debug.Write("+ ");
                    break;
                case ChangeType.Deleted:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Debug.Write("- ");
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Gray; // compromise for dark or light background
                    Debug.Write("  ");
                    break;
            }

            Debug.WriteLine(Line.Text);
        }

        Console.ForegroundColor = SavedColor;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool DeleteFile(string path);
}
