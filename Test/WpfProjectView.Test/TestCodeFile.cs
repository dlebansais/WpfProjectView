namespace WpfProjectView.Test;

using System.IO;
using System.Threading.Tasks;
using FolderView;
using NUnit.Framework;

public class TestCodeFile
{
    [Test]
    public async Task TestLoadAsync()
    {
        ILocation Location = TestTools.GetLocalLocation();

        IProject TestProject = await Project.CreateAsync(Location);
        int CodeFileCount = 0;

        foreach (var Item in TestProject.Files)
            if (Item is ICodeFile AsCodeFile)
            {
                CodeFileCount++;

                AsCodeFile.Parse();
                Assert.That(AsCodeFile.SyntaxTree, Is.Null);

                await AsCodeFile.LoadAsync(TestProject.RootFolder);
                Assert.That(AsCodeFile.SyntaxTree, Is.Null);

                AsCodeFile.Parse();
                Assert.That(AsCodeFile.SyntaxTree, Is.Not.Null);
            }

        Assert.That(CodeFileCount, Is.GreaterThan(0));
    }

    [Test]
    public async Task TestNullContentAsync()
    {
        string DummyFileName = "dummy.cs";
        _ = TestTools.DeleteFile(DummyFileName);
        CreateLocalDummyFile(DummyFileName);

        LocalLocation Location = new(".");
        IProject TestProject = await Project.CreateAsync(Location);
        
        _ = TestTools.DeleteFile(DummyFileName);

        Assert.That(TestProject.Files, Has.Count.GreaterThan(0));
        ICodeFile? CodeFile = TestProject.Files[0] as ICodeFile;
        Assert.That(CodeFile, Is.Not.Null);
        Assert.That(CodeFile.SourceFile.Name, Is.EqualTo(DummyFileName));

        Assert.ThrowsAsync<FileNotFoundException>(async () => await CodeFile.LoadAsync(TestProject.RootFolder));

        CodeFile.Parse();
        Assert.That(CodeFile.SyntaxTree, Is.Null);
    }

    private void CreateLocalDummyFile(string fileName)
    {
        using FileStream Stream = new(fileName, FileMode.Create, FileAccess.Write);
        using BinaryWriter Writer = new BinaryWriter(Stream);
        Writer.Write("Dummy");
    }
}
