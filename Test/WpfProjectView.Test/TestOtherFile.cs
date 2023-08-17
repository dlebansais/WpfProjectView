namespace WpfProjectView.Test;

using NUnit.Framework;

public class TestOtherFile
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
            if (Item is IOtherFile AsOtherFile)
            {
                CodeFileCount++;

                AsOtherFile.LoadAsync(TestProject.RootFolder);
                Assert.That(AsOtherFile.Content, Is.Not.Null);
            }

        Assert.That(CodeFileCount, Is.GreaterThan(0));
    }
}
