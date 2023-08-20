namespace WpfProjectView.Test;

using NUnit.Framework;

public class TestXamlCodeFile
{
    [Test]
    public void TestLoad()
    {
        FolderView.ILocation Location = TestTools.GetLocalLocation();

        var TestProjectTask = Project.CreateAsync(Location);
        TestProjectTask.Wait();

        IProject TestProject = TestProjectTask.Result;
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

                AsXamlCodeFile.LoadAsync(TestProject.RootFolder);

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

                TestTools.CompareXamlParingResultWithOriginalContent(AsXamlCodeFile.XamlSourceFile.Content, AsXamlCodeFile.XamlParsingResult);
            }

        Assert.That(FileCount, Is.GreaterThan(0));
    }
}
