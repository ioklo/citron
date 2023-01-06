using System;

using Citron.Infra;

using Citron.Symbol;

using S = Citron.Syntax;
using M = Citron.Module;
using System.Collections.Generic;
using Citron.Collections;

namespace Citron.Analysis;

partial class BuildingFuncDeclPhaseContext
{
    // TODO: 아래 부분 테스트로 만들기
    // class X<T> { class Y { <여기에서> } }
    //
    // T => X<>의 TypeVar T
    // Y => X<T>.Y // decl space의 typevar가 그대로 살아있다
    // X<Y>.Y => X<X<T>.Y>.Y  // 그거랑 별개로 인자로 들어온 것들은 적용을 시켜야 한다   
    record struct TypeMakerByTypeExp(List<ModuleDeclSymbol> modules, SymbolFactory factory, IDeclSymbolNode node, IDeclSymbolNode startNodeForSearchInAllModules)
    {
        // 타입일 수도 있고, Module / Namespace 심볼일 수도 있다
        struct Item
        {
            public IType? Type { get; init; }
            public ISymbolNode? Symbol { get; init; }

            public static Item Make(IType type) => new Item() { Type = type };
            public static Item Make(ISymbolNode? symbol)
            {
                if (symbol is ITypeSymbol typeSymbol)
                {
                    return new Item() { Type = typeSymbol.MakeType() };
                }
                else
                {
                    return new Item() { Symbol = symbol };
                }
            }
        }

        ImmutableArray<IType> MakeTypeArgs(ImmutableArray<S.TypeExp> typeExps)
        {
            // typeArgs계산부터, typeArg를 계산은 현재 decl space위치를 그대로 쓴다
            var typeArgsBuilder = ImmutableArray.CreateBuilder<IType>(typeExps.Length);
            foreach (var typeExp in typeExps)
            {
                var typeArg = MakeType(typeExp);
                typeArgsBuilder.Add(typeArg);
            }
            return typeArgsBuilder.MoveToImmutable();
        }

        Candidates<Item> GetItems_IdTypeExp(S.IdTypeExp idTypeExp)
        {
            var typeArgs = MakeTypeArgs(idTypeExp.TypeArgs);

            // node를 한 단계씩 차례로 올려가면서 찾는다
            IDeclSymbolNode curOuterNode = node;

            // 함수같은 경우, 이름이 완성되지 않은 경우, 모듈에서 찾을 수 없고, 실제로 다른 모듈에 같은 이름의 함수가 있는것도 이상하므로, 가능할 때만 찾도록 한다
            bool bCanSearchInAllModules = false;

            var idName = new M.Name.Normal(idTypeExp.Name);
            var candidates = new Candidates<Item>();
            while (curOuterNode != null)
            {
                candidates.Clear();

                // 1. 타입 인자에서 찾기 (타입 인자는 declSymbol이 없으므로 리턴값은 Symbol이어야 한다)
                // T => X<>의 TypeVar T
                if (idTypeExp.TypeArgs.Length == 0) // optimization, for문 안쪽에 있어야 하는데 뺐다
                {
                    int typeParamCount = curOuterNode.GetTypeParamCount();
                    for (int i = 0; i < typeParamCount; i++)
                    {
                        var typeParam = curOuterNode.GetTypeParam(i);
                        if (typeParam.Equals(idName))
                        {
                            int baseTypeParamIndex = curOuterNode.GetOuterDeclNode()?.GetTotalTypeParamCount() ?? 0;
                            var typeVarType = new TypeVarType(baseTypeParamIndex + i, typeParam);
                            candidates.Add(Item.Make(typeVarType));
                            break; // 같은 이름이 있을수 없으므로 바로 종료
                        }
                    }
                }

                // 1. 레퍼런스모듈과 현재 모듈에서 경로로 찾기
                if (!bCanSearchInAllModules)
                    bCanSearchInAllModules = (curOuterNode == startNodeForSearchInAllModules);

                if (bCanSearchInAllModules)
                {
                    var outerPath = curOuterNode.GetDeclSymbolId().Path;
                    var path = outerPath.Child(idTypeExp.Name, idTypeExp.TypeArgs.Length);
                    foreach (var module in modules)
                    {
                        var declSymbol = module.GetDeclSymbol(path);

                        if (declSymbol != null)
                        {
                            // class X<T> { class Y<U> { class Z { Y<int> x; } } }
                            // 여기서 Z에 있는 Y<int>는 X<T>.Y<int> 이다
                            // X<T>는 declSymbol의 outer로부터 OpenSymbol을 만들고, Y는 int를 적용해서 만든다

                            // declSymbol이 X<>.Y<> 일때,

                            // outerDeclSymbol은 X<>
                            var outerDeclSymbol = declSymbol.GetOuterDeclNode();

                            // outerSymbol은 X<T> (open)
                            var outerSymbol = outerDeclSymbol?.MakeOpenSymbol(factory);

                            // symbol은 X<T>.Y<int>
                            var symbol = SymbolInstantiator.Instantiate(factory, outerSymbol, declSymbol, typeArgs);
                            var candidate = Item.Make(symbol);

                            candidates.Add(candidate);
                        }
                    }
                }

                if (candidates.ContainsItem())
                    return candidates;
                
                curOuterNode = curOuterNode.GetOuterDeclNode()!;
            }

            candidates.Clear();
            return candidates;
        }
        
        Candidates<Item> GetItems_MemberTypeExp(S.MemberTypeExp memberTypeExp)
        {
            var outerItems = GetItems(memberTypeExp.Parent);
            int count = outerItems.GetCount();
            if (count == 0)
                return outerItems; // return directly, if empty

            // typeArgs계산
            var typeArgs = MakeTypeArgs(memberTypeExp.TypeArgs);

            // NOTICE: Heap 사용
            var nodeName = new DeclSymbolNodeName(new M.Name.Normal(memberTypeExp.MemberName), memberTypeExp.TypeArgs.Length, default);
            var candidates = new Candidates<Item>();
            for (int i = 0; i < count; i++)
            {
                var outerItem = outerItems.GetAt(i);

                if (outerItem.Type != null)
                {
                    var memberType = outerItem.Type.GetMemberType(nodeName.Name, typeArgs);
                    if (memberType != null)
                        candidates.Add(Item.Make(memberType));
                }
                else if (outerItem.Symbol != null)
                {
                    // class X<T> { class Y<U> { class Z { X<bool>.Y<int> } }

                    // outerItem은 X<bool> 

                    // outerDeclSymbol은 X<>
                    var outerDeclSymbol = outerItem.Symbol.GetDeclSymbolNode();

                    // memberDeclSymbol은 X<>.Y
                    var memberDeclSymbol = outerDeclSymbol.GetMemberDeclNode(nodeName);
                    if (memberDeclSymbol != null)
                    {
                        // symbol은 X<bool>.Y<int>
                        var symbol = SymbolInstantiator.Instantiate(factory, outerItem.Symbol, memberDeclSymbol, typeArgs);
                        candidates.Add(Item.Make(symbol));
                    }
                }
                else
                {
                    throw new UnreachableCodeException();
                }
            }

            return candidates;
        }

        Candidates<Item> GetItems(S.TypeExp typeExp)
        {
            switch (typeExp)
            {
                case S.IdTypeExp idTypeExp:
                    return GetItems_IdTypeExp(idTypeExp);

                case S.MemberTypeExp memberTypeExp:
                    return GetItems_MemberTypeExp(memberTypeExp);

                default:
                    throw new UnreachableCodeException();
            }
        }

        // System.*
        IType? GetRuntimeType(M.Name name, int typeParamCount)
        {
            foreach(var moduleDecl in modules)
            {
                if (moduleDecl.GetName().Equals(new M.Name.Normal("System.Runtime")))
                {
                    var nsDecl = moduleDecl.GetNamespace(new M.Name.Normal("System"));
                    if (nsDecl == null) return null;

                    var typeDecl = nsDecl.GetType(name, typeParamCount);
                    if (typeDecl == null) return null;

                    var typeSymbol = typeDecl.MakeOpenSymbol(factory) as ITypeSymbol;
                    if (typeSymbol == null) return null;

                    return typeSymbol.MakeType();
                }
            }

            return null;
        }

        public IType MakeType(S.TypeExp typeExp)
        {
            // 키워드
            if (typeExp is S.IdTypeExp idTypeExp)
            {
                // void
                if (idTypeExp.Name == "void" && idTypeExp.TypeArgs.Length == 0)
                    return new VoidType();

                // bool
                if (idTypeExp.Name == "bool" && idTypeExp.TypeArgs.Length == 0)
                {
                    var intType = GetRuntimeType(new M.Name.Normal("Bool"), 0);
                    if (intType == null)
                        throw new NotImplementedException();

                    return intType;
                }

                // int
                if (idTypeExp.Name == "int" && idTypeExp.TypeArgs.Length == 0)
                {
                    var intType = GetRuntimeType(new M.Name.Normal("Int32"), 0);
                    if (intType == null)
                        throw new NotImplementedException();

                    return intType;
                }


                // string
                if (idTypeExp.Name == "string" && idTypeExp.TypeArgs.Length == 0)
                {
                    var intType = GetRuntimeType(new M.Name.Normal("String"), 0);
                    if (intType == null)
                        throw new NotImplementedException();

                    return intType;
                }
            }


            // 네임스페이스는 모듈을 넘어서도 공유되기 때문에, 검색때는 Module을 제외한 path만 사용한다
            var candidates = GetItems(typeExp);
            var result = candidates.GetUniqueResult();

            if (result.IsFound(out var value))
            {
                if (value.Type != null)
                    return value.Type;
                else
                    throw new NotImplementedException(); // 타입이 아닙니다
            }
            else if (result.IsNotFound())
                throw new NotImplementedException(); // 없습니다
            else if (result.IsMultipleError())
                throw new NotImplementedException(); // 모호합니다
            else
                throw new UnreachableCodeException();
        }
    }
}