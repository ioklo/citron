using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using Citron.Module;

namespace Citron.Symbol
{
    // value
    [AutoConstructor]
    public partial struct FuncParameter
    {
        public FuncParameterKind Kind { get; }
        public ITypeSymbol Type { get; }
        public Name Name { get; }        

        public FuncParameter Apply(TypeEnv typeEnv)
        {
            var appliedType = Type.Apply(typeEnv);
            return new FuncParameter(Kind, appliedType, Name);
        }
    }

    public static class FuncParameterExtensions
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