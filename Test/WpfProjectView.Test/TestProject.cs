namespace WpfProjectView.Test;

using System.Threading.Tasks;
using NUnit.Framework;

public class TestProject
{
    [Test]
    public async Task TestCreateAsync()
    {
        FolderView.ILocation Location = TestTools.GetLocalLocation();

        IProject TestProject = await Project.CreateAsync(Location).ConfigureAwait(false);

        Assert.That(TestProject, Is.Not.Null);
        Assert.That(TestProject.Location, Is.EqualTo(Location));
        Assert.That(TestProject.Files, Is.Not.Null);
        Assert.That(TestProject.Files, Has.Count.GreaterThan(0));

        IProject OtherProject = ((Project)TestProject) with { };
        Assert.That(OtherProject, Is.EqualTo(TestProject));
    }
}
