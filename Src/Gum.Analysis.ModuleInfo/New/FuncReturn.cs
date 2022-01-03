using Pretune;

namespace Gum.Analysis
{
    [AutoConstructor]
    public partial struct FuncReturn
    {
        public bool IsRef { get; }
        public ITypeSymbolNode Type { get; }

        public FuncReturn Apply(TypeEnv typeEnv)
        {
            var appliedType = Type.Apply(typeEnv);
            return new FuncReturn(IsRef, appliedType);
        }
    }
}