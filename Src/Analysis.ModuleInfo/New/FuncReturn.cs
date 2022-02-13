using Pretune;

namespace Citron.Analysis
{   
    [AutoConstructor]
    public partial struct FuncReturn
    {
        public bool IsRef { get; }
        public ITypeSymbol Type { get; }

        public FuncReturn Apply(TypeEnv typeEnv)
        {
            var appliedType = Type.Apply(typeEnv);
            return new FuncReturn(IsRef, appliedType);
        }
    }
}