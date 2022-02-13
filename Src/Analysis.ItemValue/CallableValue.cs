using Citron.Collections;
using Citron.Infra;
using M = Citron.CompileTime;

namespace Citron.Analysis
{
    public abstract class CallableValue : ItemValue
    {
        protected abstract ITypeSymbol MakeTypeValueByMType(M.TypeId type);
        public abstract ImmutableArray<M.Param> GetParameters();

        // class X<T> { void Func<U>(T t, ref U u, int x); }
        // X<int>.F<bool> => (int, ref bool, int)
        public ImmutableArray<ParamInfo> GetParamInfos()
        {
            var typeEnv = MakeTypeEnv();
            var parameters = GetParameters();

            var builder = ImmutableArray.CreateBuilder<ParamInfo>(parameters.Length);
            foreach (var paramInfo in parameters)
            {
                var paramTypeValue = MakeTypeValueByMType(paramInfo.Type);
                var appliedParamTypeValue = paramTypeValue.Apply(typeEnv);

                var paramKind = paramInfo.Kind switch
                {
                    M.ParamKind.Default => R.ParamKind.Default,
                    M.ParamKind.Ref => R.ParamKind.Ref,
                    M.ParamKind.Params => R.ParamKind.Params,
                    _ => throw new UnreachableCodeException()
                };

                builder.Add(new ParamInfo(paramKind, appliedParamTypeValue));
            }

            return builder.MoveToImmutable();
        }
    }
}