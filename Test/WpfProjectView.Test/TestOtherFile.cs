﻿namespace WpfProjectView.Test;

using System.Threading.Tasks;
using NUnit.Framework;

public class TestOtherFile
{
    [Test]
    public async Task TestLoadAsync()
    {
        FolderView.ILocation Location = TestTools.GetLocalLocation();

        IProject TestProject = await Project.CreateAsync(Location).ConfigureAwait(false);
        int OtherFileCount = 0;

        foreach (IFile Item in TestProject.Files)
            if (Item is IOtherFile AsOtherFile)
            {
                OtherFileCount++;

                Assert.That(AsOtherFile.Path, Is.Not.Null);

                AsOtherFile.Parse();
                Assert.That(AsOtherFile.Content, Is.Null);

                await AsOtherFile.LoadAsync(TestProject.RootFolder).ConfigureAwait(false);
                Assert.That(AsOtherFile.Content, Is.Null);

                AsOtherFile.Parse();
                Assert.That(AsOtherFile.Content, Is.Not.Null);
            }

        Assert.That(OtherFileCount, Is.GreaterThan(0));
    }
}
