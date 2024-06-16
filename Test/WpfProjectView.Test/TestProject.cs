namespace WpfProjectView.Test;

using System.Threading.Tasks;
using FolderView;
using NUnit.Framework;

public class TestProject
{
    [Test]
    public async Task TestCreateAsync()
    {
        ILocation Location = TestTools.GetLocalLocation();

        IProject TestProject = await Project.CreateAsync(Location).ConfigureAwait(false);

        Assert.That(TestProject, Is.Not.Null);
        Assert.That(TestProject.Location, Is.EqualTo(Location));
        Assert.That(TestProject.Files, Is.Not.Null);
        Assert.That(TestProject.Files, Has.Count.GreaterThan(0));

        IProject OtherProject = ((Project)TestProject) with { };
        Assert.That(OtherProject, Is.EqualTo(TestProject));
    }

    [Test]
    public void TestEmpty()
    {
        IProject TestProject = Project.Empty;

        Assert.That(TestProject, Is.Not.Null);
        Assert.That(TestProject.Location, Is.EqualTo(EmptyLocation.Instance));
        Assert.That(TestProject.Files, Is.Not.Null);
        Assert.That(TestProject.Files, Is.Empty);

        IProject OtherProject = ((Project)TestProject) with { };
        Assert.That(OtherProject, Is.EqualTo(TestProject));
        Assert.That(OtherProject.Equals(TestProject), Is.True);
    }
}
