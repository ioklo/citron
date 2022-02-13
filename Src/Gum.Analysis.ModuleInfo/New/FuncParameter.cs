using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using M = Gum.CompileTime;

namespace Citron.Analysis
{
    // value
    [AutoConstructor]
    public partial struct FuncParameter
    {
        public M.FuncParameterKind Kind { get; }
        public ITypeSymbol Type { get; }
        public M.Name Name { get; }        

        public FuncParameter Apply(TypeEnv typeEnv)
        {
            var appliedType = Type.Apply(typeEnv);
            return new FuncParameter(Kind, appliedType, Name);
        }
    }

    public static class FuncParameterExtensions
    {
        public static ImmutableArray<M.FuncParamId> MakeFuncParamIds(this ImmutableArray<FuncParameter> funcParams)
        {
            var builder = ImmutableArray.CreateBuilder<M.FuncParamId>(funcParams.Length);

            foreach (var funcParam in funcParams)
            {
                var kind = funcParam.Kind;
                var typeId = funcParam.Type.GetSymbolId();

                builder.Add(new M.FuncParamId(kind, typeId));
            }

            return builder.MoveToImmutable();
        }
    }
}