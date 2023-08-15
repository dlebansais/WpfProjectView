# FolderView

Simple immutable view of a folder structure on disk or on the Internet.

[![Build status](https://ci.appveyor.com/api/projects/status/xewan6agkbf4u3xe?svg=true)](https://ci.appveyor.com/project/dlebansais/folderview) [![codecov](https://codecov.io/gh/dlebansais/FolderView/branch/master/graph/badge.svg?token=ZDdGWyk2Qb)](https://codecov.io/gh/dlebansais/FolderView) [![CodeFactor](https://www.codefactor.io/repository/github/dlebansais/folderview/badge)](https://www.codefactor.io/repository/github/dlebansais/folderview)

# API

## LocalLocation : ILocation

| Name      | Type     | Comment                                 |
|-----------|----------|-----------------------------------------|
| LocalRoot | `string` | The path to the root folder of interest |

## GitHubLocation : ILocation

| Name           | Type     | Comment                                                                  |
|----------------|----------|--------------------------------------------------------------------------|
| UserName       | `string` | The user name for repositories on GitHub                                 |
| RepositoryName | `string` | The repository name                                                      |
| RemoteRoot     | `string` | The path to the root folder in the repository, / for the repository root |

## Path : IPath

| Name      | Type            | Comment                                                                                      |
|-----------|-----------------|----------------------------------------------------------------------------------------------|
| Ancestors | `IList<string>` | The list of ancestor folders, from the root to the folder, an empty list for the root folder |
| Name      | `string`        | The name of a file or subfolder in the folder                                                |

### RootFolderFrom
Gets a root folder from a local path or remote address.

`public static async Task<IFolder> RootFolderFromAsync(ILocation location)`

### Combine
Combines a parent folder, or path, and a name to return the path to that name. In the case of a folder, a `null` parent indicates the root folder.

`public static IPath Combine(IFolder? parent, string name)`<br/>
`public static IPath Combine(IPath parent, string name)`

### GetRelativeFolder
Gets the folder starting from a parent and following a path.

`public static IFolder GetRelativeFolder(IFolder parent, IPath path)`

### GetRelativeFile
Gets the folder starting from a parent and following a path.

`public static IFile GetRelativeFile(IFolder parent, IPath path)`

# Loading content

The implementation of the `IFile` interface provides a `public async Task LoadAsync()` method that fills the `public byte[]? Content { get; }` property.

Notes:
+ If a file content is modified, calling `LoadAsync` updates `Content` with the new content.
+ If a file is deleted, calling `LoadAsync` throws an exception and `Content` is left unchanged.
