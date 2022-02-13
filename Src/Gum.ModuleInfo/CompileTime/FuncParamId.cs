using Pretune;
using Gum.Collections;

namespace Gum.CompileTime
{
    [AutoConstructor]
    public partial struct FuncParamId
    {
        public FuncParameterKind Kind { get; }
        public SymbolId TypeId { get; }
    }

    public static class FuncParamIdExtensions
    {
        
    }
}