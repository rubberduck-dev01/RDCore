using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace RDCore.Server;

public class SupportedLanguage
{
    // RDCore LSP server supports VBA source files:
    public static readonly SupportedLanguage VBA = new("vba", "Microsoft Visual Basic for Applications", "*.bas", "*.cls", "*.frm", "*.doccls");

    private SupportedLanguage(string id, string name, params string[] fileTypes)
    {
        Id = id;
        Name = name;
        FileTypes = fileTypes;
    }

    public string Id { get; }
    public string Name { get; }
    public string[] FileTypes { get; }

    public string FilterString => string.Join(";", FileTypes.Select(fileType => $"**/{fileType}").ToArray());
    public TextDocumentSelector ToTextDocumentSelector() => new(
        new TextDocumentFilter
        {
            Language = Id,
            Pattern = FilterString,
        });
}