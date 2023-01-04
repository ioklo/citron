using System;
using System.Collections.Generic;

using Citron.Collections;
using Citron.Module;
using Citron.Symbol;

namespace Citron.Test
{
    public class GlobalFuncBuilderComponent<TBuilder, TOuterDeclSymbol>
        where TOuterDeclSymbol : ITopLevelDeclSymbolNode, ITopLevelDeclContainable
    {
        public struct PostSkeletonPhaseContext
        {
        }

        public delegate (FuncReturn, ImmutableArray<FuncParameter>) PostSkeletonPhaseTask(PostSkeletonPhaseContext context);
        
        TBuilder builder;
        TOuterDeclSymbol outer;
        List<(GlobalFuncDeclSymbol DeclSymbol, PostSkeletonPhaseTask Task)> postSkeletonPhaseTaskInfos;

        public GlobalFuncBuilderComponent(TBuilder builder, TOuterDeclSymbol outer)
        {
            this.builder = builder;
            this.outer = outer;

            this.postSkeletonPhaseTaskInfos = new List<(GlobalFuncDeclSymbol, PostSkeletonPhaseTask)>();
        }

        // 업데이트
        public void DoPostSkeletonPhase()
        {
            var context = new PostSkeletonPhaseContext();

            foreach (var (declSymbol, task) in postSkeletonPhaseTaskInfos)
            {
                var (funcRet, funcParams) = task.Invoke(context);
                declSymbol.InitFuncReturnAndParams(funcRet, funcParams);
            }
        }

        // 기본, 모든 인자 (리턴타입이나 인자타입으로 int void등 이미 reference 가능한 타입일 경우 이용 가능하다)
        public TBuilder GlobalFunc(Accessor accessor, FuncReturn funcReturn, string funcName, ImmutableArray<Name> typeParams, ImmutableArray<FuncParameter> funcParams)
        {
            var globalFuncDecl = new GlobalFuncDeclSymbol(
                outer, accessor, new Name.Normal(funcName), typeParams: typeParams);
            outer.AddFunc(globalFuncDecl);

            globalFuncDecl.InitFuncReturnAndParams(funcReturn, funcParams);
            return builder;
        }

        // 기본, 함수 인자와 파라미터는 task에서 수행한다
        public TBuilder GlobalFunc(Accessor accessor, string funcName, ImmutableArray<Name> typeParams, PostSkeletonPhaseTask task)
        {
            var globalFuncDecl = new GlobalFuncDeclSymbol(
                outer, accessor, new Name.Normal(funcName), typeParams: typeParams);
            outer.AddFunc(globalFuncDecl);

            postSkeletonPhaseTaskInfos.Add((globalFuncDecl, task));
            return builder;
        }

        // GlobalFunc의 다른 인자는 나중에 필요시 진행한다
    }
}