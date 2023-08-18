namespace WpfProjectView.Test;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
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

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool DeleteFile(string path);
}
