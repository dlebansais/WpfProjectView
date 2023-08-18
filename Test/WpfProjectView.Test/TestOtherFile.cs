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
        int OtherFileCount = 0;

        foreach (IFile Item in TestProject.Files)
            if (Item is IOtherFile AsOtherFile)
            {
                OtherFileCount++;

                AsOtherFile.Parse();
                Assert.That(AsOtherFile.Content, Is.Null);

                AsOtherFile.LoadAsync(TestProject.RootFolder);
                Assert.That(AsOtherFile.Content, Is.Null);

                AsOtherFile.Parse();
                Assert.That(AsOtherFile.Content, Is.Not.Null);
            }

        Assert.That(OtherFileCount, Is.GreaterThan(0));
    }
}
