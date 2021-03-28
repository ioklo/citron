using System.Collections.Immutable;

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
        public ImmutableArray<TypeValue> TypeValues { get; }
        public DefaultFuncParamHint(ImmutableArray<TypeValue> typeValues)
        {
            TypeValues = typeValues;
        }
    }
}