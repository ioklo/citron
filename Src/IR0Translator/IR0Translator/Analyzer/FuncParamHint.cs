using Citron.Analysis;
using Citron.Collections;

namespace Citron.IR0Translator
{
    abstract class FuncParamHint
    {

    }

    class NoneFuncParamHint : FuncParamHint
    {
        public static readonly NoneFuncParamHint Instance = new NoneFuncParamHint();
        NoneFuncParamHint() { }
    }

    class DefaultFuncParamHint : FuncParamHint
    {
        public ImmutableArray<ITypeSymbol> TypeValues { get; }
        public DefaultFuncParamHint(ImmutableArray<ITypeSymbol> typeValues)
        {
            TypeValues = typeValues;
        }
    }
}