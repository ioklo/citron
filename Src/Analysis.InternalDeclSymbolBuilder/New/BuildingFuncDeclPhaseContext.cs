using System;

using Citron.Infra;
using Citron.Collections;

using Citron.Symbol;

using S = Citron.Syntax;
using M = Citron.Module;
using System.Collections.Generic;
using System.Diagnostics;

namespace Citron.Analysis
{
    class BuildingFuncDeclPhaseContext
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

        Candidates<ISymbolNode> GetOpenSymbols_IdTypeExp(S.IdTypeExp idTypeExp, IDeclSymbolNode node, SymbolFactory factory)
        {
            var curOuterPath = node.GetDeclSymbolId().Path;
            var curOuterNode = node;

            var candidates = new Candidates<ISymbolNode>();

            while (curOuterPath != null)
            {
                candidates.Clear();

                // 1. 타입 인자에서 찾기 (타입 인자는 declSymbol이 없으므로 리턴값은 Symbol이어야 한다)
                if (0 < idTypeExp.TypeArgs.Length) // optimization, for문 안쪽에 있어야 하는데 뺐다
                {
                    int typeParamCount = curOuterNode.GetTypeParamCount();
                    for (int i = 0; i < typeParamCount; i++)
                    {
                        var typeParam = curOuterNode.GetTypeParam(i);
                        if (typeParam.Equals(idTypeExp.Name))
                        {
                            int baseTypeParamIndex = curOuterNode.GetOuterDeclNode()?.GetTotalTypeParamCount() ?? 0;
                            var typeVarSymbol = factory.MakeTypeVar(baseTypeParamIndex + i);
                            candidates.Add(typeVarSymbol);
                            break; // 같은 이름이 있을수 없으므로 바로 종료
                        }
                    }
                }

                // 1. 레퍼런스모듈과 현재 모듈에서 경로로 찾기
                var path = curOuterPath.Child(idTypeExp.Name, idTypeExp.TypeArgs.Length);
                foreach (var module in modules)
                {
                    var declSymbol = module.GetDeclSymbol(path);

                    if (declSymbol != null)
                    {
                        var symbol = declSymbol.MakeOpenSymbol(factory);
                        candidates.Add(symbol);
                    }
                }

                if (!candidates.IsEmpty)
                    return candidates;

                curOuterPath = curOuterPath.Outer;
                curOuterNode = node.GetOuterDeclNode()!;
            }

            candidates.Clear();
            return candidates;
        }

        Candidates<ISymbolNode> GetOpenSymbols_MemberTypeExp(S.MemberTypeExp memberTypeExp, IDeclSymbolNode declSymbol, SymbolFactory factory)
        {
            var outerSymbols = GetOpenSymbols(memberTypeExp.Parent, declSymbol, factory);
            int count = outerSymbols.GetCount();
            if (count == 0)
                return outerSymbols; // return directly, if empty

            // NOTICE: Heap 사용
            var nodeName = new DeclSymbolNodeName(new M.Name.Normal(memberTypeExp.MemberName), memberTypeExp.TypeArgs.Length, default);
            var candidates = new Candidates<ISymbolNode>();
            for (int i = 0; i < count; i++)
            {
                var outerSymbol = outerSymbols.GetAt(i);
                var outerDeclSymbol = outerSymbol.GetDeclSymbolNode();

                if (outerDeclSymbol != null)
                {
                    var memberDeclSymbol = outerDeclSymbol.GetMemberDeclNode(nodeName);
                    if (memberDeclSymbol != null)
                    {
                        var openSymbol = SymbolInstantiator.InstantiateOpen(factory, outerSymbol, memberDeclSymbol);
                        candidates.Add(openSymbol);
                    }
                }
            }

            return candidates;
        }

        Candidates<ISymbolNode> GetOpenSymbols(S.TypeExp typeExp, IDeclSymbolNode declSymbol, SymbolFactory factory)
        {
            switch (typeExp)
            {
                case S.IdTypeExp idTypeExp:
                    return GetOpenSymbols_IdTypeExp(idTypeExp, declSymbol, factory);

                case S.MemberTypeExp memberTypeExp:
                    return GetOpenSymbols_MemberTypeExp(memberTypeExp, declSymbol, factory);

                default:
                    throw new UnreachableCodeException();
            }
        }

        public ITypeSymbol MakeTypeSymbol(IDeclSymbolNode curNode, S.TypeExp typeExp)
        {
            // 네임스페이스는 모듈을 넘어서도 공유되기 때문에, 검색때는 Module을 제외한 path만 사용한다            
            var candidates = GetOpenSymbols(typeExp, curNode, factory);

            switch (candidates.GetResult())
            {
                case UniqueQueryResult<ISymbolNode>.Found foundResult:
                    if (foundResult is ITypeSymbol typeSymbol)
                        return typeSymbol;
                    else
                        throw new NotImplementedException(); // 타입이 아닙니다.

                case UniqueQueryResult<ISymbolNode>.MultipleError:
                    throw new NotImplementedException(); // 모호합니다

                case UniqueQueryResult<ISymbolNode>.NotFound:
                    throw new NotImplementedException(); // 없습니다

                default:
                    throw new UnreachableCodeException();
            }
        }

        public (FuncReturn, ImmutableArray<FuncParameter> Param) MakeFuncReturnAndParams(IDeclSymbolNode curNode, bool bRefReturn, S.TypeExp retTypeSyntax, ImmutableArray<S.FuncParam> paramSyntaxes)
        {
            var retTypeSymbol = MakeTypeSymbol(curNode, retTypeSyntax);
            var ret = new FuncReturn(bRefReturn, retTypeSymbol);

            var paramsBuilder = ImmutableArray.CreateBuilder<FuncParameter>(paramSyntaxes.Length);
            foreach (var paramSyntax in paramSyntaxes)
            {   
                var paramKind = paramSyntax.Kind switch
                {
                    S.FuncParamKind.Normal => M.FuncParameterKind.Default,
                    S.FuncParamKind.Params => M.FuncParameterKind.Params,
                    S.FuncParamKind.Ref => M.FuncParameterKind.Ref,
                    _ => throw new UnreachableCodeException()
                };

                var paramTypeSymbol = MakeTypeSymbol(curNode, paramSyntax.Type);
                var param = new FuncParameter(paramKind, paramTypeSymbol, new M.Name.Normal(paramSyntax.Name));
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