namespace WpfProjectView.Test;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using FolderView;

public static class TestTools
{
    public const string ProjectRepositoryName = "PgCompletionist";
    public const string ProjectName = "PgCompletionist";
    public const string InvalidProjectRepositoryName = "InvalidSolution";
    public const string InvalidProjectName = "InvalidSolution";
    public const string SolutionSameFolderRepositoryName = "SolutionSameFolder";

    public static (ILocation, IPath) GetLocalLocationAndPathToProject()
    {
        return GetLocalLocationAndPath(ProjectRepositoryName, ProjectName);
    }

    public static (ILocation, IPath) GetLocalLocationAndPath(string repositoryName, string projectName)
    {
        string ProjectRootPath = GetExecutingProjectRootPath();
        string TestProjectRootPath = @$"{ProjectRootPath}\..\{repositoryName}";
        LocalLocation Location = new(TestProjectRootPath);

        return (Location, new FolderView.Path(new List<string>(), projectName));
    }

    private static string GetExecutingProjectRootPath()
    {
        string? CurrentDirectory = Environment.CurrentDirectory;
        bool Continue = true;

        while (Continue)
        {
            string? ParentFolder = System.IO.Path.GetDirectoryName(CurrentDirectory);
            string? FileName = System.IO.Path.GetFileName(CurrentDirectory);

            List<string?> KnownFolderNames = new()
            {
                "net481",
                "net7.0",
                "net7.0-windows7.0",
                "net8.0",
                "net8.0-windows7.0",
                "Debug",
                "Release",
                "x64",
                "bin",
            };

            if (KnownFolderNames.Contains(FileName))
                CurrentDirectory = ParentFolder;
            else
                Continue = false;
        }

        Debug.Assert(CurrentDirectory is not null);

        return CurrentDirectory!;
    }

    public static string CompareXamlParingResultWithOriginalContent(Stream content, IXamlParsingResult xamlParsingResult)
    {
        Debug.Assert(content is not null);

        _ = content!.Seek(0, SeekOrigin.Begin);
        using StreamReader Reader = new(content, Encoding.UTF8);
        string ContentString = Reader.ReadToEnd();
        ContentString = ContentString.Trim();

        StringBuilder Builder = XamlParser.Print(xamlParsingResult);
        string PrintResultString =  Builder.ToString();
        PrintResultString = PrintResultString.Trim();

        if (ContentString == PrintResultString)
            return string.Empty;

        return PrintParsingDifferences(ContentString, PrintResultString);
    }

    public static string PrintParsingDifferences(string contentString, string printResultString)
    {
        StringBuilder Builder = new();
        var Differences = InlineDiffBuilder.Diff(contentString, printResultString, ignoreWhiteSpace: false);
        bool HasDifferences = false;
        int SimilarityCount = 0;
        int LineNumber = 1;
        Collection<string> DebugLines = new();

        foreach (var Line in Differences.Lines)
        {
            switch (Line.Type)
            {
                case ChangeType.Inserted:
                    DebugLines.Add($"+ {Line.Text}");
                    HasDifferences = true;
                    SimilarityCount = 0;
                    break;
                case ChangeType.Deleted:
                    DebugLines.Add($"- {Line.Text}");
                    HasDifferences = true;
                    SimilarityCount = 0;
                    break;
                default:
                    DebugLines.Add($"  {Line.Text}");
                    SimilarityCount++;

                    if (!HasDifferences)
                    {
                        if (DebugLines.Count >= 3)
                            DebugLines.RemoveAt(0);
                    }
                    else if (SimilarityCount >= 3)
                    {
                        PrintParsingDifferences(Builder, LineNumber, DebugLines);

                        DebugLines.Clear();
                        HasDifferences = false;
                        SimilarityCount = 0;
                    }

                    break;
            }

            LineNumber++;
        }

        if (HasDifferences)
            PrintParsingDifferences(Builder, LineNumber, DebugLines);

        return Builder.ToString();
    }

    private static void PrintParsingDifferences(StringBuilder builder, int lineNumber, Collection<string> debugLines)
    {
#if NET6_0_OR_GREATER
        _ = builder.AppendLine(CultureInfo.InvariantCulture, $"****** Line {lineNumber - debugLines.Count + 1}");
#else
        _ = builder.AppendLine($"****** Line {lineNumber - debugLines.Count + 1}");
#endif

        foreach (string DebugLine in debugLines)
            _ = builder.AppendLine(DebugLine);
    }

    public static Stream GetResourceContent(string resourceName)
    {
        Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
        Stream ResourceStream = ExecutingAssembly.GetManifestResourceStream($"WpfProjectView.Test.Resources.{resourceName}")!;

        return ResourceStream;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern bool DeleteFile(string path);

    public static void CreateLocalDummyFile(string fileName)
    {
        using FileStream Stream = new(fileName, FileMode.Create, FileAccess.Write);
        using BinaryWriter Writer = new(Stream);
        Writer.Write("Dummy");
    }
}
