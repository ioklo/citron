using M = Gum.CompileTime;
using Pretune;
using Gum.Collections;

namespace Gum.Analysis
{
    [AutoConstructor]
    public partial struct FuncParamId
    {
        public FuncParameterKind Kind { get; }
        public SymbolId TypeId { get; }
    }

    public static class FuncParamIdExtensions
    {
        public static ImmutableArray<FuncParamId> MakeFuncParamIds(this ImmutableArray<FuncParameter> funcParams)
        {
            var builder = ImmutableArray.CreateBuilder<FuncParamId>(funcParams.Length);

            foreach (var funcParam in funcParams)
            {
                var kind = funcParam.Kind;
                var typeId = funcParam.Type.GetSymbolId();

                builder.Add(new FuncParamId(kind, typeId));
            }

            return builder.MoveToImmutable();
        }
    }
}