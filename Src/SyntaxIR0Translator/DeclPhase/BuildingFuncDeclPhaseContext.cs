using System;

using Citron.Infra;
using Citron.Collections;

using Citron.Symbol;

using S = Citron.Syntax;
using System.Collections.Generic;
using System.Diagnostics;

namespace Citron.Analysis
{
    partial class BuildingFuncDeclPhaseContext
    {
        record struct PostBuildConstructorTask(ITypeDeclSymbol? Prerequisite, ITypeDeclSymbol This, Action Action);
        
        List<ModuleDeclSymbol> modules;
        SymbolFactory factory;
        List<PostBuildConstructorTask> postBuildConstructorTasks;

        public BuildingFuncDeclPhaseContext(List<ModuleDeclSymbol> modules, SymbolFactory factory)
        {
            this.modules = modules;
            this.factory = factory;
            this.postBuildConstructorTasks = new List<PostBuildConstructorTask>();
        }

        public IType MakeType(S.TypeExp typeExp, IDeclSymbolNode curNode, IDeclSymbolNode startNodeForSearchInAllModules)
        {
            return new TypeMakerByTypeExp(modules, factory, curNode, startNodeForSearchInAllModules).MakeType(typeExp);
        }

        public IType MakeType(S.TypeExp typeExp, IDeclSymbolNode curNode)
        {
            return new TypeMakerByTypeExp(modules, factory, curNode, curNode).MakeType(typeExp);
        }

        public (FuncReturn, ImmutableArray<FuncParameter> Param) MakeFuncReturnAndParams(IDeclSymbolNode curNode, bool bRefReturn, S.TypeExp retTypeSyntax, ImmutableArray<S.FuncParam> paramSyntaxes)
        {
            var outerNode = curNode.GetOuterDeclNode()!;

            // 지금은 중첩 함수가 없으므로 바로 윗 단계부터 찾도록 한다
            var retType = MakeType(retTypeSyntax, curNode, outerNode);
            var ret = new FuncReturn(bRefReturn, retType);

            var paramsBuilder = ImmutableArray.CreateBuilder<FuncParameter>(paramSyntaxes.Length);
            foreach (var paramSyntax in paramSyntaxes)
            {   
                var paramKind = paramSyntax.Kind switch
                {
                    S.FuncParamKind.Normal => FuncParameterKind.Default,
                    S.FuncParamKind.Params => FuncParameterKind.Params,
                    S.FuncParamKind.Ref => FuncParameterKind.Ref,
                    _ => throw new UnreachableCodeException()
                };

                var paramTypeSymbol = MakeType(paramSyntax.Type, curNode, outerNode);
                var param = new FuncParameter(paramKind, paramTypeSymbol, new Name.Normal(paramSyntax.Name));
                paramsBuilder.Add(param);
            }

            return (ret, paramsBuilder.MoveToImmutable());
        }

        // declSymbol의 Constructor를 만들고 나서 해당 task를 수행한다
        public void RegisterTaskAfterBuildingConstructorDeclSymbols(ITypeDeclSymbol? prerequisite, ITypeDeclSymbol @this, Action task)
        {
            postBuildConstructorTasks.Add(new PostBuildConstructorTask(prerequisite, @this, task));
        }

        public void DoRegisteredTasks()
        {
            bool IsFromReferenceModule(ITypeDeclSymbol declSymbol)
            {   
                var module = declSymbol.GetModule();
                return module.IsReference();
            }

            // declSymbol은 유일하기 때문에 레퍼런스를 키로 사용한다
            var visited = new HashSet<ITypeDeclSymbol>();

            int? removedTasks = null;
            while(removedTasks == null || removedTasks.Value != 0)
            {
                removedTasks = postBuildConstructorTasks.RemoveAll(task =>
                {
                    // 선행 declSymbol이 null또는 레퍼런스라면
                    if (task.Prerequisite == null || 
                        IsFromReferenceModule(task.Prerequisite) || 
                        visited.Contains(task.Prerequisite))
                    {
                        task.Action.Invoke();
                        visited.Add(task.This);
                        return true;
                    }

                    return false;
                });
            }
        }
    }
}