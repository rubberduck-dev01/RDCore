using Antlr4.Runtime;
using RDCore.SDK.Model.Source;

namespace RDCore.Parsing.Syntax;

public abstract class VBABaseParserRuleContext : ParserRuleContext
{
    public VBABaseParserRuleContext() : base() => AnchorAt(SourcePosition.Zero);

    public VBABaseParserRuleContext(ParserRuleContext parent, int invokingStateNumber)
        : base(parent, invokingStateNumber) => AnchorAt(SourcePosition.Zero);

    public VBABaseParserRuleContext(ParserRuleContext parent, int invokingStateNumber, SourcePosition anchor)
    : base(parent, invokingStateNumber) => AnchorAt(anchor);

    /// <summary>
    /// Sets the <em>anchor offset</em> that determines where in the document the source "start token" actually begins.
    /// </summary>
    /// <param name="offset">The line/character position offset in the source document</param>
    /// <remarks>
    /// The Line and Character positions are added to both <c>Start</c> and <c>Stop</c> Line and Column positions (respectively) to compute the <c>SourceRange</c>.
    /// </remarks>
    public void AnchorAt(SourcePosition offset) => _offset = offset;
    private SourcePosition _offset;

    /// <summary>
    /// Gets a <see cref="SourceRange"/> representing the line/character start and end positions in the source document,
    /// anchored at an explicitly specified offset; the range is implicitly anchored at <c>L0C0</c> otherwise.
    /// </summary>
    /// <remarks>
    /// 👉 This is the only member that provides a correctly offset/anchored position in the source document
    /// for a <em>source code fragment</em> or partial document; other positional members' values 
    /// would only be safe to surface in the context of a full-document parse.
    /// </remarks>
    public SourceRange SourceRange => new(
            _offset + new SourcePosition(Start?.Line - 1 ?? 0, Start?.Column ?? 0),
            _offset + new SourcePosition(Stop?.Line - 1 ?? 0, Stop?.Column ?? 0));
    public SourceLocation GetSourceLocation(Uri documentUri) => new(documentUri, SourceRange);
}
