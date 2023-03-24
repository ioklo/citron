using System;

using Citron.Infra;
using Citron.Collections;

using Citron.Symbol;

using S = Citron.Syntax;
using System.Collections.Generic;
using System.Diagnostics;

namespace Citron.Analysis
{
    partial class BuildingMemberDeclPhaseContext
    {
        record struct BuildingTrivialConstructorPhaseTask(ITypeDeclSymbol? Prerequisite, ITypeDeclSymbol This, Action Action);

        ImmutableArray<ModuleDeclSymbol> modules;
        SymbolFactory factory;
        List<BuildingTrivialConstructorPhaseTask> buildingTrivialConstructorPhaseTasks;
        List<Action<BuildingBodyPhaseContext>> buildingBodyPhaseTasks;


        public BuildingMemberDeclPhaseContext(ImmutableArray<ModuleDeclSymbol> modules, SymbolFactory factory)
        {
            this.modules = modules;
            this.factory = factory;
            buildingTrivialConstructorPhaseTasks = new List<BuildingTrivialConstructorPhaseTask>();
            buildingBodyPhaseTasks = new List<Action<BuildingBodyPhaseContext>>();
        }

        public IType MakeType(S.TypeExp typeExp, IDeclSymbolNode curNode)
        {
            return TypeMakerByTypeExp.MakeType(modules.AsEnumerable(), factory, curNode, typeExp);
        }

        public (FuncReturn, ImmutableArray<FuncParameter> Param) MakeFuncReturnAndParams(IDeclSymbolNode curNode, S.TypeExp retTypeSyntax, ImmutableArray<S.FuncParam> paramSyntaxes)
        {
            // 지금은 중첩 함수가 없으므로 바로 윗 단계부터 찾도록 한다
            var retType = MakeType(retTypeSyntax, curNode);
            var ret = new FuncReturn(retType);

            var paramsBuilder = ImmutableArray.CreateBuilder<FuncParameter>(paramSyntaxes.Length);
            foreach (var paramSyntax in paramSyntaxes)
            {
                var paramKind = paramSyntax.Kind switch
                {
                    S.FuncParamKind.Normal => FuncParameterKind.Default,
                    S.FuncParamKind.Params => FuncParameterKind.Params,
                    _ => throw new UnreachableCodeException()
                };

                var paramTypeSymbol = MakeType(paramSyntax.Type, curNode);
                var param = new FuncParameter(paramKind, paramTypeSymbol, new Name.Normal(paramSyntax.Name));
                paramsBuilder.Add(param);
            }

            return (ret, paramsBuilder.MoveToImmutable());
        }

        // declSymbol의 Constructor를 만들고 나서 해당 task를 수행한다
        public void AddBuildingTrivialConstructorPhaseTask(ITypeDeclSymbol? prerequisite, ITypeDeclSymbol @this, Action task)
        {
            buildingTrivialConstructorPhaseTasks.Add(new BuildingTrivialConstructorPhaseTask(prerequisite, @this, task));
        }

        public void AddBuildingBodyPhaseTask(Action<BuildingBodyPhaseContext> task)
        {
            buildingBodyPhaseTasks.Add(task);
        }

        public void BuildTrivialConstructor()
        {
            bool IsFromReferenceModule(ITypeDeclSymbol declSymbol)
            {
                var module = declSymbol.GetModule();
                return module.IsReference();
            }

            // declSymbol은 유일하기 때문에 레퍼런스를 키로 사용한다
            var visited = new HashSet<ITypeDeclSymbol>();

            int? removedTasks = null;
            while (removedTasks == null || removedTasks.Value != 0)
            {
                removedTasks = buildingTrivialConstructorPhaseTasks.RemoveAll(task =>
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

        public void BuildBody(BuildingBodyPhaseContext context)
        {
            foreach (var task in buildingBodyPhaseTasks)
            {
                task.Invoke(context);
            }
        }
    }
}