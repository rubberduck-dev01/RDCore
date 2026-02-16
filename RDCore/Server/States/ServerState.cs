using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace RDCore.Server.States;

internal abstract record class ServerState
{
    public static ServerState Starting { get; } = new StartingServerState();
    public static ServerState Initializing { get; } = new InitializingServerState();
    public static ServerState Running { get; } = new RunningServerState();
    public static ServerState RunningVerbose { get; } = new RunningVerboseServerState();
    public static ServerState RunningTraceless { get; } = new RunningTracelessServerState();
    public static ServerState ShuttingDown { get; } = new ShuttingDownServerState();
    public static ServerState Exiting(ServerStateValue state) => new ExitingServerState(state);

    protected ServerState(ServerStateValue value)
    {
        Value = value;
    }

    public ServerStateValue Value { get; }

    public virtual int ExitCode => 1;
}

internal record class StartingServerState : ServerState
{
    public StartingServerState() : base(ServerStateValue.Starting) { }
    public override int ExitCode => 0;
}
internal record class InitializingServerState : ServerState { public InitializingServerState() : base(ServerStateValue.Initializing) { } }
internal record class RunningServerState : ServerState
{
    public RunningServerState(InitializeTrace trace = InitializeTrace.Messages) : base(ServerStateValue.Running)
    {
        Trace = trace;
    }

    protected RunningServerState(InitializeTrace trace = InitializeTrace.Messages, ServerStateValue state = ServerStateValue.Running) : base(state)
    {
        Trace = trace;
    }

    public InitializeTrace Trace { get; }
}
internal record class RunningVerboseServerState : RunningServerState { public RunningVerboseServerState() : base(InitializeTrace.Verbose, ServerStateValue.RunningVerbose) { } }
internal record class RunningTracelessServerState : RunningServerState { public RunningTracelessServerState() : base(InitializeTrace.Off, ServerStateValue.RunningTraceless) { } }
internal record class ShuttingDownServerState : ServerState { public ShuttingDownServerState() : base(ServerStateValue.ShuttingDown) { } }
internal record class ExitingServerState : ServerState
{
    public ExitingServerState(ServerStateValue state) : base(ServerStateValue.Exiting)
    {
        PreviousState = state;
    }

    public ServerStateValue PreviousState { get; }
    public override int ExitCode => PreviousState is ServerStateValue.ShuttingDown or ServerStateValue.Starting ? 0 : 1;
}
