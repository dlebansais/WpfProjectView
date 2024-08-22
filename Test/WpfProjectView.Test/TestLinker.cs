﻿namespace WpfProjectView.Test;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class TestLinker
{
    [Test]
    public async Task TestLinkAsync()
    {
        (FolderView.ILocation Location, FolderView.IPath PathToProject) = TestTools.GetLocalLocationAndPathToProject();

        IProject TestProject = await Project.CreateAsync(Location, PathToProject).ConfigureAwait(false);

        foreach (IFile Item in TestProject.Files)
        {
            if (Item.Path.Name == "Empty.xaml")
                continue;

            Assert.That(!Item.IsParsed);

            await Item.LoadAsync(TestProject.RootFolder).ConfigureAwait(false);

            Assert.That(!Item.IsParsed);

            Item.Parse();

            Assert.That(Item.IsParsed);
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

    [Test]
    public async Task TestInvalidFiles()
    {
        (FolderView.ILocation Location, FolderView.IPath PathToProject) = TestTools.GetLocalLocationAndPath("InvalidFiles", TestTools.ProjectName);

        IProject TestProject = await Project.CreateAsync(Location, PathToProject).ConfigureAwait(false);

        foreach (IFile Item in TestProject.Files)
        {
            Assert.That(!Item.IsParsed);

            await Item.LoadAsync(TestProject.RootFolder).ConfigureAwait(false);

            Assert.That(!Item.IsParsed);

            Item.Parse();

            Assert.That(Item.IsParsed);
        }

        XamlLinker Linker = new(TestProject);
        await Linker.LinkAsync().ConfigureAwait(false);

        Assert.That(Linker.Errors, Is.Not.Empty);
    }

    [Test]
    public void TestEmptyAttachecProperty()
    {
        Type TestType = typeof(XamlLinker);
        NamedType NewNamedType = new(TestType.Name, TestType, null);
        Element NewElement = new(NewNamedType, null, new List<Directive>(), new List<Property>(), new List<Event>(), new List<Element>());
        AttachedProperty NewAttachedProperty = new(NewElement, TestType);

        AttachedProperty OtherAttachedProperty = NewAttachedProperty with { };
        Assert.That(OtherAttachedProperty, Is.EqualTo(NewAttachedProperty));
        Assert.That(OtherAttachedProperty.Equals(NewAttachedProperty), Is.True);
    }
}
