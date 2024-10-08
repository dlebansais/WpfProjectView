﻿namespace WpfProjectView;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using FolderView;
using NuGet.Configuration;
using SlnExplorer;

/// <summary>
/// Represents a project.
/// </summary>
[DebuggerDisplay("{Location,nq}")]
public record Project : IProject
{
    /// <summary>
    /// Gets the empty project.
    /// </summary>
    public static Project Empty { get; } = new Project()
    {
        Location = EmptyLocation.Instance,
        RootFolder = EmptyLocation.Root,
        Files = new List<IFile>(),
        PathToExternalDlls = new List<string>(),
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="Project"/> class.
    /// </summary>
    private Project()
    {
    }

    /// <inheritdoc cref="IProject.Location" />
    public required ILocation Location { get; init; }

    /// <inheritdoc cref="IProject.RootFolder" />
    public required IFolder RootFolder { get; init; }

    /// <inheritdoc cref="IProject.Files" />
    public required IReadOnlyList<IFile> Files { get; init; }

    /// <inheritdoc cref="IProject.PathToExternalDlls" />
    public required IReadOnlyList<string> PathToExternalDlls { get; init; }

    /// <summary>
    /// Returns an instance of <see cref="Project"/> for the provided location.
    /// </summary>
    /// <param name="location">The solution location.</param>
    /// <param name="pathToProject">The project location.</param>
    public static async Task<Project> CreateAsync(ILocation location, IPath pathToProject)
    {
        Contract.RequireNotNull(location, out ILocation Location);
        Contract.RequireNotNull(pathToProject, out IPath PathToProject);

        List<IFile> Files = new();

        IFolder SolutionFolder = await Path.RootFolderFromAsync(Location).ConfigureAwait(false);
        IFolder NewRootFolder = ReferenceEquals(Path.Empty, PathToProject) ? SolutionFolder : Path.GetRelativeFolder(SolutionFolder, PathToProject);
        FillFileList(NewRootFolder, Files);

        bool IsSolutionFound = TryGetSolutionFile(SolutionFolder, out FolderView.IFile SolutionFile);
        List<string> PathToExternalDlls = IsSolutionFound ? GetPathToExternalDlls(Location, PathToProject, SolutionFile, Files) : new();

        Project Result = new() { Location = Location, RootFolder = NewRootFolder, Files = Files, PathToExternalDlls = PathToExternalDlls };

        return Result;
    }

    private static List<string> GetPathToExternalDlls(ILocation location, IPath pathToProject, FolderView.IFile solutionFile, List<IFile> files)
    {
        List<string> PathToExternalDlls = new();

        string SolutionFullPath = location.GetAbsolutePath(solutionFile.Path);
        Solution Solution = new(SolutionFullPath);

        if (TryFindProject(location, pathToProject, Solution, files, out IReadOnlyList<PackageReference> NugetReferences, out IReadOnlyList<string> ProjectReferences))
        {
            foreach (string ProjItem in ProjectReferences)
            {
                foreach (var OtherProj in Solution.ProjectList)
                    if (OtherProj.ProjectName == ProjItem)
                    {
                        string SolutionDirectoryName = Contract.AssertNotNull(System.IO.Path.GetDirectoryName(SolutionFullPath));
                        string ProjectRefRootPath = System.IO.Path.Combine(SolutionDirectoryName, OtherProj.RelativePath);
                        string ProjectDirectoryName = Contract.AssertNotNull(System.IO.Path.GetDirectoryName(ProjectRefRootPath));
                        string ProjectRefPath = System.IO.Path.Combine(ProjectDirectoryName, "bin", "x64", "Release", "net481", $"{ProjItem}.dll");
                        PathToExternalDlls.Add(ProjectRefPath);
                    }
            }

            ISettings settings = Settings.LoadDefaultSettings(null);
            string nugetPath = SettingsUtility.GetGlobalPackagesFolder(settings);

            foreach (PackageReference PackageReference in NugetReferences)
            {
                if (PackageReference.Name.EndsWith("-Debug", StringComparison.Ordinal))
                    continue;

                string PackagePath = System.IO.Path.Combine(nugetPath, PackageReference.Name, PackageReference.Version, "lib", "net481");

                if (System.IO.Directory.Exists(PackagePath))
                {
                    foreach (string DllFile in System.IO.Directory.GetFiles(PackagePath, "*.dll"))
                    {
                        PathToExternalDlls.Add(DllFile);
                    }
                }
            }
        }

        return PathToExternalDlls;
    }

    private static bool TryFindProject(ILocation location, IPath pathToProject, Solution solution, List<IFile> files, out IReadOnlyList<PackageReference> nugetReferences, out IReadOnlyList<string> projectReferences)
    {
        foreach (var Item in solution.ProjectList)
        {
            string RelativePath = Item.RelativePath;

            foreach (IFile File in files)
                if (File is IOtherFile OtherFile)
                    if (OtherFile.Path.Name.EndsWith(".csproj", StringComparison.Ordinal))
                    {
                        List<string> AllAncestors = new(pathToProject.Ancestors) { pathToProject.Name };
                        bool IsSameRelativeFolder = OtherFile.Path.Ancestors.SequenceEqual(AllAncestors);
                        if (IsSameRelativeFolder)
                        {
                            string ProjectPath = location.GetAbsolutePath(OtherFile.Path);
                            Item.LoadDetails(ProjectPath);

                            nugetReferences = Item.PackageReferenceList;
                            projectReferences = Item.ProjectReferences;
                            return true;
                        }
                    }
        }

        Contract.Unused(out nugetReferences);
        Contract.Unused(out projectReferences);
        return false;
    }

    private static bool TryGetSolutionFile(IFolder folder, out FolderView.IFile solutionFile)
    {
        foreach (var Item in folder.Files)
            if (Item.Path.Name.EndsWith(".sln", StringComparison.Ordinal))
            {
                solutionFile = Item;
                return true;
            }

        List<string> IgnoredFolders = new() { "bin", "obj" };
        bool IsRootFolder = folder.IsRoot;

        // Proceed with sub-folders recursively.
        (bool IsFound, solutionFile) = BrowseFolder(folder, (IFolder item) =>
        {
            bool IsFound = TryGetSolutionFile(item, out FolderView.IFile SolutionFile);
            return (IsFound, SolutionFile);
        });

        return IsFound;
    }

    private static void FillFileList(IFolder folder, List<IFile> files)
    {
        List<FolderView.IFile> XamlFileList = new();
        List<FolderView.IFile> CodeFileList = new();
        List<FolderView.IFile> OtherFileList = new();

        // Split files in 3 categories.
        foreach (var Item in folder.Files)
            if (Item.Path.Name.EndsWith(".xaml", StringComparison.Ordinal))
                XamlFileList.Add(Item);
            else if (Item.Path.Name.EndsWith(".cs", StringComparison.Ordinal))
                CodeFileList.Add(Item);
            else
                OtherFileList.Add(Item);

        // Find xaml files with associated code behind.
        List<(FolderView.IFile DotXaml, FolderView.IFile DotCs)> XamlWithCodeBehindList = new();

        foreach (var XamlItem in XamlFileList)
            if (CodeFileList.Find(item => item.Path.Name == XamlItem.Path.Name + ".cs") is FolderView.IFile CodeItem)
                XamlWithCodeBehindList.Add((XamlItem, CodeItem));

        // Move xaml files with code behind in their own list.
        List<FolderView.IFile> XamlCodeFileList = new();

        foreach (var Item in XamlWithCodeBehindList)
        {
            XamlCodeFileList.Add(Item.DotXaml);

            _ = XamlFileList.Remove(Item.DotXaml);
            _ = CodeFileList.Remove(Item.DotCs);
        }

        // Finally fill the list of files.
        XamlWithCodeBehindList.ForEach(item => FillFileList(new XamlCodeFile(item.DotXaml, item.DotCs), files));
        XamlFileList.ForEach(item => FillFileList(new XamlResourceFile(item), files));
        CodeFileList.ForEach(item => FillFileList(new CodeFile(item), files));
        OtherFileList.ForEach(item => FillFileList(new OtherFile(item), files));

        List<string> IgnoredFolders = new() { "bin", "obj" };
        bool IsRootFolder = folder.IsRoot;

        // Proceed with sub-folders recursively.
        _ = BrowseFolder(folder, (IFolder item) =>
        {
            FillFileList(item, files);
            Contract.Unused(out object UnusedResult);
            return (false, UnusedResult);
        });
    }

    private static void FillFileList<T>(T newFile, List<IFile> files)
        where T : File
    {
        files.Add(newFile);
    }

    private static (bool Stop, T Result) BrowseFolder<T>(IFolder folder, Func<IFolder, (bool Stop, T Result)> predicate)
        where T : class
    {
        List<string> IgnoredFolders = new() { "bin", "obj" };

        foreach (IFolder Item in folder.Folders)
            if (!IgnoredFolders.Contains(Item.Name))
            {
                (bool Stop, T Result) = predicate(Item);
                if (Stop)
                    return (true, Result);
            }

        Contract.Unused(out T UnusedResult);
        return (false, UnusedResult);
    }
}
