using System.Diagnostics;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace RDCore.Parsing;

public enum VBCompileErrorId
{
    ForbiddenWithOptionStrict = 9000,
    AmbiguousName,
    DuplicateDeclaration,
    InvalidUseOfObject,
    InvalidParamArrayUse,
    InvalidReDim,
    ExpectedArray,
    ExpectedIdentifier,
    LabelNotDefined,
    TypeMismatch,
    UserDefinedTypeNotDefined,
    ExitDoNotWithinDoLoop,
    ExitForNotWithinForNext,
    ExitFunctionNotAllowedInSubOrProperty,
    ExitPropertyNotAllowedInSubOrFunction,
}

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class VBCompileErrorException : ApplicationException
{
    public VBCompileErrorException(Range location, VBCompileErrorId id, string message, string? verbose = null)
        : base($"Compile error: {message}")
    {
        VBCompileErrorId = id;
        Location = location;
        Verbose = verbose;
    }

    private string DebuggerDisplay => $"[{VBCompileErrorId.ToDiagnosticCode()}] {Message}{(Verbose is null ? string.Empty : " | " + Verbose)}";

    #region Classic-VB compile-time errors
    // NOTE: VB compile errors are just messages, ID is made up.
    public static VBCompileErrorException InvalidUseOfObject(Range location, string? verbose = null) => new(location, VBCompileErrorId.InvalidUseOfObject, "Invalid use of object", verbose);
    public static VBCompileErrorException InvalidParamArrayUse(Range location, string? verbose = null) => new(location, VBCompileErrorId.InvalidParamArrayUse, "Invalid ParamArray use", verbose);
    public static VBCompileErrorException InvalidReDim(Range location, string? verbose = null) => new(location, VBCompileErrorId.InvalidReDim, "Invalid ReDim", verbose);
    public static VBCompileErrorException ExpectedArray(Range location, string? verbose = null) => new(location, VBCompileErrorId.ExpectedArray, "Expected array", verbose);
    public static VBCompileErrorException ExpectedIdentifier(Range location, string? verbose = null) => new(location, VBCompileErrorId.ExpectedIdentifier, "Expected identifier", verbose);
    public static VBCompileErrorException LabelNotDefined(Range location, string? verbose = null) => new(location, VBCompileErrorId.LabelNotDefined, "Label not defined", verbose);
    public static VBCompileErrorException TypeMismatch(Range location, string? verbose = null) => new(location, VBCompileErrorId.TypeMismatch, "Type mismatch", verbose);
    public static VBCompileErrorException UserDefinedTypeNotDefined(Range location, string? verbose = null) => new(location, VBCompileErrorId.UserDefinedTypeNotDefined, "User-defined type not defined", verbose);
    public static VBCompileErrorException ExitDoNotWithinDoLoop(Range location, string? verbose = null) => new(location, VBCompileErrorId.ExitDoNotWithinDoLoop, "Exit Do not within Do...Loop", verbose);
    public static VBCompileErrorException ExitForNotWithinForNext(Range location, string? verbose = null) => new(location, VBCompileErrorId.ExitForNotWithinForNext, "Exit For not within For...Next", verbose);
    public static VBCompileErrorException ExitFunctionNotAllowedInSubOrProperty(Range location, string? verbose = null) => new(location, VBCompileErrorId.ExitFunctionNotAllowedInSubOrProperty, "Exit Function not allowed in Sub or Property", verbose);
    public static VBCompileErrorException AmbiguousName(Range location, string? verbose = null) => new(location, VBCompileErrorId.AmbiguousName, $"Ambiguous name detected: {symbol.Name}", verbose);
    public static VBCompileErrorException DuplicateDeclaration(Range location, string? verbose = null) => new(location, VBCompileErrorId.DuplicateDeclaration, $"Duplicate declaration in current scope", verbose);

    #endregion

    public VBCompileErrorId VBCompileErrorId { get; }
    public Range Location { get; }
    public string? Verbose { get; }
}
