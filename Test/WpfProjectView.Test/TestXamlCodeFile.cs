namespace WpfProjectView.Test;

using System.Threading.Tasks;
using NUnit.Framework;

public class TestXamlCodeFile
{
    [Test]
    public async Task TestLoadAsync()
    {
        FolderView.ILocation Location = TestTools.GetLocalLocation();

        IProject TestProject = await Project.CreateAsync(Location);
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

                await AsXamlCodeFile.LoadAsync(TestProject.RootFolder);

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
                Assert.That(ComparisonMessage, Is.Empty, $"{AsXamlCodeFile.XamlSourceFile.Name}\r\n{ComparisonMessage}");
            }

        Assert.That(FileCount, Is.GreaterThan(0));
    }
}
