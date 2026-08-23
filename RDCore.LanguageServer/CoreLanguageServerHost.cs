using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RDCore.SDK.Platform;
using RDCore.SDK.Server;

namespace RDCore.LanguageServer;

/// <summary>
/// The RDCore <strong>RD-VBA Language Server</strong> application host.
/// </summary>
internal sealed class CoreLanguageServerHost() : RDCorePlatformServerHost<CoreLanguageServerApp>()
{
    protected override void ConfigureAdditionalExternalServices(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton<IPlatformCompositionService, PlatformCompositionService>()
            .AddSingleton<IPlatformOrchestrationService, PlatformOrchestrationService>();
    }
}
