#pragma once
#include "RSymbolConfig.h"
#include <unordered_map>
#include <span>
#include <memory>
#include <deque>
#include "Infra/Ref.h"
#include "Infra/Hash.h"
#include "RTypes.h" // for RFuncType::Parameter
#include "RModule.h"

namespace Citron {

class RDecl;
class RModule;
class RTypeArguments;
class RFactory;
class RTypeDecl;
using RFactoryPtr = std::shared_ptr<RFactory>;

struct RFuncTypeKey
{
    bool bLocal;
    RType* retType;
    std::vector<RType_Func::Parameter> params;

    bool operator==(const RFuncTypeKey& other) const noexcept
    {
        return bLocal == other.bLocal && retType == other.retType && params == other.params;
    }
};

struct RFuncTypeKeyHasher
{
    size_t operator()(const RFuncTypeKey& key) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, key.bLocal);
        Citron::hash_combine(s, key.retType);
        Citron::hash_combine(s, key.params);
        return s;
    }
};

template<typename TDecl>
struct RInstanceTypeKey
{
    TDecl* decl;
    RTypeArguments* typeArgs;

    bool operator==(const RInstanceTypeKey& other) const noexcept
    {
        return decl == other.decl && typeArgs == other.typeArgs;
    }
};

template<typename TDecl>
struct RInstanceTypeKeyHasher
{
    size_t operator()(const RInstanceTypeKey<TDecl>& key) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, key.decl);
        Citron::hash_combine(s, key.typeArgs);
        return s;
    }
};

struct RFactoryPrivateData;
using RFactoryPtr = std::shared_ptr<class RFactory>;

// TODO: weak처리
// flyweight
class RFactory
{
    std::unique_ptr<RFactoryPrivateData> privateData;
    std::deque<std::unique_ptr<RDecl>> decls;
    
    // inner type -> nullable type
    std::unordered_map<RType*, std::unique_ptr<RType_Nullable>> nullableValueTypes;
    std::unordered_map<RType*, std::unique_ptr<RType_NullableInplace>> nullableRefTypes;
    std::unordered_map<RTypeParam*, std::unique_ptr<RType_TypeVar>> typeVarTypes;
    std::unique_ptr<RType_Void> voidType;
    std::unordered_map<std::vector<RTupleVar>, std::unique_ptr<RType_Tuple>> tupleTypes;
    std::unordered_map<RFuncTypeKey, std::unique_ptr<RType_Func>, RFuncTypeKeyHasher> funcTypes;
    std::unordered_map<RType*, std::unique_ptr<RType_Ptr>> ptrTypes;
    std::unordered_map<RType*, std::unique_ptr<RType_Shared>> sharedTypes;
    std::unordered_map<RType*, std::unique_ptr<RType_Box>> boxTypes;

    template<typename TDecl, typename TType>
    using InstanceTypeKeyUnorderedMap = std::unordered_map<RInstanceTypeKey<TDecl>, std::unique_ptr<TType>, RInstanceTypeKeyHasher<TDecl>>;

    InstanceTypeKeyUnorderedMap<RClassDecl, RType_Class> classTypes;
    InstanceTypeKeyUnorderedMap<RStructDecl, RType_Struct> structTypes;
    InstanceTypeKeyUnorderedMap<REnumDecl, RType_Enum> enumTypes;
    InstanceTypeKeyUnorderedMap<REnumElemDecl, RType_EnumElem> enumElemTypes;
    InstanceTypeKeyUnorderedMap<RInterfaceDecl, RType_Interface> interfaceTypes;
    InstanceTypeKeyUnorderedMap<RLambdaDecl, RType_Lambda> lambdaTypes;
   

    // 기본 타입
    std::unique_ptr<RType> boolType;
    std::unique_ptr<RType> intType;

    RType* stringType;

    std::unique_ptr<RClassDecl> listDecl;
    std::unique_ptr<RStructDecl> listIterDecl;

public:
    RSYMBOL_API static RFactoryPtr Make();
    
private:
    RSYMBOL_API RFactory();
    RSYMBOL_API void Init(RType* stringType);

public:
    RSYMBOL_API ~RFactory();

    template<typename TDecl>
    TDecl* MakeDecl(auto&&... args)
    {   
        auto decl = std::make_unique<TDecl>(std::forward<decltype(args)>(args)...);
        auto* pDecl = decl.get();
        decls.push_back(std::move(decl));
        return pDecl;
    }

    RSYMBOL_API RTypeParam* MakeTypeParam(RDecl* rDecl, RName&& name, size_t index, TakeRef<RFactoryPtr> rFactory);

    RSYMBOL_API RType_Nullable* MakeNullableType(RType* innerType);
    RSYMBOL_API RType_NullableInplace* MakeNullableInplaceType(RType* innerType);
    RSYMBOL_API RType_TypeVar* MakeTypeVarType(RTypeParam* decl);
    RSYMBOL_API RType_Void* MakeVoidType();
    RSYMBOL_API RType_Tuple* MakeTupleType(std::vector<RTupleVar>&& vars);
    RSYMBOL_API RType_Func* MakeFuncType(bool bLocal, RType* retType, std::vector<RType_Func::Parameter>&& params);
    RSYMBOL_API RType_Ptr* MakePtrType(RType* innerType);
    RSYMBOL_API RType_Shared* MakeSharedType(RType* innerType);
    RSYMBOL_API RType_Box* MakeBoxType(RType* innerType);

    RSYMBOL_API RType_Class* MakeClassType(RClassDecl* decl, RTypeArguments* typeArgs);
    RSYMBOL_API RType_Struct* MakeStructType(RStructDecl* decl, RTypeArguments* typeArgs);
    RSYMBOL_API RType_Enum* MakeEnumType(REnumDecl* decl, RTypeArguments* typeArgs);
    RSYMBOL_API RType_EnumElem* MakeEnumElemType(REnumElemDecl* decl, RTypeArguments* typeArgs);
    RSYMBOL_API RType_Interface* MakeInterfaceType(RInterfaceDecl* decl, RTypeArguments* typeArgs, bool bLocal);
    RSYMBOL_API RType_Lambda* MakeLambdaType(RLambdaDecl* decl, RTypeArguments* typeArgs);
    RSYMBOL_API RType_Opaque* MakeOpaqueType(RAppliedDecl<RTraitDecl>&& appliedTrait, RAppliedDecl<RDecl>&& appliedOwnerFunc);
    

    RSYMBOL_API RTypeArguments* MakeTypeArguments(std::span<RType*> items);
    RSYMBOL_API RTypeArguments* MakeTypeArguments(std::vector<RType*>&& items);
    RSYMBOL_API RTypeArguments* MakeEmptyTypeArguments();
    RSYMBOL_API RTypeArguments* MergeTypeArguments(RTypeArguments* typeArgs0, RTypeArguments* typeArgs1);
    RSYMBOL_API RTypeArguments* AppendTypeArguments(RTypeArguments* typeArgs, std::span<RType*> typeArgSpan);
    
    // utilities
    RSYMBOL_API RType* MakeBoolType();
    RSYMBOL_API RType* MakeIntType();
    RSYMBOL_API RType* MakeStringType();
    RSYMBOL_API RType* MakeListType(RType* itemType);
    RSYMBOL_API RType* MakeListIteratorType(RType* itemType);

    RSYMBOL_API bool IsListType(RType* type, RType** outItemType);
    
    RSYMBOL_API RNamespace* MakeRootNamespaceDecl(RModule* module, TakeRef<RFactoryPtr> rFactory);
    RSYMBOL_API RNamespace* MakeChildNamespaceDecl(RNamespace* outer, std::string_view name, TakeRef<RFactoryPtr> rFactory);

    RSYMBOL_API RModule* MakeModule(RModuleName&& name);
    
private:
    template<typename TDecl, typename TType, typename... TArgs>
    TType* MakeInstanceType(InstanceTypeKeyUnorderedMap<TDecl, TType>& instanceTypes, TDecl* decl, RTypeArguments* typeArgs, TArgs&&... args);
};

} // namespace Citron
