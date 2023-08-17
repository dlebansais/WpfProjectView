namespace WpfProjectView.Test;

using NUnit.Framework;

public class TestCodeFile
{
    [Test]
    public void TestLoad()
    {
        FolderView.ILocation Location = TestTools.GetLocalLocation();

        var TestProjectTask = Project.CreateAsync(Location);
        TestProjectTask.Wait();

        IProject TestProject = TestProjectTask.Result;
        int CodeFileCount = 0;

        foreach (IFile Item in TestProject.Files)
            if (Item is ICodeFile AsCodeFile)
            {
                CodeFileCount++;

                AsCodeFile.LoadAsync(TestProject.RootFolder);
                Assert.That(AsCodeFile.SyntaxTree, Is.Not.Null);
            }

        Assert.That(CodeFileCount, Is.GreaterThan(0));
    }
}
