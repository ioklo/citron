using Citron.Infra;

namespace Citron.Analysis
{
    static class SymbolQueryResultExtensions
    {
        public static IdentifierResult.Error ToErrorIdentifierResult(this SymbolQueryResult.Error errorResult)
        {
            switch (errorResult)
            {
                case SymbolQueryResult.Error.MultipleCandidates:
                    return IdentifierResult.Error.MultipleCandiates;

                case SymbolQueryResult.Error.VarWithTypeArg:
                    return IdentifierResult.Error.VarWithTypeArg;
            }

            throw new UnreachableCodeException();
        }
    }
}
