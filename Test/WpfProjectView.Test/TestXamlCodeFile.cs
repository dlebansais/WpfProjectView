namespace WpfProjectView.Test;

using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture, Order(5)]
public class TestXamlCodeFile
{
    [Test]
    public async Task TestLoadXamlAsync()
    {
        (FolderView.ILocation Location, FolderView.IPath PathToProject) = TestTools.GetLocalLocationAndPathToProject();

        IProject TestProject = await Project.CreateAsync(Location, PathToProject).ConfigureAwait(false);
        int FileCount = 0;

        foreach (IFile Item in TestProject.Files)
            if (Item is IXamlCodeFile AsXamlCodeFile)
            {
                FileCount += 2;

                Assert.That(AsXamlCodeFile.XamlSourceFile, Is.Not.Null);
                Assert.That(AsXamlCodeFile.XamlSourceFile.Content, Is.Null);
                Assert.That(AsXamlCodeFile.CodeSourceFile, Is.Not.Null);
                Assert.That(AsXamlCodeFile.CodeSourceFile.Content, Is.Null);
                Assert.That(AsXamlCodeFile.XamlParsingResult, Is.Null);

                AsXamlCodeFile.Parse();

                Assert.That(AsXamlCodeFile.XamlSourceFile, Is.Not.Null);
                Assert.That(AsXamlCodeFile.XamlSourceFile.Content, Is.Null);
                Assert.That(AsXamlCodeFile.CodeSourceFile, Is.Not.Null);
                Assert.That(AsXamlCodeFile.CodeSourceFile.Content, Is.Null);
                Assert.That(AsXamlCodeFile.XamlParsingResult, Is.Not.Null);
                Assert.That(AsXamlCodeFile.XamlParsingResult.Root, Is.Null);
                Assert.That(AsXamlCodeFile.SyntaxTree, Is.Null);

                await AsXamlCodeFile.LoadAsync(TestProject.RootFolder).ConfigureAwait(false);

                Assert.That(AsXamlCodeFile.XamlSourceFile, Is.Not.Null);
                Assert.That(AsXamlCodeFile.XamlSourceFile.Content, Is.Not.Null);
                Assert.That(AsXamlCodeFile.CodeSourceFile, Is.Not.Null);
                Assert.That(AsXamlCodeFile.CodeSourceFile.Content, Is.Not.Null);
                Assert.That(AsXamlCodeFile.XamlParsingResult, Is.Not.Null);
                Assert.That(AsXamlCodeFile.XamlParsingResult.Root, Is.Null);
                Assert.That(AsXamlCodeFile.SyntaxTree, Is.Null);

                AsXamlCodeFile.Parse();

                Assert.That(AsXamlCodeFile.XamlParsingResult, Is.Not.Null);
                Assert.That(AsXamlCodeFile.XamlParsingResult.Root, Is.Not.Null);
                Assert.That(AsXamlCodeFile.SyntaxTree, Is.Not.Null);

                string ComparisonMessage = TestTools.CompareXamlParingResultWithOriginalContent(AsXamlCodeFile.XamlSourceFile.Content, AsXamlCodeFile.XamlParsingResult);

                if (ComparisonMessage != string.Empty)
                {
                }

                Assert.That(ComparisonMessage, Is.Empty, $"{AsXamlCodeFile.XamlSourceFile.Name}\r\n{ComparisonMessage}");
            }

        Assert.That(FileCount, Is.GreaterThan(0));
    }
}
