#include "RFactory.h"

#include <cassert>
#include <algorithm>

#include "Infra/Hash.h"
#include "Infra/Ptr.h"

#include "RNamespaceDecl.h"

#include "RModule.h"
#include "RTypes.h"
#include "RTypeDecl.h"
#include "RStructDecl.h"
#include "RClassDecl.h"
#include "REnumDecl.h"
#include "REnumElemDecl.h"
#include "RInterfaceDecl.h"
#include "RLambdaDecl.h"

#include "RNamespaceDeclGroup.h"
#include "RTypeArguments.h"
#include "RTypeParam.h"


using namespace std;

namespace Citron {

struct RFactoryPrivateData
{
    std::deque<RModule> modules;
    std::deque<RTypeParam> typeParams;
};

RFactory::RFactory()
    : voidType{new RType_Void()}
    , boolType{new RType_Primitive(RType_PrimitiveKind::Bool)}
    , intType{new RType_Primitive(RType_PrimitiveKind::Int32)}
    , privateData{new RFactoryPrivateData()}
{
    // TODO: 아직 MakeStructType, MakeClassType과는 연결이 되지 않은 상태
    stringType = MakeStructType(nullptr, MakeEmptyTypeArguments());
}

RFactory::~RFactory()
{
}

RTypeParam* RFactory::MakeTypeParam(RDecl* rDecl, RName&& name, size_t index, TakeRef<RFactoryPtr> rFactory)
{
    privateData->typeParams.emplace_back(rDecl, move(name), index, move(rFactory));
    return &privateData->typeParams.back(); 
}

RType_Nullable* RFactory::MakeNullableType(RType* innerType)
{   
    auto i = nullableValueTypes.find(innerType);

    if (i != nullableValueTypes.end())
        return i->second.get();

    auto* key = innerType;
    unique_ptr<RType_Nullable> newType{new RType_Nullable(innerType, this)};
    auto* pNewType = newType.get();
    nullableValueTypes.emplace(innerType, move(newType));

    return pNewType;
}

RType_NullableInplace* RFactory::MakeNullableInplaceType(RType* innerType)
{
    auto i = nullableRefTypes.find(innerType);

    if (i != nullableRefTypes.end())
        return i->second.get();

    auto* key = innerType;
    unique_ptr<RType_NullableInplace> newType{new RType_NullableInplace{innerType, this}};
    auto pNewType = newType.get();
    nullableRefTypes.emplace(key, move(newType));

    return pNewType;
}

RType_TypeVar* RFactory::MakeTypeVarType(RTypeParam* decl)
{
    auto i = typeVarTypes.find(decl);
    if (i != typeVarTypes.end())
        return i->second.get();

    unique_ptr<RType_TypeVar> newTypeVarType{new RType_TypeVar(decl)};
    auto pNewTypeVarType = newTypeVarType.get();
    typeVarTypes.try_emplace(decl, move(newTypeVarType));
    return pNewTypeVarType;
}

RType_Void* RFactory::MakeVoidType()
{
    return voidType.get();
}

// (a: int, b: string)과 (c: int, d: string)은 같은 타입처럼 써야 하는데, 멤버 이름이 달라서
// 같은 타입이라고 하지 않고, 호환되는 타입이라고 하자
// 대입같은걸 할때 호환타입도 같이 검색해야 한다
RType_Tuple* RFactory::MakeTupleType(vector<RTupleVar>&& vars)
{
    auto i = tupleTypes.find(vars);
    if (i != tupleTypes.end())
        return i->second.get();

    auto key = vars;
    unique_ptr<RType_Tuple> tupleType{new RType_Tuple{move(vars), this}};
    auto pTupleType = tupleType.get();
    tupleTypes.emplace(move(key), move(tupleType));

    return pTupleType;
}

RType_Func* RFactory::MakeFuncType(bool bLocal, RType* retType, vector<RType_Func::Parameter>&& params)
{
    RFuncTypeKey key{bLocal, retType, params};
    auto i = funcTypes.find(key);
    if (i != funcTypes.end())
        return i->second.get();
    
    unique_ptr<RType_Func> newFuncType{new RType_Func{bLocal, retType, move(params), this}};
    auto pNewFuncType = newFuncType.get();
    funcTypes.emplace(key, move(newFuncType));
    return pNewFuncType;
}

RType_Ptr* RFactory::MakePtrType(RType* innerType)
{
    auto i = ptrTypes.find(innerType);
    if (i != ptrTypes.end())
        return i->second.get();

    unique_ptr<RType_Ptr> newType{new RType_Ptr{innerType, this}};
    auto pNewType = newType.get();
    ptrTypes.emplace(innerType, move(newType));
    return pNewType;
}

RType_Shared* RFactory::MakeSharedType(RType* innerType)
{
    auto i = sharedTypes.find(innerType);
    if (i != sharedTypes.end())
        return i->second.get();

    unique_ptr<RType_Shared> newType{new RType_Shared{innerType, this}};
    auto pNewType = newType.get();
    sharedTypes.emplace(innerType, move(newType));
    return pNewType;
}

RType_Box* RFactory::MakeBoxType(RType* innerType)
{
    auto i = boxTypes.find(innerType);
    if (i != boxTypes.end())
        return i->second.get();

    unique_ptr<RType_Box> newType{new RType_Box{innerType, this}};
    auto pNewType = newType.get();
    boxTypes.emplace(innerType, move(newType));
    return pNewType;
}

template<typename TDecl, typename TType, typename... TArgs>
TType* RFactory::MakeInstanceType(InstanceTypeKeyUnorderedMap<TDecl, TType>& instanceTypes, TDecl* decl, RTypeArguments* typeArgs, TArgs&&... args)
{
    RInstanceTypeKey<TDecl> key{decl, typeArgs};
    auto i = instanceTypes.find(key);
    if (i != instanceTypes.end())
        return i->second.get();
    
    unique_ptr<TType> newType{new TType{decl, typeArgs, std::forward<TArgs>(args)..., this}};
    auto pNewType = newType.get();
    instanceTypes.emplace(key, move(newType));
    return pNewType;
}

RType_Class* RFactory::MakeClassType(RClassDecl* decl, RTypeArguments* typeArgs)
{
    return MakeInstanceType(classTypes, decl, typeArgs);
}

RType_Struct* RFactory::MakeStructType(RStructDecl* decl, RTypeArguments* typeArgs)
{
    return MakeInstanceType(structTypes, decl, typeArgs);
}

RType_Enum* RFactory::MakeEnumType(REnumDecl* decl, RTypeArguments* typeArgs)
{
    return MakeInstanceType(enumTypes, decl, typeArgs);
}

RType_EnumElem* RFactory::MakeEnumElemType(REnumElemDecl* decl, RTypeArguments* typeArgs)
{
    return MakeInstanceType(enumElemTypes, decl, typeArgs);
}

RType_Interface* RFactory::MakeInterfaceType(RInterfaceDecl* decl, RTypeArguments* typeArgs, bool bLocal)
{
    return MakeInstanceType(interfaceTypes, decl, typeArgs, bLocal);
}

RType_Lambda* RFactory::MakeLambdaType(RLambdaDecl* decl, RTypeArguments* typeArgs)
{
    return MakeInstanceType(lambdaTypes, decl, typeArgs);
}

RTypeArguments* RFactory::MakeTypeArguments(span<RType*> items)
{
    RTypeArgumentsKeyView key{items};

    auto i = typeArgsMap.find(key);
    if (i != typeArgsMap.end())
        return i->second.get();

    // TODO: vector 두벌 생성
    unique_ptr<RTypeArguments> v{new RTypeArguments{vector<RType*>{items.begin(), items.end()}, this}};
    auto pv = v.get();
    typeArgsMap.emplace(vector<RType*>{items.begin(), items.end()}, move(v));
    return pv;
}

RTypeArguments* RFactory::MakeTypeArguments(std::vector<RType*>&& items)
{
    RTypeArgumentsKeyView key{items};
    auto i = typeArgsMap.find(key);
    if (i != typeArgsMap.end())
        return i->second.get();

    // TODO: vector 두벌 생성
    unique_ptr<RTypeArguments> v{new RTypeArguments{vector<RType*>{items}, this}};
    auto pv = v.get();
    typeArgsMap.emplace(move(items), move(v));
    return pv;
}

RTypeArguments* RFactory::MakeEmptyTypeArguments()
{
    // static std::vector<RType*> items;
    static RTypeArgumentsKeyView key{{}};

    auto i = typeArgsMap.find(key);
    if (i != typeArgsMap.end())
        return i->second.get();

    unique_ptr<RTypeArguments> v{new RTypeArguments{{}, this}};
    auto pv = v.get();
    typeArgsMap.emplace(vector<RType*>{}, move(v));
    return pv;
}

RTypeArguments* RFactory::MergeTypeArguments(RTypeArguments* typeArgs0, RTypeArguments* typeArgs1)
{
    vector<RType*> items{typeArgs0->items}; // 복사
    items.insert(items.end(), typeArgs1->items.begin(), typeArgs1->items.end());

    RTypeArgumentsKeyView key{items};
    auto i = typeArgsMap.find(key);
    if (i != typeArgsMap.end())
        return i->second.get();

    unique_ptr<RTypeArguments> v{new RTypeArguments{move(items), this}};
    auto pv = v.get();
    typeArgsMap.emplace(vector<RType*>{pv->items.begin(), pv->items.end()}, move(v));
    return pv;
}

RType* RFactory::MakeType(RTypeDecl* decl, RTypeArguments* typeArgs)
{
    struct Visitor
    {
        using ResultType = RType*;

        RFactory& factory;
        RTypeArguments* typeArgs;

        RType* Visit(RClassDecl* classDecl)
        {
            return factory.MakeClassType(classDecl, typeArgs);
        }

        RType* Visit(RStructDecl* structDecl)
        {
            return factory.MakeStructType(structDecl, typeArgs);
        }

        RType* Visit(REnumDecl* enumDecl)
        {
            return factory.MakeEnumType(enumDecl, typeArgs);
        }

        RType* Visit(REnumElemDecl* enumElemDecl)
        {
            return factory.MakeEnumElemType(enumElemDecl, typeArgs);
        }

        RType* Visit(RInterfaceDecl* interfaceDecl)
        {
            return factory.MakeInterfaceType(interfaceDecl, typeArgs, false);
        }

        RType* Visit(RLambdaDecl* lambdaDecl)
        {
            return factory.MakeLambdaType(lambdaDecl, typeArgs);
        }

        RType* Visit(RTypeParam* typeParamDecl)
        {
            assert(typeArgs->GetCount() == 0);
            return factory.MakeTypeVarType(typeParamDecl);
        }

        RType* Visit(RTraitDecl* traitDecl)
        {
            // TODO: [66] 2026-07-09, Trait, Extend 구현
            // 에러, Trait로 타입을 만들 수 없습니다
            throw NotImplementedException{};
        }
    };

    return Accept(Visitor{*this, typeArgs}, decl);
}

RType* RFactory::MakeBoolType()
{
    return boolType.get();
}

RType* RFactory::MakeIntType()
{
    return intType.get();
}

RType* RFactory::MakeStringType()
{
    return stringType;
}

RType* RFactory::MakeListType(RType* itemType)
{
    auto* typeArgs = MakeTypeArguments({&itemType, 1});
    return MakeClassType(listDecl.get(), typeArgs);
}

RType* RFactory::MakeListIteratorType(RType* itemType)
{
    auto* typeArgs = MakeTypeArguments({&itemType, 1});
    return MakeStructType(listIterDecl.get(), typeArgs);
}

bool RFactory::IsListType(RType* type, RType** outItemType)
{
    auto* classType = dynamic_cast<RType_Class*>(type);
    if (!classType) return false;

    if (classType->decl != listDecl.get()) return false;
    if (classType->typeArgs->GetCount() != 1) return false;

    *outItemType = classType->typeArgs->Get(0);
    return true;
}

RNamespaceDeclGroup* RFactory::GetNamespaceDeclGroup(InRef<std::vector<RName>> name)
{
    auto i = nsGroupsMap.find(*name);
    if (i != nsGroupsMap.end())
        return i->second.get();
    
    auto newGroup = make_unique<RNamespaceDeclGroup>();
    auto pNewGroup = newGroup.get();
    nsGroupsMap.emplace(*name, move(newGroup));
    return pNewGroup;
}

RNamespaceDecl* RFactory::MakeRootNamespaceDecl(TakeRef<RFactoryPtr> rFactory)
{
    // root namespace면 
    auto* group = GetNamespaceDeclGroup(std::vector<RName>{});
    unique_ptr<RNamespaceDecl> newDecl{new RNamespaceDecl{nullptr, RName_None{}, group, rFactory.Take()}};
    auto pNewDecl = newDecl.get();
    decls.push_back(move(newDecl));

    group->Add(pNewDecl);
    return pNewDecl;
}

RNamespaceDecl* RFactory::MakeChildNamespaceDecl(RNamespaceDecl* outer, InRef<std::string> name, TakeRef<RFactoryPtr> rFactory)
{
    assert(outer && !name->empty());

    // root namespace면 
    vector<RName> ids;

    ids.push_back(RName_Normal{*name});
    auto curNS = outer;

    while (curNS)
    {
        auto curOuter = curNS->GetOuterNamespace();

        if (!curOuter)
        {
            // root 라면 그만둔다
            assert(curNS->GetName() == RName_None{});
            break;
        }

        ids.push_back(curNS->GetName());
        curNS = curOuter;
    }

    reverse(ids.begin(), ids.end());

    auto group = GetNamespaceDeclGroup(ids);
    unique_ptr<RNamespaceDecl> newDecl{new RNamespaceDecl{outer, RName_None{}, group, rFactory.Take()}};
    auto pNewDecl = newDecl.get();
    decls.push_back(move(newDecl));

    group->Add(pNewDecl);
    return pNewDecl;
}

RModule* RFactory::MakeModule(RName&& name)
{
    privateData->modules.push_back(RModule{move(name)});
    return &privateData->modules.back();
}


} // Citron