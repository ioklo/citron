using Pretune;
using Citron.Collections;

namespace Citron.Symbol
{
    [AutoConstructor]
    public partial struct FuncParamId
    {
        public FuncParameterKind Kind { get; }
        public FuncParamTypeId TypeId { get; }
    }

    public abstract record class FuncParamTypeId
    {
        public record Symbol(SymbolId Id) : FuncParamTypeId;
        public record Tuple(ImmutableArray<FuncParamTypeId> MemberTypeIds) : FuncParamTypeId;
        public record Nullable(FuncParamTypeId InnerTypeId) : FuncParamTypeId;
        public record Void() : FuncParamTypeId;
        public record TypeVar(int index) : FuncParamTypeId;
    }

    public static class FuncParamIdExtensions
    {
        public static ImmutableArray<FuncParamId> MakeFuncParamIds(this ImmutableArray<FuncParameter> funcParams)
        {
            var builder = ImmutableArray.CreateBuilder<FuncParamId>(funcParams.Length);

            foreach (var funcParam in funcParams)
            {
                var kind = funcParam.Kind;
                var typeId = funcParam.Type.MakeFuncParamTypeId();

                builder.Add(new FuncParamId(kind, typeId));
            }

            return builder.MoveToImmutable();
        }
    }
}