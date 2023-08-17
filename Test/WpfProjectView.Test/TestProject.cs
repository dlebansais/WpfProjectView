namespace WpfProjectView.Test;

using NUnit.Framework;

public class TestProject
{
    [Test]
    public void TestCreate()
    {
        FolderView.ILocation Location = TestTools.GetLocalLocation();

        var TestProjectTask = Project.CreateAsync(Location);
        TestProjectTask.Wait();

        IProject TestProject = TestProjectTask.Result;

        Assert.That(TestProject, Is.Not.Null);
        Assert.That(TestProject.Location, Is.EqualTo(Location));
        Assert.That(TestProject.Files, Is.Not.Null);
        Assert.That(TestProject.Files, Has.Count.GreaterThan(0));
    }
}
