namespace WpfProjectView.Test;

using System.Threading.Tasks;
using NUnit.Framework;

public class TestProject
{
    [Test]
    public async Task TestCreateAsync()
    {
        FolderView.ILocation Location = TestTools.GetLocalLocation();

        IProject TestProject = await Project.CreateAsync(Location);

        Assert.That(TestProject, Is.Not.Null);
        Assert.That(TestProject.Location, Is.EqualTo(Location));
        Assert.That(TestProject.Files, Is.Not.Null);
        Assert.That(TestProject.Files, Has.Count.GreaterThan(0));
    }
}
