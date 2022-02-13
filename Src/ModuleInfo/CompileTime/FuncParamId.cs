using Pretune;
using Citron.Collections;

namespace Citron.CompileTime
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