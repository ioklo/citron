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
    struct CanSearchInAllModulesVisitor : IDeclSymbolNodeVisitor
    {
        public bool result;

        void IDeclSymbolNodeVisitor.VisitModule(ModuleDeclSymbol declSymbol) { result = true; }
        void IDeclSymbolNodeVisitor.VisitNamespace(NamespaceDeclSymbol declSymbol) { result = true; }
        void IDeclSymbolNodeVisitor.VisitGlobalFunc(GlobalFuncDeclSymbol declSymbol) { result = false; }

        void IDeclSymbolNodeVisitor.VisitClass(ClassDeclSymbol declSymbol) { result = false; }
        void IDeclSymbolNodeVisitor.VisitClassConstructor(ClassConstructorDeclSymbol declSymbol) { result = false; }
        void IDeclSymbolNodeVisitor.VisitClassMemberFunc(ClassMemberFuncDeclSymbol declSymbol) { result = false; }
        void IDeclSymbolNodeVisitor.VisitClassMemberVar(ClassMemberVarDeclSymbol declSymbol) { result = false; }

        void IDeclSymbolNodeVisitor.VisitStruct(StructDeclSymbol declSymbol) { result = false; }
        void IDeclSymbolNodeVisitor.VisitStructConstructor(StructConstructorDeclSymbol declSymbol) { result = false; }
        void IDeclSymbolNodeVisitor.VisitStructMemberFunc(StructMemberFuncDeclSymbol declSymbol) { result = false; }
        void IDeclSymbolNodeVisitor.VisitStructMemberVar(StructMemberVarDeclSymbol declSymbol) { result = false; }

        void IDeclSymbolNodeVisitor.VisitEnum(EnumDeclSymbol declSymbol) { result = false; }
        void IDeclSymbolNodeVisitor.VisitEnumElem(EnumElemDeclSymbol declSymbol) { result = false; }
        void IDeclSymbolNodeVisitor.VisitEnumElemMemberVar(EnumElemMemberVarDeclSymbol declSymbol) { result = false; }

        void IDeclSymbolNodeVisitor.VisitInterface(InterfaceDeclSymbol declSymbol) { result = false; }

        void IDeclSymbolNodeVisitor.VisitLambda(LambdaDeclSymbol declSymbol) { result = false; }
        void IDeclSymbolNodeVisitor.VisitLambdaMemberVar(LambdaMemberVarDeclSymbol declSymbol) { result = false; }        
    }

    public static bool CanSearchInAllModules(this IDeclSymbolNode node)
    {
        var visitor = new CanSearchInAllModulesVisitor();
        node.AcceptDeclSymbolVisitor(ref visitor);
        return visitor.result;
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
    // Module, Namespace, Type중 하나이다. 나머지는 해당 안됨
    struct Item
    {
        // 아래 셋 중에 하나이다
        public ModuleSymbol? Module { get; init; }
        public NamespaceSymbol? Namespace { get; init; }
        public IType? Type { get; init; }

        public static Item Make(IType type)
        {
            return new Item() { Type = type };
        }

        public static Item? Make(ISymbolNode? symbol)
        {
            switch(symbol)
            {
                case ModuleSymbol moduleSymbol: return new Item() { Module = moduleSymbol };
                case NamespaceSymbol namespaceSymbol: return new Item() { Namespace = namespaceSymbol };
                case ITypeSymbol typeSymbol: return new Item() { Type = typeSymbol.MakeType() };
                default: return null;
            }
        }
    }

    record struct ItemGetter(IEnumerable<ModuleDeclSymbol> modules, SymbolFactory factory, IDeclSymbolNode node) : ITypeExpVisitor<Candidates<Item>>
    {
        public IType MakeType(TypeExp typeExp)
        {
            // 키워드
            if (typeExp is IdTypeExp idTypeExp)
            {
                // var
                if (idTypeExp.Name == "var" && idTypeExp.TypeArgs.Length == 0)
                    return new VarType();

                // void
                if (idTypeExp.Name == "void" && idTypeExp.TypeArgs.Length == 0)
                    return new VoidType();

                // bool
                if (idTypeExp.Name == "bool" && idTypeExp.TypeArgs.Length == 0)
                {
                    var boolType = GetRuntimeType(new Name.Normal("Bool"), 0);
                    if (boolType == null)
                        throw new NotImplementedException();

                    return boolType;
                }

                // int
                if (idTypeExp.Name == "int" && idTypeExp.TypeArgs.Length == 0)
                {
                    var intType = GetRuntimeType(new Name.Normal("Int32"), 0);
                    if (intType == null)
                        throw new NotImplementedException();

                    return intType;
                }


                // string
                if (idTypeExp.Name == "string" && idTypeExp.TypeArgs.Length == 0)
                {
                    var intType = GetRuntimeType(new Name.Normal("String"), 0);
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
                var typeArg = MakeType(typeExp);
                typeArgsBuilder.Add(typeArg);
            }
            return typeArgsBuilder.MoveToImmutable();
        }

        Candidates<Item> GetItems(TypeExp typeExp)
        {   
            return typeExp.Accept<ItemGetter, Candidates<Item>>(ref this);
        }

        // System.*
        IType? GetRuntimeType(Name name, int typeParamCount)
        {
            foreach (var moduleDecl in modules)
            {
                if (moduleDecl.GetName().Equals(new Name.Normal("System.Runtime")))
                {
                    var nsDecl = moduleDecl.GetNamespace(new Name.Normal("System"));
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
                            candidates.Add(Item.Make(typeVarType));
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
                            var candidate = Item.Make(symbol);

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

                        var candidate = Item.Make(symbol);
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

                if (outerItem.Type != null)
                {
                    var memberType = outerItem.Type.GetMemberType(nodeName.Name, typeArgs);
                    if (memberType != null)
                        candidates.Add(Item.Make(memberType));
                }
                else
                {
                    ISymbolNode outerSymbol;

                    if (outerItem.Module != null)
                        outerSymbol = outerItem.Module;
                    else if (outerItem.Namespace != null)
                        outerSymbol = outerItem.Namespace;
                    else
                        throw new UnreachableException();

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
                        var item = Item.Make(symbol);

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
                    candidates.Add(Item.Make(type));
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
            var type = MakeType(typeExp.InnerTypeExp);

            var candidates = new Candidates<Item>();
            candidates.Add(Item.Make(new LocalPtrType(type)));

            return candidates;
        }

        Candidates<Item> ITypeExpVisitor<Candidates<Item>>.VisitBoxPtr(BoxPtrTypeExp typeExp)
        {
            throw new NotImplementedException();
        }
    }    

    public static IType MakeType(IEnumerable<ModuleDeclSymbol> modules, SymbolFactory factory, IDeclSymbolNode node, TypeExp typeExp)
    {
        var getter = new ItemGetter(modules, factory, node);
        return getter.MakeType(typeExp);
    }
}