namespace WpfProjectView.Test;

using System.IO;
using System.Threading.Tasks;
using FolderView;
using NUnit.Framework;

public class TestXamlResourceFile
{
    [Test]
    public async Task TestLoadAsync()
    {
        ILocation Location = TestTools.GetLocalLocation();

        IProject TestProject = await Project.CreateAsync(Location).ConfigureAwait(false);
        int XamlFileCount = 0;

        foreach (var Item in TestProject.Files)
            if (Item is IXamlResourceFile AsXamlResourceFile)
            {
                XamlFileCount++;

                Assert.That(AsXamlResourceFile.SourceFile, Is.Not.Null);
                Assert.That(AsXamlResourceFile.SourceFile.Content, Is.Null);
                Assert.That(AsXamlResourceFile.XamlParsingResult, Is.Null);

                AsXamlResourceFile.Parse();

                Assert.That(AsXamlResourceFile.SourceFile, Is.Not.Null);
                Assert.That(AsXamlResourceFile.SourceFile.Content, Is.Null);
                Assert.That(AsXamlResourceFile.XamlParsingResult, Is.Not.Null);
                Assert.That(AsXamlResourceFile.XamlParsingResult.Root, Is.Null);

                await AsXamlResourceFile.LoadAsync(TestProject.RootFolder).ConfigureAwait(false);

                Assert.That(AsXamlResourceFile.SourceFile, Is.Not.Null);
                Assert.That(AsXamlResourceFile.SourceFile.Content, Is.Not.Null);
                Assert.That(AsXamlResourceFile.XamlParsingResult, Is.Not.Null);
                Assert.That(AsXamlResourceFile.XamlParsingResult.Root, Is.Null);

                AsXamlResourceFile.Parse();

                Assert.That(AsXamlResourceFile.SourceFile, Is.Not.Null);
                Assert.That(AsXamlResourceFile.SourceFile.Content, Is.Not.Null);
                Assert.That(AsXamlResourceFile.XamlParsingResult, Is.Not.Null);
                Assert.That(AsXamlResourceFile.XamlParsingResult.Root, Is.Not.Null);

                string ComparisonMessage = TestTools.CompareXamlParingResultWithOriginalContent(AsXamlResourceFile.SourceFile.Content, AsXamlResourceFile.XamlParsingResult);
                Assert.That(ComparisonMessage, Is.Empty, $"{AsXamlResourceFile.SourceFile.Name}\r\n{ComparisonMessage}");
            }

        Assert.That(XamlFileCount, Is.GreaterThan(0));
    }

    [Test]
    public async Task TestNullContentAsync()
    {
        const string DummyFileName = "dummy.xaml";
        _ = TestTools.DeleteFile(DummyFileName);
        CreateLocalDummyFile(DummyFileName);

        LocalLocation Location = new(".");
        IProject TestProject = await Project.CreateAsync(Location).ConfigureAwait(false);

        _ = TestTools.DeleteFile(DummyFileName);

        Assert.That(TestProject.Files, Has.Count.GreaterThan(0));
        IXamlResourceFile? XamlFile = TestProject.Files[0] as IXamlResourceFile;
        Assert.That(XamlFile, Is.Not.Null);
        Assert.That(XamlFile.SourceFile.Name, Is.EqualTo(DummyFileName));

        _ = Assert.ThrowsAsync<FileNotFoundException>(async () => await XamlFile.LoadAsync(TestProject.RootFolder).ConfigureAwait(false));

        XamlFile.Parse();
        Assert.That(XamlFile.XamlParsingResult, Is.Not.Null);
        Assert.That(XamlFile.XamlParsingResult.Root, Is.Null);
    }

    private static void CreateLocalDummyFile(string fileName)
    {
        using FileStream Stream = new(fileName, FileMode.Create, FileAccess.Write);
        using BinaryWriter Writer = new(Stream);
        Writer.Write("Dummy");
    }
}
