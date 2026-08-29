namespace RDCore.SDK.Platform;

public record class PlatformManifest
{
    public string PlatformVersion { get; init; } = string.Empty;
    public DateTime GeneratedUtc { get; init; } = default;

    public string ExtensionsDirectory { get; init; } = string.Empty;

    public string HostService { get; init; } = string.Empty;
    public string LangService { get; init; } = string.Empty;
    public string ParseServer { get; init; } = string.Empty;
}
