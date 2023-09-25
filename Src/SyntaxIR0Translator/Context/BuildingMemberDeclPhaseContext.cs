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
        List<Func<BuildingBodyPhaseContext, bool>> buildingBodyPhaseTasks;


        public BuildingMemberDeclPhaseContext(ImmutableArray<ModuleDeclSymbol> modules, SymbolFactory factory)
        {
            this.modules = modules;
            this.factory = factory;
            buildingTrivialConstructorPhaseTasks = new List<BuildingTrivialConstructorPhaseTask>();
            buildingBodyPhaseTasks = new List<Func<BuildingBodyPhaseContext, bool>>();
        }

        public IType MakeType(S.TypeExp typeExp, IDeclSymbolNode curNode)
        {
            return TypeMakerByTypeExp.MakeType(modules.AsEnumerable(), factory, curNode, typeExp);
        }

        // TODO: [0] bVariadic 처리를 해야 한다
        public (FuncReturn Ret, ImmutableArray<FuncParameter> Params, bool bLastParamVariadic) MakeFuncReturnAndParams(IDeclSymbolNode curNode, S.TypeExp retTypeSyntax, ImmutableArray<S.FuncParam> paramSyntaxes)
        {
            // 지금은 중첩 함수가 없으므로 바로 윗 단계부터 찾도록 한다
            var retType = MakeType(retTypeSyntax, curNode);
            var ret = new FuncReturn(retType);

            bool bLastParamVariadic = false;

            int paramSyntaxesCount = paramSyntaxes.Length;
            var paramsBuilder = ImmutableArray.CreateBuilder<FuncParameter>(paramSyntaxesCount);

            for(int i = 0; i < paramSyntaxesCount; i++)
            {
                var paramSyntax = paramSyntaxes[i];

                var paramTypeSymbol = MakeType(paramSyntax.Type, curNode);
                var param = new FuncParameter(paramSyntax.HasOut, paramTypeSymbol, new Name.Normal(paramSyntax.Name));
                paramsBuilder.Add(param);

                if (paramSyntax.HasParams)
                {
                    if (i == paramSyntaxesCount - 1)
                    {
                        bLastParamVariadic = true;
                    }
                    else
                    {
                        throw new NotImplementedException(); // 에러 처리. bVariadic은 마지막에 있어야 합니다
                    }
                }
            }

            return (ret, paramsBuilder.MoveToImmutable(), bLastParamVariadic);
        }

        // declSymbol의 Constructor를 만들고 나서 해당 task를 수행한다
        public void AddBuildingTrivialConstructorPhaseTask(ITypeDeclSymbol? prerequisite, ITypeDeclSymbol @this, Action task)
        {
            buildingTrivialConstructorPhaseTasks.Add(new BuildingTrivialConstructorPhaseTask(prerequisite, @this, task));
        }

        public void AddBuildingBodyPhaseTask(Func<BuildingBodyPhaseContext, bool> task)
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

        public bool BuildBody(BuildingBodyPhaseContext context)
        {
            foreach (var task in buildingBodyPhaseTasks)
            {
                if (!task.Invoke(context))
                    return false;
            }

            return true;
        }
    }
}