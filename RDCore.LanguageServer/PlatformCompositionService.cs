using RDCore.SDK.Platform;
using System.IO.Abstractions;
using System.Reflection;
using System.Text.Json;

namespace RDCore.LanguageServer;

internal class PlatformCompositionService(IFileSystem fileSystem) : IPlatformCompositionService
{
    private static readonly string _manifestFileName = "rdcore.json";
    private readonly string _rootPath = fileSystem.Directory.GetParent(Assembly.GetEntryAssembly()!.Location)!.FullName;
    private PlatformManifest? _cached;

    public string RootPath => _rootPath;

    public PlatformManifest GetManifest()
    {
        if (_cached is null)
        {
            var path = fileSystem.Path.Combine(_rootPath, _manifestFileName);
            var content = fileSystem.File.ReadAllText(path);
            _cached = JsonSerializer.Deserialize<PlatformManifest>(content)
                ?? throw new InvalidOperationException();
        }
        return _cached;
    }
}
