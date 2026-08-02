using RDCore.SDK.Model.AST.Abstract;
using RDCore.SDK.Model.AST.Expressions;
using RDCore.SDK.Model.Source;

namespace RDCore.SDK.Model.AST.Declarations;

/// <summary>
/// An AST node representing a local variable (child of a member node).
/// </summary>
/// <param name="SemanticId">The semantic ID of this AST node.</param>
/// <param name="Location">The source location of this module; the <c>SourceRange</c> is invalid.</param>
/// <param name="Name">The declared identifier name of the member.</param>
/// <param name="AccessModifier">The access modifier, if one was supplied.</param>
/// <param name="DeclaredTypeExpression">The <em>declared type</em> expression, if one was supplied.</param>
/// <param name="TypeHint">The <em>type hint</em> token, if one was supplied.</param>
public record class VariableDeclarationNode(Uri SemanticId, SourceLocation Location, string Name, AccessModifier AccessModifier = AccessModifier.Implicit, VBAsTypeExpression? DeclaredTypeExpression = default, string? TypeHint = default) 
    : SyntaxNode(SemanticId, Location, DeclaredTypeExpression is ExpressionNode node ? [node] : []);
