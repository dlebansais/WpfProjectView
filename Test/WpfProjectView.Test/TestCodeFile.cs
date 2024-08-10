namespace WpfProjectView.Test;

using System.IO;
using System.Threading.Tasks;
using FolderView;
using NUnit.Framework;

[TestFixture, Order(1)]
public class TestCodeFile
{
    [Test]
    public async Task TestLoadCodeAsync()
    {
        (ILocation Location, IPath PathToProject) = TestTools.GetLocalLocationAndPathToProject();

        IProject TestProject = await Project.CreateAsync(Location, PathToProject).ConfigureAwait(false);
        int CodeFileCount = 0;

        foreach (var Item in TestProject.Files)
            if (Item is ICodeFile AsCodeFile)
            {
                CodeFileCount++;

                Assert.That(AsCodeFile.Path, Is.Not.Null);

                AsCodeFile.Parse();
                Assert.That(AsCodeFile.SyntaxTree, Is.Null);

                await AsCodeFile.LoadAsync(TestProject.RootFolder).ConfigureAwait(false);
                Assert.That(AsCodeFile.SyntaxTree, Is.Null);

                AsCodeFile.Parse();
                Assert.That(AsCodeFile.SyntaxTree, Is.Not.Null);
            }

        Assert.That(CodeFileCount, Is.GreaterThan(0));
    }

    [Test]
    public async Task TestNullContentAsync()
    {
        const string DummyFileName = "dummy.cs";
        _ = TestTools.DeleteFile(DummyFileName);
        CreateLocalDummyFile(DummyFileName);

        LocalLocation Location = new(".");
        IProject TestProject = await Project.CreateAsync(Location, FolderView.Path.Empty).ConfigureAwait(false);
        
        _ = TestTools.DeleteFile(DummyFileName);

        Assert.That(TestProject.Files, Has.Count.GreaterThan(0));
        ICodeFile? CodeFile = TestProject.Files[0] as ICodeFile;
        Assert.That(CodeFile, Is.Not.Null);
        Assert.That(CodeFile.SourceFile.Name, Is.EqualTo(DummyFileName));

        _ = Assert.ThrowsAsync<FileNotFoundException>(async () => await CodeFile.LoadAsync(TestProject.RootFolder).ConfigureAwait(false));

        CodeFile.Parse();
        Assert.That(CodeFile.SyntaxTree, Is.Null);
    }

    private static void CreateLocalDummyFile(string fileName)
    {
        using FileStream Stream = new(fileName, FileMode.Create, FileAccess.Write);
        using BinaryWriter Writer = new(Stream);
        Writer.Write("Dummy");
    }
}
