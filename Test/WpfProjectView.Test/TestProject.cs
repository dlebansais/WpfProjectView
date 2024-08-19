namespace WpfProjectView.Test;

using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class TestProject
{
    [Test]
    public async Task TestCreateAsync()
    {
        (FolderView.ILocation Location, FolderView.IPath PathToProject) = TestTools.GetLocalLocationAndPathToProject();

        IProject TestProject = await Project.CreateAsync(Location, PathToProject).ConfigureAwait(false);

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
        Assert.That(TestProject.Location, Is.EqualTo(FolderView.EmptyLocation.Instance));
        Assert.That(TestProject.Files, Is.Not.Null);
        Assert.That(TestProject.Files, Is.Empty);

        IProject OtherProject = ((Project)TestProject) with { };
        Assert.That(OtherProject, Is.EqualTo(TestProject));
        Assert.That(OtherProject.Equals(TestProject), Is.True);
    }

    [Test]
    public async Task TestNotExisting()
    {
        (FolderView.ILocation Location, FolderView.IPath PathToProject) = TestTools.GetLocalLocationAndPath(TestTools.InvalidProjectRepositoryName, TestTools.InvalidProjectName);

        IProject TestProject = await Project.CreateAsync(Location, PathToProject).ConfigureAwait(false);

        Assert.That(TestProject, Is.Not.Null);
        Assert.That(TestProject.Location, Is.EqualTo(Location));
        Assert.That(TestProject.Files, Is.Not.Null);
        Assert.That(TestProject.Files, Has.Count.EqualTo(3));
        Assert.That(TestProject.PathToExternalDlls, Is.Empty);
    }

    [Test]
    public async Task TestSolutionSameFolder()
    {
        (FolderView.ILocation Location, FolderView.IPath PathToProject) = TestTools.GetLocalLocationAndPath(TestTools.SolutionSameFolderRepositoryName, TestTools.ProjectName);

        IProject TestProject = await Project.CreateAsync(Location, PathToProject).ConfigureAwait(false);

        Assert.That(TestProject, Is.Not.Null);
        Assert.That(TestProject.Location, Is.EqualTo(Location));
        Assert.That(TestProject.Files, Is.Not.Null);
        Assert.That(TestProject.Files, Has.Count.EqualTo(2));
    }
}
