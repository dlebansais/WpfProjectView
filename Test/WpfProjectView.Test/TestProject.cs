﻿namespace WpfProjectView.Test;

using FolderView;
using NUnit.Framework;

public class TestProject
{
    private const string GitHubProjectUserName = "dlebansais";
    private const string GitHubProjectRepositoryName = "PgCompletionist";
    private const string ProjectName = "PgCompletionist";

    [Test]
    public void TestCreateLocal()
    {
        string ProjectRootPath = TestTools.GetExecutingProjectRootPath();
        string TestProjectRootPath = @$"{ProjectRootPath}\..\..\..\{GitHubProjectRepositoryName}\{ProjectName}";
        LocalLocation LocalLocation = new(TestProjectRootPath);

        TestCreate(LocalLocation);
    }

#if ENABLE_REMOTE
    [Test]
    public void TestCreateRemote()
    {
        string RemoteRoot = ProjectName;
        GitHubLocation RemoteLocation = new(GitHubProjectUserName, GitHubProjectRepositoryName, RemoteRoot);

        TestCreate(RemoteLocation);
    }
#endif

    private void TestCreate(ILocation location)
    {
        var TestProjectTask = Project.CreateAsync(location);
        TestProjectTask.Wait();

        IProject TestProject = TestProjectTask.Result;

        Assert.That(TestProject, Is.Not.Null);
        Assert.That(TestProject.Location, Is.EqualTo(location));
        Assert.That(TestProject.Files, Is.Not.Null);
        Assert.That(TestProject.Files, Has.Count.GreaterThan(0));
    }
}
