namespace RDCore.Server.States;

internal abstract record class ServerState
{
    public static ServerState Starting { get; } = new StartingServerState();
    public static ServerState Initializing { get; } = new InitializingServerState();
    public static ServerState Running { get; } = new RunningServerState();
    public static ServerState ShuttingDown { get; } = new ShuttingDownServerState();
    public static ServerState Exiting { get; } = new ExitingServerState();

    protected ServerState(ServerStateValue value)
    {
        Value = value;
    }

    public ServerStateValue Value { get; }
}

internal record class StartingServerState : ServerState { public StartingServerState() : base(ServerStateValue.Starting) { } }
internal record class InitializingServerState : ServerState { public InitializingServerState() : base(ServerStateValue.Initializing) { } }
internal record class RunningServerState : ServerState { public RunningServerState() : base(ServerStateValue.Running) { } }
internal record class ShuttingDownServerState : ServerState { public ShuttingDownServerState() : base(ServerStateValue.ShuttingDown) { } }
internal record class ExitingServerState : ServerState { public ExitingServerState() : base(ServerStateValue.Exiting) { } }
