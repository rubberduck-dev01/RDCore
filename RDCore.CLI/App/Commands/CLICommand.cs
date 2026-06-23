namespace RDCore.CLI.App.Commands;

internal abstract record class CLICommand<T>(string Name, string? Alias = default) where T : struct
{
    public abstract void Execute(T args);
}
