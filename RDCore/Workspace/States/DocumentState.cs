namespace RDCore.Workspace.States;

internal abstract record class DocumentState
{
    public static DocumentState Unloaded { get; } = new UnloadedDocumentState();
    public static DocumentState Loaded { get; } = new LoadedDocumentState();
    public static DocumentState Missing { get; } = new MissingDocumentState();
    public static DocumentState LoadError { get; } = new LoadErrorDocumentState();
    public static DocumentState Opened { get; } = new OpenedDocumentState();

    protected DocumentState(DocumentStateValue value)
    {
        Value = value;
    }
    public DocumentStateValue Value { get; }
}

internal record class UnloadedDocumentState : DocumentState { public UnloadedDocumentState() : base(DocumentStateValue.Unloaded) { } }
internal record class LoadedDocumentState : DocumentState { public LoadedDocumentState() : base(DocumentStateValue.Loaded) { } }
internal record class MissingDocumentState : DocumentState { public MissingDocumentState() : base(DocumentStateValue.Missing) { } }
internal record class LoadErrorDocumentState : DocumentState { public LoadErrorDocumentState() : base(DocumentStateValue.LoadError) { } }
internal record class OpenedDocumentState : DocumentState { public OpenedDocumentState() : base(DocumentStateValue.Opened) { } }
