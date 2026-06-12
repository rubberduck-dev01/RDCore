using Microsoft.Extensions.Options;
using RDCore.SDK.Server.Configuration;
using System.IO.Abstractions;
using System.Text.Json;

namespace RDCore.LanguageServer.Extensibility;

/// <summary>
/// The information contained in an <em>extension manifest</em>.
/// </summary>
/// <remarks>
/// 🧩 This is how RDCore recognizes an extension as such.
/// </remarks>
/// <param name="Name">The name of the extension executable (.exe).</param>
/// <param name="Title">The <em>friendly name</em> / title of the extension. This should also be the name of the folder.</param>
/// <param name="Version">The version of the extension.</param>
/// <param name="Publisher">The name of the publisher.</param>
/// <param name="PublisherWebUrl">The publisher's web URL.</param>
/// <param name="Description">A short description of the extension.</param>
/// <param name="AppId"> The <em>application ID</em> of a registered extension that authenticates with the RDCore cloud infrastructure, if applicable.</param>
public record class ExtensionInfo(
    string Name,
    string Title,
    Version Version,
    string Publisher,
    string PublisherWebUrl,
    string Description,
    string? AppId)
{
}

/// <summary>
/// A service that can discover and manage platform extensions.
/// </summary>
public interface IExtensionsProvider
{
    /// <summary>
    /// Scans the <em>extensions folder</em> for subfolders containing an <em>extension manifest</em>.
    /// </summary>
    IEnumerable<ExtensionInfo> DiscoverExtensions();
}

internal class ExtensionsClient(IOptions<SdkAppOptions> options, IFileSystem fileSystem) : IExtensionsProvider
{
    // TODO manage extensions

    public IEnumerable<ExtensionInfo> DiscoverExtensions()
    {
        var manifestFileName = options.Value.Platform.Extensions.Manifest;
        var info = fileSystem.DirectoryInfo.New(options.Value.Platform.Extensions.Path);
        foreach (var folder in info.EnumerateDirectories())
        {
            var title = folder.Name;
            if (folder.GetFiles(manifestFileName).FirstOrDefault() is IFileInfo manifest
                && JsonSerializer.Deserialize<ExtensionInfo>(manifest.OpenRead()) is ExtensionInfo extensionInfo)
            {
                yield return extensionInfo;
            }
        }
    }
}
