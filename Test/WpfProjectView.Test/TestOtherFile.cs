namespace WpfProjectView.Test;

using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class TestOtherFile
{
    [Test]
    public async Task TestLoadOtherAsync()
    {
        (FolderView.ILocation Location, FolderView.IPath PathToProject) = TestTools.GetLocalLocationAndPathToProject();

        IProject TestProject = await Project.CreateAsync(Location, PathToProject).ConfigureAwait(false);
        int OtherFileCount = 0;

        foreach (IFile Item in TestProject.Files)
            if (Item is IOtherFile AsOtherFile)
            {
                OtherFileCount++;

                Assert.That(AsOtherFile.Path, Is.Not.Null);

                AsOtherFile.Parse();
                Assert.That(AsOtherFile.Content, Is.Null);
                Assert.That(AsOtherFile.IsParsed, Is.False);

                await AsOtherFile.LoadAsync(TestProject.RootFolder).ConfigureAwait(false);
                Assert.That(AsOtherFile.Content, Is.Null);

                AsOtherFile.Parse();
                Assert.That(AsOtherFile.Content, Is.Not.Null);
                Assert.That(AsOtherFile.IsParsed, Is.True);
            }

        Assert.That(OtherFileCount, Is.GreaterThan(0));
    }
}
