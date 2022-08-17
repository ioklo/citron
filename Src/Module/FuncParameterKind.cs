using Citron.Infra;

namespace Citron.Module
{
    public enum FuncParameterKind
    {
        Default,
        Params,
        Ref,        
    }

    public static class FuncParameterKindExtensions
    {
        public static FuncParameterKind MakeFuncParameterKind(this ParamKind kind)
        {
            return kind switch
            {
                ParamKind.Default => FuncParameterKind.Default,
                ParamKind.Ref => FuncParameterKind.Ref,
                ParamKind.Params => FuncParameterKind.Params,
                _ => throw new UnreachableCodeException()
            };
        }

        public static ParamKind ToMParamKind(this FuncParameterKind kind)
        {
            return kind switch
            {
                FuncParameterKind.Default => ParamKind.Default,
                FuncParameterKind.Ref => ParamKind.Ref,
                FuncParameterKind.Params => ParamKind.Params,
                _ => throw new UnreachableCodeException()
            };
        }


    }
}