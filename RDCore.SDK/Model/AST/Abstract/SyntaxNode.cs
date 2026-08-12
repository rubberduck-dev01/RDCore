using RDCore.SDK.Model.AST.Directives;
using RDCore.SDK.Model.Source;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace RDCore.SDK.Model.AST.Abstract;

/// <summary>
/// Represents an identifier that is unique for every node of an AST.
/// </summary>
/// <remarks>
/// Encodes the node's position within the AST.
/// </remarks>
/// <param name="Lineage">An array of successive child index values in the syntax tree.</param>
public record struct SyntaxNodeId(ImmutableArray<int> Lineage)
{
    public override string ToString() => string.Join('/', Lineage);
}

/// <summary>
/// A node in the <em>abstract syntax tree</em> (AST).
/// </summary>
/// <remarks>
/// This is the base abstract node type every AST node is derived from.
/// </remarks>
/// <param name="Identity">A unique identifier for this specific syntax node.</param>
/// <param name="SourceLocation">The document location (<c>Uri</c>+<c>Range</c>) of this node.</param>
[JsonDerivedType(typeof(AttributeDirectiveNode))]
public abstract record class SyntaxNode(SyntaxNodeId Identity, SourceLocation SourceLocation, ImmutableArray<SyntaxNode> Children)
{
    /// <summary>
    /// A unique identifier encoding the node's position in the syntax tree.
    /// </summary>
    public SyntaxNodeId Identity { get; init; } = Identity;
    /// <summary>
    /// The location of the node in the source document.
    /// </summary>
    public SourceLocation SourceLocation { get; init; } = SourceLocation;
    /// <summary>
    /// The child syntax nodes.
    /// </summary>
    public ImmutableArray<SyntaxNode> Children { get; init; } = Children;
}