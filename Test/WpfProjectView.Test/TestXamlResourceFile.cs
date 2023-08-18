namespace WpfProjectView.Test;

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
        int CodeFileCount = 0;

        foreach (IFile Item in TestProject.Files)
            if (Item is IXamlResourceFile AsXamlResourceFile)
            {
                CodeFileCount++;

                AsXamlResourceFile.Parse();
                Assert.That(AsXamlResourceFile.Content, Is.Null);

                AsXamlResourceFile.LoadAsync(TestProject.RootFolder);
                Assert.That(AsXamlResourceFile.Content, Is.Null);

                AsXamlResourceFile.Parse();
                Assert.That(AsXamlResourceFile.Content, Is.Not.Null);
            }

        Assert.That(CodeFileCount, Is.GreaterThan(0));
    }
}
