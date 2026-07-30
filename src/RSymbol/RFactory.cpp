#include "RFactory.h"

#include <cassert>
#include <algorithm>
#include <unordered_set>

#include "Infra/Hash.h"
#include "Infra/Ptr.h"

#include "RNamespace.h"
#include "Infra/Hash.h"

#include "RModule.h"
#include "RTypes.h"
#include "RTypeDecl.h"
#include "RStructDecl.h"
#include "RClassDecl.h"
#include "REnumDecl.h"
#include "REnumElemDecl.h"
#include "RInterfaceDecl.h"
#include "RLambdaDecl.h"

#include "RTypeArguments.h"
#include "RTypeParam.h"
#include "RTraitDecl.h"

using namespace std;

namespace Citron {

struct RTypeArgumentsKeyView
{
    std::span<RType*> items;
};

struct RTypeArgumentsKeyView2
{
    std::span<RType*> items0;
    std::span<RType*> items1;
};

struct RTypeArgumentsPtrHasher
{
    using is_transparent = void;

    size_t operator()(RTypeArguments* typeArgs) const noexcept
    {
        return Hash(typeArgs->items);
    }

    size_t operator()(const RTypeArgumentsKeyView& key) const noexcept
    {
        return Hash(key.items);
    }

    size_t operator()(const RTypeArgumentsKeyView2& key) const noexcept
    {
        size_t s = 0;

        for (auto* item : key.items0)
            Citron::hash_combine(s, item);

        for (auto* item : key.items1)
            Citron::hash_combine(s, item);

        return s;
    }

private:
    template<typename TItems>
    static size_t Hash(const TItems& items) noexcept
    {
        size_t s = 0;

        for (auto* item : items)
            Citron::hash_combine(s, item);

        return s;
    }
};

struct RTypeArgumentsPtrEqual
{
    using is_transparent = void;

    bool operator()(RTypeArguments* x, RTypeArguments* y) const noexcept
    {
        return x->items == y->items;
    }

    bool operator()(RTypeArguments* x, const RTypeArgumentsKeyView& y) const noexcept
    {
        return Equal(x->items, y.items);
    }

    bool operator()(const RTypeArgumentsKeyView& x, RTypeArguments* y) const noexcept
    {
        return Equal(y->items, x.items);
    }

    bool operator()(const RTypeArgumentsKeyView2& x, RTypeArguments* y) const noexcept
    {
        return Equal(y->items, x.items0, x.items1);
    }

    bool operator()(RTypeArguments* x, const RTypeArgumentsKeyView2& y) const noexcept
    {
        return Equal(x->items, y.items0, y.items1);
    }

private:
    static bool Equal(std::span<RType* const> x, std::span<RType*> y) noexcept
    {
        if (x.size() != y.size())
            return false;

        return std::equal(x.begin(), x.end(), y.begin());
    }

    static bool Equal(std::span<RType* const> x, std::span<RType*> y0, std::span<RType*> y1) noexcept
    {
        if (x.size() != y0.size() + y1.size())
            return false;

        size_t i = 0;
        for (size_t countY0 = y0.size(); i < countY0; i++)
            if (x[i] != y0[i]) return false;

        for (size_t j = 0, countY1 = y1.size(); j < countY1; i++, j++)
            if (x[i] != y1[j]) return false;

        return true;
    }
};


struct RType_OpaqueKey
{
    RAppliedDecl<RTraitDecl> appliedTrait;
    RAppliedDecl<RDecl> appliedOwnerFunc;

    bool operator==(const RType_OpaqueKey& other) const noexcept
    {
        return appliedTrait.decl == other.appliedTrait.decl && appliedTrait.typeArgs == other.appliedTrait.typeArgs &&
            appliedOwnerFunc.decl == other.appliedOwnerFunc.decl && appliedOwnerFunc.typeArgs == other.appliedOwnerFunc.typeArgs;
    }
};

struct RType_OpaquePtrHasher 
{
    using is_transparent = void;

    size_t operator()(RType_Opaque* opaqueType) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, opaqueType->appliedTrait);
        Citron::hash_combine(s, opaqueType->appliedOwnerFunc);
        return s;
    }

    size_t operator()(const RType_OpaqueKey& key) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, key.appliedTrait);
        Citron::hash_combine(s, key.appliedOwnerFunc);
        return s;
    }
};

struct RType_OpaquePtrKeyEq
{
    using is_transparent = void;

    bool operator()(RType_Opaque* lhs, RType_Opaque* rhs) const noexcept
    {
        return lhs->appliedTrait.decl == rhs->appliedTrait.decl && lhs->appliedTrait.typeArgs == rhs->appliedTrait.typeArgs &&
            lhs->appliedOwnerFunc.decl == rhs->appliedOwnerFunc.decl && lhs->appliedOwnerFunc.typeArgs == rhs->appliedOwnerFunc.typeArgs;
    }

    bool operator()(RType_Opaque* lhs, const RType_OpaqueKey& rhs) const noexcept
    {
        return lhs->appliedTrait.decl == rhs.appliedTrait.decl && lhs->appliedTrait.typeArgs == rhs.appliedTrait.typeArgs &&
            lhs->appliedOwnerFunc.decl == rhs.appliedOwnerFunc.decl && lhs->appliedOwnerFunc.typeArgs == rhs.appliedOwnerFunc.typeArgs;
    }

    bool operator()(const RType_OpaqueKey& lhs, RType_Opaque* rhs) const noexcept
    {
        return lhs.appliedTrait.decl == rhs->appliedTrait.decl && lhs.appliedTrait.typeArgs == rhs->appliedTrait.typeArgs &&
            lhs.appliedOwnerFunc.decl == rhs->appliedOwnerFunc.decl && lhs.appliedOwnerFunc.typeArgs == rhs->appliedOwnerFunc.typeArgs;
    }
};

struct RFactoryPrivateData
{
    std::deque<RModule> modules;
    std::deque<RTypeParam> typeParams;

    std::deque<RTypeArguments> typeArgsStorage;
    std::unordered_set<RTypeArguments*, RTypeArgumentsPtrHasher, RTypeArgumentsPtrEqual> typeArgsSet;


    std::deque<RType_Opaque> opaqueTypes;
    std::unordered_set<RType_Opaque*, RType_OpaquePtrHasher, RType_OpaquePtrKeyEq> opaqueTypeSet;
};

RFactoryPtr RFactory::Make()
{
    shared_ptr<RFactory> factory{new RFactory{}};
    auto* tempModule = factory->MakeModule(RModuleName{"__TEMP__"});
    auto* tempRootNamespace = factory->MakeRootNamespaceDecl(tempModule, factory);
    tempModule->InitRootNamespace(tempRootNamespace);
    auto* tempSystemNamespace = factory->MakeChildNamespaceDecl(tempRootNamespace, "System", factory);

    // TODO: 아직 MakeStructType, MakeClassType과는 연결이 되지 않은 상태
    auto* tempString = factory->MakeDecl<RStructDecl>(RDeclKey::Normal(RName::Normal("String")), RTypeDeclOuter_Namespace{tempSystemNamespace, RNamespaceMemberAccessor::Public}, RName::Normal("String"), factory);
    tempString->InitTypeParams({});
    tempString->InitTraits({});
    auto* stringType = factory->MakeStructType(tempString, factory->MakeEmptyTypeArguments());
    factory->Init(stringType);

    return factory;
}

RFactory::RFactory()
    : voidType{new RType_Void()}
    , boolType{new RType_Primitive(RType_PrimitiveKind::Bool)}
    , intType{new RType_Primitive(RType_PrimitiveKind::Int32)}
    , privateData{new RFactoryPrivateData()}
{   
}

void RFactory::Init(RType* stringType)
{
    this->stringType = stringType;
}


RFactory::~RFactory()
{
}

RTypeParam* RFactory::MakeTypeParam(RDecl* rDecl, RName&& name, size_t index, TakeRef<RFactoryPtr> rFactory)
{
    auto& typeParam = privateData->typeParams.emplace_back(rDecl, move(name), index, move(rFactory));
    return &typeParam;
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

RType_Opaque* RFactory::MakeOpaqueType(RAppliedDecl<RTraitDecl>&& appliedTrait, RAppliedDecl<RDecl>&& appliedOwnerFunc)
{
    auto i = privateData->opaqueTypeSet.find(RType_OpaqueKey{appliedTrait, appliedOwnerFunc});
    if (i != privateData->opaqueTypeSet.end())
        return *i;

    // move로 인한 key 무효화 (실제로는 move를 지원하지 않기 때문에 무효화되지 않는다)
    auto& newType = privateData->opaqueTypes.emplace_back(move(appliedTrait), move(appliedOwnerFunc), this, RType_Opaque::PrivateKey{});
    privateData->opaqueTypeSet.insert(&newType);
    return &newType;
}

RTypeArguments* RFactory::MakeTypeArguments(span<RType*> items)
{
    auto i = privateData->typeArgsSet.find(RTypeArgumentsKeyView{items});
    if (i != privateData->typeArgsSet.end())
        return *i;
    
    auto& newTypeArgs = privateData->typeArgsStorage.emplace_back(vector<RType*>{items.begin(), items.end()}, this, RTypeArguments::PrivateKey{});
    privateData->typeArgsSet.insert(&newTypeArgs);
    return &newTypeArgs;
}

RTypeArguments* RFactory::MakeTypeArguments(std::vector<RType*>&& items)
{
     auto i = privateData->typeArgsSet.find(RTypeArgumentsKeyView{items});
    if (i != privateData->typeArgsSet.end())
        return *i;

    auto& newTypeArgs = privateData->typeArgsStorage.emplace_back(move(items), this, RTypeArguments::PrivateKey{});
    privateData->typeArgsSet.insert(&newTypeArgs);
    return &newTypeArgs;
}

RTypeArguments* RFactory::MakeEmptyTypeArguments()
{
    auto i = privateData->typeArgsSet.find(RTypeArgumentsKeyView{});
    if (i != privateData->typeArgsSet.end())
        return *i;

    auto& newTypeArgs = privateData->typeArgsStorage.emplace_back(vector<RType*>{}, this, RTypeArguments::PrivateKey{});
    privateData->typeArgsSet.insert(&newTypeArgs);
    return &newTypeArgs;
}

RTypeArguments* RFactory::MergeTypeArguments(RTypeArguments* typeArgs0, RTypeArguments* typeArgs1)
{   
    return AppendTypeArguments(typeArgs0, typeArgs1->items);
}

RTypeArguments* RFactory::AppendTypeArguments(RTypeArguments* typeArgs, std::span<RType*> typeArgSpan)
{   
    auto i = privateData->typeArgsSet.find(RTypeArgumentsKeyView2{typeArgs->items, typeArgSpan});
    if (i != privateData->typeArgsSet.end())
        return *i;

    vector<RType*> items;
    items.reserve(typeArgs->GetCount() + typeArgSpan.size());
    items.insert(items.end(), typeArgs->items.begin(), typeArgs->items.end());
    items.insert(items.end(), typeArgSpan.begin(), typeArgSpan.end());

    auto& newTypeArgs = privateData->typeArgsStorage.emplace_back(move(items), this, RTypeArguments::PrivateKey{});
    privateData->typeArgsSet.insert(&newTypeArgs);
    return &newTypeArgs;
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

RNamespace* RFactory::MakeRootNamespaceDecl(RModule* module, TakeRef<RFactoryPtr> rFactory)
{
    // root namespace면
    unique_ptr<RNamespace> newDecl{new RNamespace{RDeclKey::RootNamespace(), RNamespaceKind_Root{module}, rFactory.Take()}};
    auto pNewDecl = newDecl.get();
    decls.push_back(move(newDecl));
    return pNewDecl;
}

void FillIdentifierWithoutModule(RDecl* decl, string& buffer)
{
    auto& key = decl->GetDeclKey();

    if (key == RDeclKey::RootNamespace()) return;

    FillIdentifierWithoutModule(decl->GetOuter(), buffer);
    buffer += ".";
    buffer += key.GetValue();
}

RNamespace* RFactory::MakeChildNamespaceDecl(RNamespace* outer, string_view name, TakeRef<RFactoryPtr> rFactory)
{
    assert(outer && !name.empty());

    unique_ptr<RNamespace> newDecl{new RNamespace{RDeclKey::Normal(RName::Normal(string{name})), RNamespaceKind_Normal{outer, RName_Normal{string{name}}}, rFactory.Take()}};
    auto pNewDecl = newDecl.get();
    decls.push_back(move(newDecl));
    return pNewDecl;
}

RModule* RFactory::MakeModule(RModuleName&& name)
{
    auto& module = privateData->modules.emplace_back(RModule{move(name)});
    return &module;
}


} // Citron