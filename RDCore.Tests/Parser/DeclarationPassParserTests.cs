using NSubstitute;
using NSubstitute.ClearExtensions;
using RDCore.Parsing;
using RDCore.Parsing.PreProcessing;
using RDCore.Parsing.PreProcessing.Legacy;
using RDCore.SDK.Model.AST.Abstract;
using RDCore.SDK.Model.AST.Declarations;
using System.Text;
using System.Text.Json;

namespace RDCore.Tests;

[TestClass]
public class DeclarationPassParserTests
{
    private readonly ICompilationArgumentsProvider _compilationArgsProvider = Substitute.For<ICompilationArgumentsProvider>();

    private readonly VBAPreprocessorParser _preprocessorParser = new();

    [TestInitialize]
    public void InitializeFileMock()
    {
        _compilationArgsProvider.PredefinedCompilationConstants.Returns(provider => new VBAPredefinedCompilationConstants(vbVersion: 7));
        _compilationArgsProvider.UserDefinedCompilationArguments(Arg.Any<Uri>()).Returns(provider => []);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _compilationArgsProvider.ClearSubstitute();
    }

    [TestMethod]
    public void InvalidContent_ReturnsErrorResult()
    {
        // arrange
        var uri = new Uri("file://C:/RDCore.Tests/Parser/TestModule.bas");
        var sut = new ModuleParser();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("invalid content"));

        // act
        var result = sut.Parse(uri, ModuleType.StdModule, stream);

        // assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.SyntaxError);
    }

    private const string _testModuleWithDeclarations = """
Option Explicit

Private SomeField As Double

Public Sub Test()
    DoSomething 42
    DoSomething 32767
    DoSomething -32768
End Sub

Private Sub DoSomething(ByVal SomeValue As Long)
    Const MultiplierValue = 2

    Dim OtherValue As Integer
    OtherValue = IIf(SomeValue > 32767, 0, 10)

    On Error GoTo CleanFail
    Debug.Print Multiplier * SomeValue + OtherValue

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
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // act
        var result = sut.Parse(uri, ModuleType.StdModule, stream);

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
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // act
        var result = sut.Parse(uri, ModuleType.StdModule, stream);
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
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // act
        var result = sut.Parse(uri, ModuleType.StdModule, stream);
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
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // act
        var result = sut.Parse(uri, ModuleType.StdModule, stream);
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
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // act
        var result = sut.Parse(uri, ModuleType.StdModule, stream);
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
            Assert.Inconclusive();
        }
    }
}
