using RDCore.SDK.Client;
using RDCore.SDK.Extensibility;

namespace RDCore.LanguageServer;

internal interface IPlatformOrchestrationService
{
    public IRDCoreClientApp RuntimeEnvironment { get; }
    public IRDCoreClientApp ParsingService { get; }
    public IEnumerable<IRDCoreClientApp> Extensions { get; }

    public bool RegisterCoreComponent(CoreServerComponent component, IRDCoreClientApp client);
    bool RegisterExtension(ExtensionInfo manifest, IRDCoreClientApp client);
}

internal sealed class PlatformOrchestrationService : IPlatformOrchestrationService
{
    private IRDCoreClientApp? _runtimeEnvironment;
    public IRDCoreClientApp RuntimeEnvironment => _runtimeEnvironment!;

    private IRDCoreClientApp? _parsingService;
    public IRDCoreClientApp ParsingService => _parsingService!;

    private Dictionary<ExtensionInfo, IRDCoreClientApp> _extensions = [];
    public IEnumerable<IRDCoreClientApp> Extensions => _extensions.Values;

    public bool RegisterCoreComponent(CoreServerComponent component, IRDCoreClientApp client)
    {
        switch (component)
        {
            case CoreServerComponent.EnvironmentHost:
                if (_runtimeEnvironment is not null)
                {
                    return false;
                }
                _runtimeEnvironment = client;
                return true;

            case CoreServerComponent.ParsingServer:
                if (_parsingService is not null)
                {
                    return false;
                }
                _parsingService = client;
                return true;
        }
        return false;
    }
    public bool RegisterExtension(ExtensionInfo manifest, IRDCoreClientApp client) => _extensions.TryAdd(manifest, client);
}
