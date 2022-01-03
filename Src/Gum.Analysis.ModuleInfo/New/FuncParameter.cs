using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.Analysis
{
    public enum FuncParameterKind
    {
        Default,
        Params,
        Ref,        
    }

    public static class FuncParameterKindExtensions
    {
        public static M.ParamKind ToMParamKind(this FuncParameterKind kind)
        {
            return kind switch
            {
                FuncParameterKind.Default => M.ParamKind.Default,
                FuncParameterKind.Ref => M.ParamKind.Ref,
                FuncParameterKind.Params => M.ParamKind.Params,
                _ => throw new UnreachableCodeException()
            };
        }
        
        public static R.ParamKind ToRParamKind(this FuncParameterKind kind)
        {
            return kind switch
            {
                FuncParameterKind.Default => R.ParamKind.Default,
                FuncParameterKind.Ref => R.ParamKind.Ref,
                FuncParameterKind.Params => R.ParamKind.Params,
                _ => throw new UnreachableCodeException()
            };
        }
    }

    // value
    [AutoConstructor]
    public partial struct FuncParameter
    {
        public FuncParameterKind Kind { get; }
        public ITypeSymbolNode Type { get; }
        public M.Name Name { get; }

        public M.Param GetMParam()
        {
            return new M.Param(Kind.ToMParamKind(), Type.GetMTypeId(), Name);
        }

        public FuncParameter Apply(TypeEnv typeEnv)
        {
            var appliedType = Type.Apply(typeEnv);
            return new FuncParameter(Kind, appliedType, Name);
        }

        public R.ParamHashEntry GetParamHashEntry()
        {
            return new R.ParamHashEntry(Kind.ToRParamKind(), Type.MakeRPath());
        }
    }    

    public static class FuncParametersExtentions
    {
        public static ImmutableArray<R.ParamHashEntry> MakeParamHashEntries(this ImmutableArray<FuncParameter> parameters)
        {
            var entries = ImmutableArray.CreateBuilder<R.ParamHashEntry>(parameters.Length);
            foreach (var parameter in parameters)
            {
                var paramHashEntry = parameter.GetParamHashEntry();
                entries.Add(paramHashEntry);
            }

            return entries.MoveToImmutable();
        }

        public static M.ParamTypes MakeMParamTypes(this ImmutableArray<FuncParameter> funcParams)
        {
            var builder = ImmutableArray.CreateBuilder<M.ParamKindAndType>(funcParams.Length);

            foreach (var funcParam in funcParams)
            {
                var kind = funcParam.Kind.ToMParamKind();
                var mtypeid = funcParam.Type.GetMTypeId();

                builder.Add(new M.ParamKindAndType(kind, mtypeid));
            }

            return new M.ParamTypes(builder.MoveToImmutable());
        }
    }
}