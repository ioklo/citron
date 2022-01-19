using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using M = Gum.CompileTime;

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
        public static FuncParameterKind MakeFuncParameterKind(this M.ParamKind kind)
        {
            return kind switch
            {
                M.ParamKind.Default => FuncParameterKind.Default,
                M.ParamKind.Ref => FuncParameterKind.Ref,
                M.ParamKind.Params => FuncParameterKind.Params,
                _ => throw new UnreachableCodeException()
            };
        }

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
        
        
    }

    // value
    [AutoConstructor]
    public partial struct FuncParameter
    {
        public FuncParameterKind Kind { get; }
        public ITypeSymbol Type { get; }
        public M.Name Name { get; }        

        public FuncParameter Apply(TypeEnv typeEnv)
        {
            var appliedType = Type.Apply(typeEnv);
            return new FuncParameter(Kind, appliedType, Name);
        }
    }
}