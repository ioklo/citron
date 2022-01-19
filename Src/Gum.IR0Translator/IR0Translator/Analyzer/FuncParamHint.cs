using Gum.Analysis;
using Gum.Collections;

namespace Gum.IR0Translator
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