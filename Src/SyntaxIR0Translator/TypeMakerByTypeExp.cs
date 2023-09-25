using System;
using System.Collections.Generic;

using Citron.Infra;
using Citron.Symbol;
using Citron.Collections;
using Citron.Syntax;
using System.Diagnostics;

namespace Citron.Analysis;

static class IDeclSymbolNodeCanSearchInAllModulesExtension
{
    // extension을 고려해야 하나
    struct CanSearchInAllModulesVisitor : IDeclSymbolNodeVisitor<bool>
    {
        bool IDeclSymbolNodeVisitor<bool>.VisitModule(ModuleDeclSymbol declSymbol) { return true; }
        bool IDeclSymbolNodeVisitor<bool>.VisitNamespace(NamespaceDeclSymbol declSymbol) { return true; }
        bool IDeclSymbolNodeVisitor<bool>.VisitGlobalFunc(GlobalFuncDeclSymbol declSymbol) { return false; }

        bool IDeclSymbolNodeVisitor<bool>.VisitClass(ClassDeclSymbol declSymbol) { return false; }
        bool IDeclSymbolNodeVisitor<bool>.VisitClassConstructor(ClassConstructorDeclSymbol declSymbol) { return false; }
        bool IDeclSymbolNodeVisitor<bool>.VisitClassMemberFunc(ClassMemberFuncDeclSymbol declSymbol) { return false; }
        bool IDeclSymbolNodeVisitor<bool>.VisitClassMemberVar(ClassMemberVarDeclSymbol declSymbol) { return false; }

        bool IDeclSymbolNodeVisitor<bool>.VisitStruct(StructDeclSymbol declSymbol) { return false; }
        bool IDeclSymbolNodeVisitor<bool>.VisitStructConstructor(StructConstructorDeclSymbol declSymbol) { return false; }
        bool IDeclSymbolNodeVisitor<bool>.VisitStructMemberFunc(StructMemberFuncDeclSymbol declSymbol) { return false; }
        bool IDeclSymbolNodeVisitor<bool>.VisitStructMemberVar(StructMemberVarDeclSymbol declSymbol) { return false; }

        bool IDeclSymbolNodeVisitor<bool>.VisitEnum(EnumDeclSymbol declSymbol) { return false; }
        bool IDeclSymbolNodeVisitor<bool>.VisitEnumElem(EnumElemDeclSymbol declSymbol) { return false; }
        bool IDeclSymbolNodeVisitor<bool>.VisitEnumElemMemberVar(EnumElemMemberVarDeclSymbol declSymbol) { return false; }

        bool IDeclSymbolNodeVisitor<bool>.VisitInterface(InterfaceDeclSymbol declSymbol) { return false; }

        bool IDeclSymbolNodeVisitor<bool>.VisitLambda(LambdaDeclSymbol declSymbol) { return false; }
        bool IDeclSymbolNodeVisitor<bool>.VisitLambdaMemberVar(LambdaMemberVarDeclSymbol declSymbol) { return false; }        
    }

    public static bool CanSearchInAllModules(this IDeclSymbolNode node)
    {
        var visitor = new CanSearchInAllModulesVisitor();
        return node.Accept<CanSearchInAllModulesVisitor, bool>(ref visitor);
    }
}

// TODO: 아래 부분 테스트로 만들기
// class X<T> { class Y { <여기에서> } }
//
// T => X<>의 TypeVar T
// Y => X<T>.Y // decl space의 typevar가 그대로 살아있다
// X<Y>.Y => X<X<T>.Y>.Y  // 그거랑 별개로 인자로 들어온 것들은 적용을 시켜야 한다   
static class TypeMakerByTypeExp
{   
    // 심볼이거나, 인라인 타입이거나(nullable, box ptr, local ptr)
    struct Item
    {   
        public ISymbolNode? Symbol { get; init; }
        public IType? Type { get; init; }

        public static Item MakeTypeItem(IType type)
        {
            return new Item() { Type = type };
        }

        public static Item? MakeSymbolItem(ISymbolNode? symbol)
        {
            return new Item() { Symbol = symbol };
        }
    }

    record struct ItemGetter(IEnumerable<ModuleDeclSymbol> modules, SymbolFactory factory, IDeclSymbolNode node) : ITypeExpVisitor<Candidates<Item>>
    {
        // 최상단
        public IType MakeType(TypeExp typeExp, bool bLocalInterface)
        {
            // 키워드
            if (typeExp is IdTypeExp idTypeExp)
            {
                if (bLocalInterface)
                {
                    // TODO: [11] TypeExp에서 local은 인터페이스에서만 쓸수 있다고 에러를 낸다
                    throw new NotImplementedException();
                }

                // void
                if (idTypeExp.Name == "void" && idTypeExp.TypeArgs.Length == 0)
                    return new VoidType();

                // bool
                if (idTypeExp.Name == "bool" && idTypeExp.TypeArgs.Length == 0)
                {
                    var boolType = GetRuntimeType(new Name.Normal("Bool"), 0, bLocalInterface: false);
                    if (boolType == null)
                        throw new NotImplementedException();

                    return boolType;
                }

                // int
                if (idTypeExp.Name == "int" && idTypeExp.TypeArgs.Length == 0)
                {
                    var intType = GetRuntimeType(new Name.Normal("Int32"), 0, bLocalInterface: false);
                    if (intType == null)
                        throw new NotImplementedException();

                    return intType;
                }


                // string
                if (idTypeExp.Name == "string" && idTypeExp.TypeArgs.Length == 0)
                {
                    var intType = GetRuntimeType(new Name.Normal("String"), 0, bLocalInterface: false);
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
                if (value.Symbol is ITypeSymbol typeSymbol)
                {
                    if (bLocalInterface && typeSymbol is not InterfaceSymbol)
                    {
                        // TODO: [11] TypeExp에서 local은 인터페이스에서만 쓸수 있다고 에러를 낸다
                        throw new NotImplementedException();
                    }

                    return typeSymbol.MakeType(bLocalInterface);
                }
                else if (value.Type != null)
                    return value.Type;
                else
                    throw new NotImplementedException(); // 타입이 아닙니다
            }
            else if (result.IsNotFound())
                throw new NotImplementedException(); // 없습니다
            else if (result.IsMultipleError(out _))
                throw new NotImplementedException(); // 모호합니다
            else
                throw new UnreachableException();
        }

        ImmutableArray<IType> MakeTypeArgs(ImmutableArray<TypeExp> typeExps)
        {
            // typeArgs계산부터, typeArg를 계산은 현재 decl space위치를 그대로 쓴다
            var typeArgsBuilder = ImmutableArray.CreateBuilder<IType>(typeExps.Length);
            foreach (var typeExp in typeExps)
            {
                var typeArg = MakeType(typeExp, bLocalInterface: false);
                typeArgsBuilder.Add(typeArg);
            }
            return typeArgsBuilder.MoveToImmutable();
        }

        Candidates<Item> GetItems(TypeExp typeExp)
        {   
            return typeExp.Accept<ItemGetter, Candidates<Item>>(ref this);
        }

        // System.*
        IType? GetRuntimeType(Name name, int typeParamCount, bool bLocalInterface)
        {
            foreach (var moduleDecl in modules)
            {
                if (moduleDecl.GetName().Equals(new Name.Normal("System.Runtime")))
                {
                    var nsDecl = moduleDecl.GetNamespace(new Name.Normal("System"));
                    if (nsDecl == null) return null;

                    var typeDecl = nsDecl.GetType(name, typeParamCount);
                    if (typeDecl == null) return null;

                    var typeSymbol = typeDecl.MakeOpenSymbol(factory);
                    if (typeSymbol == null) return null;

                    return typeSymbol.MakeType(bLocalInterface);
                }
            }

            return null;
        }        

        Candidates<Item> ITypeExpVisitor<Candidates<Item>>.VisitId(IdTypeExp idTypeExp)
        {
            var typeArgs = MakeTypeArgs(idTypeExp.TypeArgs);

            // node를 한 단계씩 차례로 올려가면서 찾는다
            IDeclSymbolNode curOuterNode = node;

            var idName = new Name.Normal(idTypeExp.Name);
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
                            candidates.Add(Item.MakeTypeItem(typeVarType));
                            break; // 같은 이름이 있을수 없으므로 바로 종료
                        }
                    }
                }

                // 1. 레퍼런스모듈과 현재 모듈에서 경로로 찾기
                if (curOuterNode.CanSearchInAllModules())
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
                            var candidate = Item.MakeSymbolItem(symbol);

                            // Module, Namespace, Type에 해당이 되지 않으면 스킵
                            if (candidate != null)
                                candidates.Add(candidate.Value);
                        }
                    }
                }
                else
                {
                    var declSymbol = curOuterNode.GetDeclSymbol(new DeclSymbolPath(null, idName, idTypeExp.TypeArgs.Length));
                    if (declSymbol != null)
                    {
                        var symbol = declSymbol.MakeOpenSymbol(factory);

                        var candidate = Item.MakeSymbolItem(symbol);
                        if (candidate != null)
                            candidates.Add(candidate.Value);
                    }
                }

                if (candidates.ContainsItem())
                    return candidates;

                curOuterNode = curOuterNode.GetOuterDeclNode()!;
            }

            candidates.Clear();
            return candidates;
        }

        Candidates<Item> ITypeExpVisitor<Candidates<Item>>.VisitMember(MemberTypeExp memberTypeExp)
        {
            var outerItems = GetItems(memberTypeExp.Parent);
            int count = outerItems.GetCount();
            if (count == 0)
                return outerItems; // return directly, if empty

            // typeArgs계산
            var typeArgs = MakeTypeArgs(memberTypeExp.TypeArgs);

            // NOTICE: Heap 사용
            var nodeName = new DeclSymbolNodeName(new Name.Normal(memberTypeExp.MemberName), memberTypeExp.TypeArgs.Length, default);
            var candidates = new Candidates<Item>();
            for (int i = 0; i < count; i++)
            {
                var outerItem = outerItems.GetAt(i);

                if (outerItem.Type != null) // 심볼이 아닌 Type
                {
                    // 현재 인라인 타입은 멤버 타입이 없다. 에러
                    throw new NotImplementedException();

                    //var memberType = outerItem.Type.GetMemberType(nodeName.Name, typeArgs);
                    //if (memberType != null)
                    //    candidates.Add(Item.MakeTypeItem(memberType));
                }
                else if (outerItem.Symbol != null)
                {
                    var outerSymbol = outerItem.Symbol;
                    
                    // class X<T> { class Y<U> { class Z { X<bool>.Y<int> } }

                    // outerItem은 X<bool> 

                    // outerDeclSymbol은 X<>
                    var outerDeclSymbol = outerSymbol.GetDeclSymbolNode();

                    // memberDeclSymbol은 X<>.Y
                    var memberDeclSymbol = outerDeclSymbol.GetMemberDeclNode(nodeName);
                    if (memberDeclSymbol != null)
                    {
                        // symbol은 X<bool>.Y<int>
                        var symbol = SymbolInstantiator.Instantiate(factory, outerSymbol, memberDeclSymbol, typeArgs);
                        var item = Item.MakeSymbolItem(symbol);

                        if (item != null)
                            candidates.Add(item.Value);
                    }
                }
            }

            return candidates;
        }

        // int?
        Candidates<Item> ITypeExpVisitor<Candidates<Item>>.VisitNullable(NullableTypeExp typeExp)
        {            
            var candidates = GetItems(typeExp.InnerTypeExp);

            var result = candidates.GetUniqueResult();
            if (result.IsFound(out var item))
            {
                if (item.Type != null)
                {
                    var type = new NullableType(item.Type);

                    candidates.Clear();
                    candidates.Add(Item.MakeTypeItem(type));
                    return candidates;
                }
                else
                {
                    throw new NotImplementedException(); // 타입이 아닙니다
                }
            }
            else
            {
                throw new NotImplementedException(); // 타입이 아닙니다
            }
        }

        Candidates<Item> ITypeExpVisitor<Candidates<Item>>.VisitLocalPtr(LocalPtrTypeExp typeExp)
        {
            var type = MakeType(typeExp.InnerTypeExp, bLocalInterface: false);

            var candidates = new Candidates<Item>();
            candidates.Add(Item.MakeTypeItem(new LocalPtrType(type)));

            return candidates;
        }

        Candidates<Item> ITypeExpVisitor<Candidates<Item>>.VisitBoxPtr(BoxPtrTypeExp typeExp)
        {
            var type = MakeType(typeExp.InnerTypeExp, bLocalInterface: false);

            var candidates = new Candidates<Item>();
            candidates.Add(Item.MakeTypeItem(new BoxPtrType(type)));

            return candidates;
        }

        Candidates<Item> ITypeExpVisitor<Candidates<Item>>.VisitLocal(LocalTypeExp typeExp)
        {
            // local 검사
            var type = MakeType(typeExp.InnerTypeExp, bLocalInterface: true); // 여기서 bLocalInterface 플래그를 올린다
            var candidates = new Candidates<Item>();
            candidates.Add(Item.MakeTypeItem(new BoxPtrType(type)));

            return candidates;
        }
    }    

    public static IType MakeType(IEnumerable<ModuleDeclSymbol> modules, SymbolFactory factory, IDeclSymbolNode node, TypeExp typeExp)
    {
        var getter = new ItemGetter(modules, factory, node);
        return getter.MakeType(typeExp, bLocalInterface: false); // bLocalInterface의 초기값은 false, typeExp 중에 LocalTypeExp가 있으면 순회중에 켜진다
    }
}