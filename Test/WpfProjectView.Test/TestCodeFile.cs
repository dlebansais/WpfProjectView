namespace WpfProjectView.Test;

using System.IO;
using System.Runtime.InteropServices;
using FolderView;
using NUnit.Framework;

public class TestCodeFile
{
    [Test]
    public void TestLoad()
    {
        ILocation Location = TestTools.GetLocalLocation();

        var TestProjectTask = Project.CreateAsync(Location);
        TestProjectTask.Wait();

        IProject TestProject = TestProjectTask.Result;
        int CodeFileCount = 0;

        foreach (var Item in TestProject.Files)
            if (Item is ICodeFile AsCodeFile)
            {
                CodeFileCount++;

                AsCodeFile.Parse();
                Assert.That(AsCodeFile.SyntaxTree, Is.Null);

                AsCodeFile.LoadAsync(TestProject.RootFolder);
                Assert.That(AsCodeFile.SyntaxTree, Is.Null);

                AsCodeFile.Parse();
                Assert.That(AsCodeFile.SyntaxTree, Is.Not.Null);
            }

        Assert.That(CodeFileCount, Is.GreaterThan(0));
    }

    [Test]
    public void TestNullContent()
    {
        string DummyFileName = "dummy.cs";
        _ = DeleteFile(DummyFileName);
        CreateLocalDummyFile(DummyFileName);

        LocalLocation Location = new(".");
        var TestProjectTask = Project.CreateAsync(Location);
        TestProjectTask.Wait();
        IProject TestProject = TestProjectTask.Result;

        _ = DeleteFile(DummyFileName);

        foreach (var Item in TestProject.Files)
            if (Item is ICodeFile AsCodeFile && AsCodeFile.SourceFile.Name == DummyFileName)
            {
                Assert.ThrowsAsync<FileNotFoundException>(async () => await AsCodeFile.LoadAsync(TestProject.RootFolder));

                AsCodeFile.Parse();
                Assert.That(AsCodeFile.SyntaxTree, Is.Null);
                break;
            }
    }

    private void CreateLocalDummyFile(string fileName)
    {
        using FileStream Stream = new(fileName, FileMode.Create, FileAccess.Write);
        using BinaryWriter Writer = new BinaryWriter(Stream);
        Writer.Write("Dummy");
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool DeleteFile(string path);
}
