namespace WpfProjectView.Test;

using System.IO;
using System.Threading.Tasks;
using FolderView;
using NUnit.Framework;

[TestFixture]
public class TestXamlResourceFile
{
    [Test]
    public async Task TestLoadXamlResourceAsync()
    {
        (ILocation Location, IPath PathToProject) = TestTools.GetLocalLocationAndPathToProject();

        IProject TestProject = await Project.CreateAsync(Location, PathToProject).ConfigureAwait(false);
        int XamlFileCount = 0;

        foreach (var Item in TestProject.Files)
            if (Item is IXamlResourceFile AsXamlResourceFile && AsXamlResourceFile.Path.Name != "Empty.xaml")
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
        TestTools.CreateLocalDummyFile(DummyFileName);

        LocalLocation Location = new(".");
        IProject TestProject = await Project.CreateAsync(Location, FolderView.Path.Empty).ConfigureAwait(false);

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
}
