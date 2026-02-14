namespace RDCore.Workspace.Services;

internal interface IWorkspaceFolderService
{
    void Create(ProjectFile model);
    void Create(string relativeUri, string rootUri);
    void Copy(string source, string destination);
    void Rename(string relativeUri, string rootUri);
    void Delete(string path);
    string[] GetFiles(string path);
}

internal class WorkspaceFolderService : IWorkspaceFolderService
{
    public void Copy(string source, string destination) => File.Copy(source, destination, overwrite: true);

    public void Create(ProjectFile model)
    {
        var rootUri = model.WorkspaceUri;
        if (!Directory.Exists(rootUri))
        {
            Directory.CreateDirectory(rootUri);
        }

        var srcRoot = Path.Combine(rootUri, "src");
        if (!Directory.Exists(srcRoot))
        {
            Directory.CreateDirectory(srcRoot);
        }

        var folders = model.ProjectInfo.GetWorkspaceFolders(srcRoot);
        foreach (var path in folders)
        {
            Directory.CreateDirectory(path);
        }
    }

    public void Create(string relativeUri, string rootUri) => Directory.CreateDirectory(Path.Combine(rootUri, relativeUri));

    public void Delete(string path) => Directory.Delete(path);

    public string[] GetFiles(string path) => Directory.GetFiles(path);

    public void Rename(string relativeUri, string rootUri) => Directory.Move(Path.Combine(rootUri, relativeUri), Path.Combine(rootUri, relativeUri));
}