using RDCore.Parsing.Model;
using RDCore.Parsing.Model.Expressions.Operators;
using RDCore.Parsing.Model.Symbols;
using RDCore.Parsing.Model.Types;
using RDCore.Server.ProtocolExtensions;
using System.Collections.Concurrent;

namespace RDCore.Runtime;

internal class SymbolTable { /*TODO*/}

internal static class GlobalSymbols
{
    // operators
    public static readonly AdditionOperatorSymbol Addition = new();
    //public static readonly SubtractionOperatorSymbol Subtraction = new();
    //public static readonly ConcatenationOperatorSymbol Concatenation = new();

    // constants
    public static readonly StaticSymbol Empty = new(Tokens.Empty, SymbolKindExt.Constant, VBEmptyType.TypeInfo);
    public static readonly StaticSymbol Nothing = new(Tokens.Nothing, SymbolKindExt.Constant, VBObjectType.TypeInfo);
    public static readonly StaticSymbol Null = new(Tokens.Null, SymbolKindExt.Constant, VBNullType.TypeInfo);

    public static void Initialize(ConcurrentDictionary<Uri, Symbol> index)
    {
        if (!index.IsEmpty)
        {
            throw new InvalidOperationException("The specified index dictionary is already initialized; index is expected to be empty.");
        }

        index[Addition.Uri] = Addition;
        index[Null.Uri] = Null;
        index[Empty.Uri] = Empty;
    }
}