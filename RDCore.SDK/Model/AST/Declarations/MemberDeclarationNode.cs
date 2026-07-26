using RDCore.SDK.Model.AST.Abstract;
using RDCore.SDK.Model.Source;
using System.Collections.Immutable;

namespace RDCore.SDK.Model.AST.Declarations;

/// <summary>
/// An AST node representing a member of a module.
/// </summary>
/// <param name="SemanticId">The semantic ID of this AST node.</param>
/// <param name="Location">The source location of this module; the <c>SourceRange</c> is invalid.</param>
/// <param name="Children">An immutable array containing all AST nodes of this module.</param>
/// <param name="Name">The declared identifier name of the member.</param>
/// <param name="MemberKind">Specifies the kind of member.</param>
/// <param name="AccessModifier">An access modifier, if one was supplied.</param>
/// <param name="DeclaredTypeExpression">The <em>declared type</em> expression, if one was supplied.</param>
public record class MemberDeclarationNode(Uri SemanticId, SourceLocation Location, ImmutableArray<BoundNode> Children, string Name, MemberKind MemberKind, AccessModifier AccessModifier = AccessModifier.Implicit, BoundExpression? DeclaredTypeExpression = default)
    : BoundNode(SemanticId, Location, Children);
