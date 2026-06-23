using RDCore.CLI.App.Messages;
using RDCore.CLI.App.Messages.Model;
using RDCore.CLI.Themes.Model;

namespace RDCore.CLI.App.Commands;

internal readonly record struct SplashArgs(
    bool Show = true);

internal record class ShowSplashCommand : CLICommand<SplashArgs>
{
    private readonly IConsoleMessageWriter _writer = default!;
    private readonly IAppThemeService _themes;
    public ShowSplashCommand(IConsoleMessageWriter writer, IAppThemeService themes) : base("slash")
    {
        _writer = writer;
        _themes = themes;
    }

    public override void Execute(SplashArgs args)
    {
        if (!args.Show)
        {
            return;
        }

        var adjustedBackground = string.Join(Environment.NewLine, Resources.RDCoreSplash_Background
            .Split(Environment.NewLine)
            .Select(line => $"{new string(' ', 15)}{line}"));

        _writer
            .WriteAssemblyInfo()
            .WriteLegalNotice()
            .WriteMessage(new ConsoleMessageBuilder()
                .WithKind(MessageKind.Trace)
                .WithTitle(Environment.NewLine + adjustedBackground)
                .WithMessageBody(Resources.RDCoreSplash_Foreground, nameof(ConsoleColor.White)))
            .WriteSlogan();
    }
}
