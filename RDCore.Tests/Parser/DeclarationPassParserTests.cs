using RDCore.Parsing;
using RDCore.SDK.Model.AST.Abstract;
using RDCore.SDK.Model.AST.Declarations;
using RDCore.SDK.Model.AST.Directives;
using System.Text.Json;

namespace RDCore.Tests.Parser;

[TestClass]
public class DeclarationPassParserTests
{
    [TestMethod]
    public void InvalidContent_ReturnsErrorResult()
    {
        // arrange
        var uri = new Uri("file://C:/RDCore.Tests/Parser/TestModule.bas");
        var sut = new ModuleParser();
        var content = "invalid content";

        // act
        var result = sut.Parse(uri, ModuleType.StdModule, content);

        // assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotEmpty(result.SyntaxErrors);
    }

    [TestMethod]
    public void PrecompilerTrivia_IsIncludedInResult()
    {
        const string content = """
            Option Explicit
            #Const DEBUG = 1
            #If DEBUG Then
            Dim Foo As Long
            #Else
            Dim Foo As Double
            #End If
            """;
        var uri = TestUri.TestModuleUri();
        var sut = new ModuleParser();

        var result = sut.Parse(uri, ModuleType.StdModule, content);

        Assert.IsNotNull(result.SyntaxTree);
        Assert.HasCount(1, result.SyntaxTree.Children.OfType<ModuleOptionDirectiveNode>());
        Assert.HasCount(2, result.SyntaxTree.Children.OfType<VariableDeclarationNode>());

        Assert.HasCount(1, result.PrecompilerTrivia.OfType<PrecompilerConstantDeclarationNode>());
    }

    private const string _testModuleWithDeclarations = """
Option Explicit

#Const DEBUG = 1

Public Sub Test()
    DoSomething 42
    DoSomething 32767
    DoSomething -32768
End Sub

Private Sub DoSomething(ByVal SomeValue As Long)
    Const MultiplierValue = 2

#If DEBUG Then
    Dim OtherValue As Integer
    OtherValue = IIf(SomeValue > 32767, 0, 10)
#EndIf

    On Error GoTo CleanFail
    Debug.Print MultiplierValue * SomeValue + OtherValue

CleanExit:
    Exit Sub

CleanFail:
    Debug.Print Err.Description
    Resume CleanExit
End Sub
""";

    [TestMethod]
    public void ValidContent_ReturnsModuleNodeWithChildren()
    {
        // arrange
        var content = _testModuleWithDeclarations;
        var uri = new Uri("file://C:/RDCore.Tests/Parser/TestModule.bas");
        var sut = new ModuleParser();

        // act
        var result = sut.Parse(uri, ModuleType.StdModule, content);

        // assert
        Assert.IsNotNull(result.SyntaxTree);
        Assert.IsGreaterThan(0, result.SyntaxTree!.Children.Length);
    }

    [TestMethod]
    public void ValidContent_ContainsLocalVariableChildren()
    {
        // arrange
        var content = _testModuleWithDeclarations;
        var uri = new Uri("file://C:/RDCore.Tests/Parser/TestModule.bas");
        var sut = new ModuleParser();

        // act
        var result = sut.Parse(uri, ModuleType.StdModule, content);
        var localVariables = result.SyntaxTree!.Children.OfType<MemberDeclarationNode>()
            .SelectMany(member => member.Children.OfType<VariableDeclarationNode>())
            .ToArray();

        // assert
        Assert.IsGreaterThan(0, localVariables.Length);
    }

    [TestMethod]
    public void ValidContent_ContainsLocalConstChildren()
    {
        // arrange
        var content = _testModuleWithDeclarations;
        var uri = new Uri("file://C:/RDCore.Tests/Parser/TestModule.bas");
        var sut = new ModuleParser();

        // act
        var result = sut.Parse(uri, ModuleType.StdModule, content);
        var localConstants = result.SyntaxTree!.Children.OfType<MemberDeclarationNode>()
            .SelectMany(member => member.Children.OfType<ConstantDeclarationNode>())
            .ToArray();

        // assert
        Assert.IsGreaterThan(0, localConstants.Length);
    }

    [TestMethod]
    public void ValidContent_ContainsLabelChildren()
    {
        // arrange
        var content = _testModuleWithDeclarations;
        var uri = new Uri("file://C:/RDCore.Tests/Parser/TestModule.bas");
        var sut = new ModuleParser();

        // act
        var result = sut.Parse(uri, ModuleType.StdModule, content);
        var lineLabels = result.SyntaxTree!.Children.OfType<MemberDeclarationNode>()
            .SelectMany(member => member.Children.OfType<LineLabelNode>())
            .ToArray();

        // assert
        Assert.IsGreaterThan(0, lineLabels.Length);
    }

    [TestMethod]
    public void SyntaxTree_DeserializesToSyntaxNode()
    {
        var content = _testModuleWithDeclarations;
        var uri = new Uri("file://C:/RDCore.Tests/Parser/TestModule.bas");
        var sut = new ModuleParser();

        // act
        var result = sut.Parse(uri, ModuleType.StdModule, content);
        if (result.IsSuccess)
        {
            var ast = result.SyntaxTree!;
            var json = JsonSerializer.Serialize(ast);
            var deserialized = JsonSerializer.Deserialize<ModuleNode>(json);

            Assert.AreEqual(ast.Identity, deserialized?.Identity);
            Assert.AreSequenceEqual(ast.Children.Select(node => node.Identity), deserialized?.Children.Select(node => node.Identity));
        }
        else
        {
            Assert.Inconclusive(result.SyntaxErrors[0]!.Description);
        }
    }
}
