using Citron.Collections;
using Citron.Infra;
using Citron.Module;
using Citron.Symbol;
using System;
using System.Diagnostics;

namespace Citron.Test.Misc
{
    internal class FuncRetParamsBuilderComponent<TBuilder>
    {
        TBuilder builder;

        IHolder<FuncReturn>? funcRetHolder;
        IHolder<ImmutableArray<FuncParameter>>? funcParamsHolder;
        ImmutableArray<FuncParameter>.Builder funcParamsBuilder;

        internal FuncRetParamsBuilderComponent(TBuilder builder)
        {
            this.builder = builder;

            this.funcRetHolder = null;
            this.funcParamsHolder = null;
            this.funcParamsBuilder = ImmutableArray.CreateBuilder<FuncParameter>();
        }

        public TBuilder FuncReturn(bool isRef, ITypeSymbol type)
        {
            Debug.Assert(this.funcRetHolder == null);
            this.funcRetHolder = new FuncReturn(isRef, type).ToHolder();
            return builder;
        }

        public TBuilder FuncReturnHolder(out Holder<FuncReturn> funcRetHolder)
        {
            Debug.Assert(this.funcRetHolder == null);
            funcRetHolder = new Holder<FuncReturn>();
            this.funcRetHolder = funcRetHolder;
            return builder;
        }

        public TBuilder FuncParameter(FuncParameterKind kind, ITypeSymbol type, Name name)
        {
            Debug.Assert(this.funcParamsHolder == null);

            var parameter = new FuncParameter(kind, type, name);
            funcParamsBuilder.Add(parameter);
            return builder;
        }

        public TBuilder FuncParametersHolder(out Holder<ImmutableArray<FuncParameter>> funcParamsHolder)
        {
            Debug.Assert(this.funcParamsBuilder.Count == 0);
            Debug.Assert(this.funcParamsHolder == null);

            funcParamsHolder = new Holder<ImmutableArray<FuncParameter>>();
            this.funcParamsHolder = funcParamsHolder;

            return builder;
        }

        // constructor의 경우, Return이 없으므로
        public IHolder<ImmutableArray<FuncParameter>> GetParamsHolderOnly()
        {
            // funcParamsHolder가 있을 경우, Count는 0이 아니어야 한다
            Debug.Assert(funcParamsHolder != null && funcParamsBuilder.Count == 0);

            if (funcParamsHolder == null)
                funcParamsHolder = funcParamsBuilder.ToImmutable().ToHolder();

            return funcParamsHolder;
        }

        public (IHolder<FuncReturn> RetHolder, IHolder<ImmutableArray<FuncParameter>> ParamsHolder) Get()
        {
            Debug.Assert(funcRetHolder != null);

            // funcParamsHolder가 있을 경우, Count는 0이 아니어야 한다
            Debug.Assert(funcParamsHolder != null && funcParamsBuilder.Count == 0);

            if (funcParamsHolder == null)
                funcParamsHolder = funcParamsBuilder.ToImmutable().ToHolder();

            return (funcRetHolder, funcParamsHolder);
        }
    }
}