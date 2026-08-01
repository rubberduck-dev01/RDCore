using Antlr4.Runtime;

namespace RDCore.Parsing.PreProcessing;

public interface ITokenStreamPreprocessor
{
    CommonTokenStream? PreprocessTokenStream(Uri uri, CommonTokenStream tokenStream);
}
