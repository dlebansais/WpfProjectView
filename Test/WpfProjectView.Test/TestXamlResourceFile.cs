namespace WpfProjectView.Test;

using System.IO;
using FolderView;
using NUnit.Framework;

public class TestXamlResourceFile
{
    [Test]
    public void TestLoad()
    {
        FolderView.ILocation Location = TestTools.GetLocalLocation();

        var TestProjectTask = Project.CreateAsync(Location);
        TestProjectTask.Wait();

        IProject TestProject = TestProjectTask.Result;
        int XamlFileCount = 0;

        foreach (var Item in TestProject.Files)
            if (Item is IXamlResourceFile AsXamlResourceFile)
            {
                XamlFileCount++;

                Assert.That(AsXamlResourceFile.XamlParsingResult, Is.Null);

                AsXamlResourceFile.Parse();
                Assert.That(AsXamlResourceFile.XamlParsingResult, Is.Not.Null);
                Assert.That(AsXamlResourceFile.XamlParsingResult.Root, Is.Null);

                AsXamlResourceFile.LoadAsync(TestProject.RootFolder);
                Assert.That(AsXamlResourceFile.XamlParsingResult.Root, Is.Null);

                AsXamlResourceFile.Parse();
                Assert.That(AsXamlResourceFile.XamlParsingResult.Root, Is.Not.Null);
            }

        Assert.That(XamlFileCount, Is.GreaterThan(0));
    }

    [Test]
    public void TestNullContent()
    {
        string DummyFileName = "dummy.xaml";
        _ = TestTools.DeleteFile(DummyFileName);
        CreateLocalDummyFile(DummyFileName);

        LocalLocation Location = new(".");
        var TestProjectTask = Project.CreateAsync(Location);
        TestProjectTask.Wait();
        IProject TestProject = TestProjectTask.Result;

        _ = TestTools.DeleteFile(DummyFileName);

        Assert.That(TestProject.Files, Has.Count.GreaterThan(0));
        IXamlResourceFile? XamlFile = TestProject.Files[0] as IXamlResourceFile;
        Assert.That(XamlFile, Is.Not.Null);
        Assert.That(XamlFile.SourceFile.Name, Is.EqualTo(DummyFileName));

        Assert.ThrowsAsync<FileNotFoundException>(async () => await XamlFile.LoadAsync(TestProject.RootFolder));

        XamlFile.Parse();
        Assert.That(XamlFile.XamlParsingResult, Is.Not.Null);
        Assert.That(XamlFile.XamlParsingResult.Root, Is.Null);
    }

    private void CreateLocalDummyFile(string fileName)
    {
        using FileStream Stream = new(fileName, FileMode.Create, FileAccess.Write);
        using BinaryWriter Writer = new BinaryWriter(Stream);
        Writer.Write("Dummy");
    }
}
