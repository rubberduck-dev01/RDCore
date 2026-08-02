using RDCore.SDK.Model.Source;
using System.Collections.Immutable;

namespace RDCore.SDK.Model.AST.Abstract;


/// <summary>
/// A <see cref="SyntaxNode"/> representing a <em>module directive</em>, which is module metadata that is neither typed, nor executable.
/// </summary>
/// <param name="SemanticId">A semantic <c>Uri</c> uniquely identifying this specific node.</param>
/// <param name="SourceLocation">The document location (<c>Uri</c>+<c>Range</c>) of the bound expression.</param>
public abstract record class DirectiveNode(Uri SemanticId, SourceLocation SourceLocation, ImmutableArray<SyntaxNode> Children) 
    : SyntaxNode(SemanticId, SourceLocation, Children);

/// <summary>
/// A <em>syntax node</em> representing a <em>line label directive</em>, which is procedure metadata that defines a named symbol for various jump targets.
/// </summary>
/// <param name="SemanticId">A semantic <c>Uri</c> uniquely identifying this specific node.</param>
/// <param name="SourceLocation">The document location (<c>Uri</c>+<c>Range</c>) of the bound expression.</param>
/// <param name="Name">The label name.</param>
public record class LineLabelNode(Uri SemanticId, SourceLocation SourceLocation, string Name) 
    : DirectiveNode(SemanticId, SourceLocation, []);
/// <summary>
/// A <em>line label directive</em> that represents a line number, which is procedure metadata that defines a named symbol for various jump targets.
/// </summary>
/// <param name="SemanticId">A semantic <c>Uri</c> uniquely identifying this specific node.</param>
/// <param name="SourceLocation">The document location (<c>Uri</c>+<c>Range</c>) of the bound expression.</param>
/// <param name="Number">The label name.</param>
/// <remarks>
/// The string representation of the number is the label name.
/// </remarks>
public record class LineNumberNode(Uri SemanticId, SourceLocation SourceLocation, int Number)
    : LineLabelNode(SemanticId, SourceLocation, Number.ToString());