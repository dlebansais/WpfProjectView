namespace WpfProjectView.Test;

using System.Threading.Tasks;
using NUnit.Framework;

public class TestLinker
{
    [Test, RunInApplicationDomain]
    public async Task TestLinkAsync()
    {
        (FolderView.ILocation Location, FolderView.IPath PathToProject) = TestTools.GetLocalLocationAndPathToProject();

        IProject TestProject = await Project.CreateAsync(Location, PathToProject).ConfigureAwait(false);

        foreach (IFile Item in TestProject.Files)
            if (!Item.IsParsed)
            {
                await Item.LoadAsync(TestProject.RootFolder).ConfigureAwait(false);
                Item.Parse();
            }

        XamlLinker Linker = new(TestProject);
        await Linker.LinkAsync().ConfigureAwait(false);

        Assert.That(Linker.Errors, Is.Empty);
    }

    [Test]
    public async Task TestUnparsedFiles()
    {
        (FolderView.ILocation Location, FolderView.IPath PathToProject) = TestTools.GetLocalLocationAndPathToProject();

        IProject TestProject = await Project.CreateAsync(Location, PathToProject).ConfigureAwait(false);

        XamlLinker Linker = new(TestProject);
        await Linker.LinkAsync().ConfigureAwait(false);

         Assert.That(Linker.Errors, Has.Count.EqualTo(1));
    }
}
 