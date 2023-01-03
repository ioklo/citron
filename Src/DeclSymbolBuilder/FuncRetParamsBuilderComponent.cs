using Citron.Collections;
using Citron.Infra;
using Citron.Module;
using Citron.Symbol;
using System;
using System.Diagnostics;

namespace Citron.Test
{
    // closed state
    internal partial class FuncRetParamsBuilderComponent<TBuilder>
    {
        // 함수 파라미터 입력에 관한 상태
        abstract record class FuncParamState;

        // 1. 아무것도 안한 경우 (처음), 직접 입력하거나 Holder를 발급 받거나, 파라미터가 없는 상태로 둘수 있다
        record class NoneFuncParamState : FuncParamState;

        // 2. Holder를 발급 받은 경우, 직접 입력했었으면 에러.
        record class HolderFuncParamState(ImmutableArray<FuncParamId> ParamIds, IHolder<ImmutableArray<FuncParameter>> Holder) : FuncParamState;

        // 3. 직접 입력하는 경우, Holder를 발급받았으면 에러.
        record class EagerFuncParamState(ImmutableArray<FuncParameter>.Builder Builder) : FuncParamState;
    }

    internal partial class FuncRetParamsBuilderComponent<TBuilder>
    {
        TBuilder builder;        

        IHolder<FuncReturn>? funcRetHolder;
        FuncParamState funcParamState;

        internal FuncRetParamsBuilderComponent(TBuilder builder)
        {
            this.builder = builder;

            this.funcRetHolder = null;
            this.funcParamState = new NoneFuncParamState();
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
        
        // FuncParameter를 명시합니다
        public TBuilder FuncParameter(FuncParameterKind kind, ITypeSymbol type, Name name)
        {
            switch(funcParamState)
            {
                case NoneFuncParamState:
                    {
                        var builder = ImmutableArray.CreateBuilder<FuncParameter>();
                        funcParamState = new EagerFuncParamState(builder);

                        var parameter = new FuncParameter(kind, type, name);
                        builder.Add(parameter);
                        break;
                    }

                case HolderFuncParamState:
                    Debug.Assert(false);
                    break;

                case EagerFuncParamState explicitState:
                    {
                        var parameter = new FuncParameter(kind, type, name);
                        explicitState.Builder.Add(parameter);
                        break;
                    }

                default:
                    throw new UnreachableCodeException();
            }

            return builder;
        }

        // Parameter자체는 나중에 제공하더라도, 파라미터 id는 먼저 제공해야 한다
        public TBuilder FuncParametersHolder(ImmutableArray<FuncParamId> paramIds, out Holder<ImmutableArray<FuncParameter>> funcParamsHolder)
        {
            if (funcParamState is not NoneFuncParamState)
                Debug.Assert(false);            

            funcParamsHolder = new Holder<ImmutableArray<FuncParameter>>();
            funcParamState = new HolderFuncParamState(paramIds, funcParamsHolder);

            return builder;
        }

        // constructor의 경우, Return이 없으므로
        public IHolder<ImmutableArray<FuncParameter>> GetParamsOnly()
        {
            switch(funcParamState)
            {
                case NoneFuncParamState:
                    return new Holder<ImmutableArray<FuncParameter>>(default);

                case HolderFuncParamState holderState:
                    return holderState.Holder;

                case EagerFuncParamState explicitState:
                    {
                        var funcParams = explicitState.Builder.ToImmutable();
                        return funcParams.ToHolder();
                    }

                default:
                    throw new UnreachableCodeException();
            }
        }

        public (IHolder<FuncReturn> RetHolder, IHolder<ImmutableArray<FuncParameter>> ParamsHolder) Get()
        {
            Debug.Assert(funcRetHolder != null);
            var paramsHolder = GetParamsOnly();

            return (funcRetHolder, paramsHolder);
        }
    }
}