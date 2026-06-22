using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RDCore.CLI.App.Messages;
using RDCore.LanguageServer.Extensibility;
using RDCore.SDK.Extensibility;
using RDCore.SDK.Server.Configuration;
using System.IO.Abstractions;
using System.Text.Json;

namespace RDCore.CLI.App.Commands;

/// <summary>
/// The arguments for the <see cref="DescribeExtensionCommand"/>.
/// </summary>
/// <param name="Name">The name of the server executable to serialize a manifest for.</param>
/// <param name="Description">A short description of the extension.<br/><strong>Optional</strong>: <c>string.Empty</c> unless specified otherwise.</param>
/// <param name="Overwrite"><c>true</c> if the command is allowed to overwrite an existing manifest.<br/><strong>Optional</strong>: <c>false</c> unless specified otherwise.</param>
internal readonly record struct DescribeExtensionArgs(
    string Name,
    string Description,
    bool Overwrite);

internal sealed record class DescribeExtensionCommand(
    IRDCoreLogger Logger, 
    IFileSystem FileSystem,
    IOptions<SdkAppOptions> Options,
    IExtensionsProvider Provider) 
    : CLICommand<DescribeExtensionArgs>(CommandNames.DescribeExtensionCommand.Name, CommandNames.DescribeExtensionCommand.Alias)
{
    public override void Execute(DescribeExtensionArgs args)
    {
        if (!Options.Value.Server.UnsafeDevMode)
        {
            // this can only be done in dev mode.
            return;
        }

        var manifestFileName = Options.Value.Platform.Extensions.Manifest;
        var name = args.Name ?? throw new ArgumentException(Resources.Command_InvalidArgs);
        var description = args.Description ?? Resources.Extension_DefaultDescription;
        
        if (!args.Overwrite && FileSystem.File.Exists(manifestFileName))
        {
            Logger.Log(LogLevel.Warning,
                title: Resources.DescribeExtension_Title,
                message: Resources.DescribeExtension_AlreadyExists,
                verbose: name);
            return;
        }

        if (FileSystem.Path.GetFileName(name) != name)
        {
            throw new ArgumentException(Resources.Command_InvalidArgs);
        }

        if (Provider.Describe(name, description) is ExtensionInfo info)
        {
            FileSystem.File.WriteAllText(manifestFileName, JsonSerializer.Serialize(info));
        }
    }
}