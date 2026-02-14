using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace RDCore.Workspace;

internal record class ProjectFile
{
    public static readonly string FileName = ".rdproj";

    [JsonIgnore]
    public bool IsDirty { get; init; }

    [JsonIgnore]
    public string WorkspaceUri { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;

    public RDCoreProject ProjectInfo { get; init; } = default!;

    public ProjectFile WithWorkspaceUri(string uri) => this with { WorkspaceUri = uri };

    public ProjectFile WithReference(RDCoreReference reference) => this with { ProjectInfo = ProjectInfo.WithReference(reference), IsDirty = true };
    public ProjectFile WithModule(RDCoreModule module) => this with { ProjectInfo = ProjectInfo.WithModule(module), IsDirty = true };
    public ProjectFile WithDocument(RDCoreFile document) => this with { ProjectInfo = ProjectInfo.WithDocument(document), IsDirty = true };
    public ProjectFile WithFolder(string folder) => this with { ProjectInfo = ProjectInfo.WithFolder(folder), IsDirty = true };

    public ProjectFile WithoutReference(RDCoreReference reference) => this with { ProjectInfo = ProjectInfo.WithoutReference(reference), IsDirty = true };
    public ProjectFile WithoutModule(RDCoreModule module) => this with { ProjectInfo = ProjectInfo.WithoutModule(module), IsDirty = true };
    public ProjectFile WithoutDocument(RDCoreFile document) => this with { ProjectInfo = ProjectInfo.WithoutDocument(document), IsDirty = true };
    public ProjectFile WithoutFolder(string folder) => this with { ProjectInfo = ProjectInfo.WithoutFolder(folder), IsDirty = true };

    public override int GetHashCode() => WorkspaceUri.GetHashCode();
}

internal record class RDCoreProject
{
    public string Name { get; init; } = string.Empty;
    public RDCoreReference[] References { get; init; } = [RDCoreReference.VBStandardLibrary];
    public RDCoreModule[] Modules { get; init; } = [];
    public RDCoreFile[] OtherFiles { get; init; } = [];
    public string[] Folders { get; set; } = [];

    public HashSet<string> GetWorkspaceFolders(string srcRoot) => 
        [
            .. Modules.Select(module => Path.Combine(srcRoot, module.RelativeUri)),
            .. OtherFiles.Select(file => Path.Combine(srcRoot, file.RelativeUri)),
            .. Folders.Select(folder => Path.Combine(srcRoot, folder)),
        ];

    public IEnumerable<RDCoreFile> GetFolderFiles(string folder) => 
        [
            .. Modules.Where(m => Path.GetDirectoryName(m.RelativeUri)!.Replace('\\', '/').StartsWith(folder)),
            .. OtherFiles.Where(f => Path.GetDirectoryName(f.RelativeUri)!.Replace('\\', '/').StartsWith(folder)),
        ];

    public RDCoreProject WithName(string name) => this with { Name = name };
    public RDCoreProject WithReference(RDCoreReference reference) => this with { References = [.. References, reference] };
    public RDCoreProject WithModule(RDCoreModule module) => this with { Modules = [.. Modules, module] };
    public RDCoreProject WithDocument(RDCoreFile document) => this with { OtherFiles = [.. OtherFiles, document] };
    public RDCoreProject WithFolder(string folder) => this with { Folders = [.. Folders, folder] };

    public RDCoreProject WithoutReference(RDCoreReference reference) => this with { References = [.. References.Where(r => r != reference)] };
    public RDCoreProject WithoutModule(RDCoreModule module) => this with { Modules = [.. Modules.Where(m => m != module)] };
    public RDCoreProject WithoutDocument(RDCoreFile document) => this with { OtherFiles = [.. OtherFiles.Where(d => d != document)] };
    public RDCoreProject WithoutFolder(string folder) => this with { Folders = [.. Folders.Where(f => f != folder)] };
}

internal record class RDCoreReference
{
    public static RDCoreReference VBStandardLibrary { get; } = new RDCoreReference
    {
        Name = "VBA",
        Guid = new Guid("000204ef-0000-0000-c000-000000000046"),
        AbsolutePath = "C:\\Program Files\\Microsoft Office\\root\\vfs\\ProgramFilesCommonX64\\Microsoft Shared\\VBA\\VBA7.1\\VBE7.DLL",
        Major = 4,
        Minor = 2,
        IsUnremovable = true,
    };

    public string Name { get; init; } = string.Empty;
    public string? AbsolutePath { get; init; }
    public Guid? Guid { get; init; }
    public int? Major { get; init; }
    public int? Minor { get; init; }
    public string? TypeLibInfoPath { get; init; }

    public bool IsUnremovable { get; init; } = false;

    public override int GetHashCode() => HashCode.Combine(Name, Guid, Major, Minor);
}

internal record class RDCoreFile : IEquatable<RDCoreFile>
{
    public string RelativeUri { get; init; } = string.Empty;

    [JsonIgnore]
    public string Extension => RelativeUri[^RelativeUri.LastIndexOf('.')..];

    [JsonIgnore]
    public string DefaultName => Path.GetFileNameWithoutExtension(RelativeUri.Replace('\\', '/').Split('/').Last());

    public override int GetHashCode() => RelativeUri.GetHashCode();
    
    bool IEquatable<RDCoreFile>.Equals(RDCoreFile? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return RelativeUri == other.RelativeUri;
    }
}

internal enum DocClassType
{
    Unknown = 0,
    ExcelWorkbook = 1,
    ExcelWorksheet = 2,
    AccessForm = 3,
    AccessReport = 4,
}

internal record class RDCoreModule : RDCoreFile
{
    /// <summary>
    /// The name of the module; must be unique across the entire workspace.
    /// </summary>
    /// <remarks>
    /// The value of the module's <c>VB_Name</c> attribute.
    /// </remarks>
    public string Name { get; init; } = string.Empty;
    public DocClassType? Super { get; init; }
}
