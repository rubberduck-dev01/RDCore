using NSubstitute;
using NSubstitute.ClearExtensions;
using RDCore.Parsing;
using RDCore.Parsing.PreProcessing;
using RDCore.Parsing.PreProcessing.Legacy;
using RDCore.SDK.Model.AST.Declarations;
using System.IO.Abstractions;
using System.Text;
using System.Text.Unicode;

namespace RDCore.Tests;

[TestClass]
public class DeclarationPassParserTests
{
    private readonly ICompilationArgumentsProvider _compilationArgsProvider = Substitute.For<ICompilationArgumentsProvider>();

    private readonly VBAPreprocessorParser _preprocessorParser = new();
    private ITokenStreamPreprocessor? _preprocessor;

    [TestInitialize]
    public void InitializeFileMock()
    {
        _compilationArgsProvider.PredefinedCompilationConstants.Returns(provider => new VBAPredefinedCompilationConstants(vbVersion: 7));
        _compilationArgsProvider.UserDefinedCompilationArguments(Arg.Any<Uri>()).Returns(provider => []);
        _preprocessor = new VBAPreprocessor(_preprocessorParser, _compilationArgsProvider);
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
        var sut = new ModuleParser(_preprocessor!);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("invalid content"));

        // act
        var result = sut.Parse(uri, ModuleType.StdModule, stream);

        // assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.SyntaxError);
    }

    [TestMethod]
    public void ValidContent_ReturnsModuleNodeWithChildren()
    {
        // arrange
        var content = """
Option Explicit

Public Sub DoSomething(ByVal SomeValue As Long)
    Debug.Print SomeValue
End Sub
""";
            

        var uri = new Uri("file://C:/RDCore.Tests/Parser/TestModule.bas");
        var sut = new ModuleParser(_preprocessor!);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // act
        var result = sut.Parse(uri, ModuleType.StdModule, stream);

        // assert
        Assert.IsNotNull(result.SyntaxTree);
        Assert.IsGreaterThan(0, result.SyntaxTree!.Children.Length);
    }
}
