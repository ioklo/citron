#pragma once
#include "RSymbolConfig.h"

#include <unordered_map>
#include <memory>

#include "RTypes.h" // for RFuncType::Parameter

namespace Citron {

class RNamespaceDeclGroup;
class RTypeArguments;
class RFactory;

namespace RSymbols {

struct FuncTypeKey
{
    bool bLocal;
    RType* retType;
    std::vector<RType_Func::Parameter> params;

    bool operator==(const FuncTypeKey& other) const noexcept
    {
        return bLocal == other.bLocal && retType == other.retType && params == other.params;
    }
};

struct FuncTypeKeyHasher
{
    size_t operator()(const FuncTypeKey& key) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, key.bLocal);
        Citron::hash_combine(s, key.retType);
        Citron::hash_combine(s, key.params);
        return s;
    }
};

template<typename TDecl>
struct InstanceTypeKey
{
    TDecl* decl;
    RTypeArguments* typeArgs;

    bool operator==(const InstanceTypeKey& other) const noexcept
    {
        return decl == other.decl && typeArgs == other.typeArgs;
    }
};

template<typename TDecl>
struct InstanceTypeKeyHasher
{
    size_t operator()(const InstanceTypeKey<TDecl>& key) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, key.decl);
        Citron::hash_combine(s, key.typeArgs);
        return s;
    }
};

struct TypeArgumentsKey
{
    std::vector<RType*> items;

    bool operator==(const TypeArgumentsKey& other) const noexcept
    {
        return items == other.items;
    }
};

struct TypeArgumentsKeyHasher
{
    size_t operator()(const TypeArgumentsKey& key) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, key.items);
        return s;
    }
};

} // namespace RSymbols

// TODO: weak처리
// flyweight
class RFactory
{
    // inner type -> nullable type
    std::unordered_map<RType*, std::unique_ptr<RType_NullableValue>> nullableValueTypes;
    std::unordered_map<RType*, std::unique_ptr<RType_NullableRef>> nullableRefTypes;
    std::unordered_map<int, std::unique_ptr<RType_TypeVar>> typeVarTypes;
    std::unique_ptr<RType_Void> voidType;
    std::unordered_map<std::vector<RTupleVar>, std::unique_ptr<RType_Tuple>> tupleTypes;
    std::unordered_map<RSymbols::FuncTypeKey, std::unique_ptr<RType_Func>, RSymbols::FuncTypeKeyHasher> funcTypes;
    std::unordered_map<RType*, std::unique_ptr<RType_LocalPtr>> localPtrTypes;
    std::unordered_map<RType*, std::unique_ptr<RType_BoxPtr>> boxPtrTypes;

    template<typename TDecl, typename TType>
    using InstanceTypeKeyUnorderedMap = std::unordered_map<RSymbols::InstanceTypeKey<TDecl>, std::unique_ptr<TType>, RSymbols::InstanceTypeKeyHasher<TDecl>>;

    InstanceTypeKeyUnorderedMap<RClassDecl, RType_Class> classTypes;
    InstanceTypeKeyUnorderedMap<RStructDecl, RType_Struct> structTypes;
    InstanceTypeKeyUnorderedMap<REnumDecl, RType_Enum> enumTypes;
    InstanceTypeKeyUnorderedMap<REnumElemDecl, RType_EnumElem> enumElemTypes;
    InstanceTypeKeyUnorderedMap<RInterfaceDecl, RType_Interface> interfaceTypes;
    InstanceTypeKeyUnorderedMap<RLambdaDecl, RType_Lambda> lambdaTypes;

    std::unordered_map<RSymbols::TypeArgumentsKey, std::unique_ptr<RTypeArguments>, RSymbols::TypeArgumentsKeyHasher> typeArgsMap;

    // 기본 타입
    std::unique_ptr<RType> boolType;
    std::unique_ptr<RType> intType;
    std::unique_ptr<RType> stringType;

    std::unique_ptr<RClassDecl> listDecl;

    // namespace group
    std::unordered_map<std::vector<std::string>, std::unique_ptr<RNamespaceDeclGroup>> nsGroupsMap;

public:
    RSYMBOL_API RFactory();
    RSYMBOL_API ~RFactory();

    RSYMBOL_API RType_NullableValue* MakeNullableValueType(RType* innerType);
    RSYMBOL_API RType_NullableRef* MakeNullableRefType(RType* innerType);
    RSYMBOL_API RType_TypeVar* MakeTypeVarType(int index);
    RSYMBOL_API RType_Void* MakeVoidType();
    RSYMBOL_API RType_Tuple* MakeTupleType(std::vector<RTupleVar>&& vars);
    RSYMBOL_API RType_Func* MakeFuncType(bool bLocal, RType* retType, std::vector<RType_Func::Parameter>&& params);
    RSYMBOL_API RType_LocalPtr* MakeLocalPtrType(RType* innerType);
    RSYMBOL_API RType_BoxPtr* MakeBoxPtrType(RType* innerType);

    RSYMBOL_API RType_Class* MakeClassType(RClassDecl* decl, RTypeArguments* typeArgs);
    RSYMBOL_API RType_Struct* MakeStructType(RStructDecl* decl, RTypeArguments* typeArgs);
    RSYMBOL_API RType_Enum* MakeEnumType(REnumDecl* decl, RTypeArguments* typeArgs);
    RSYMBOL_API RType_EnumElem* MakeEnumElemType(REnumElemDecl* decl, RTypeArguments* typeArgs);
    RSYMBOL_API RType_Interface* MakeInterfaceType(RInterfaceDecl* decl, RTypeArguments* typeArgs, bool bLocal);
    RSYMBOL_API RType_Lambda* MakeLambdaType(RLambdaDecl* decl, RTypeArguments* typeArgs);

    RSYMBOL_API RTypeArguments* MakeTypeArguments(const std::vector<RType*>& items);
    RSYMBOL_API RTypeArguments* MergeTypeArguments(RTypeArguments& typeArgs0, RTypeArguments& typeArgs1);

    // utilities
    RSYMBOL_API RType* MakeBoolType();
    RSYMBOL_API RType* MakeIntType();
    RSYMBOL_API RType* MakeStringType();
    RSYMBOL_API RType* MakeListType(RType* itemType);

    RSYMBOL_API bool IsListType(RType* type, RType** outItemType);
    
    // Reference Module까지 아우를 수 있는 DeclGroup
    RSYMBOL_API RNamespaceDeclGroup* GetNamespaceDeclGroup(const std::vector<std::string>& name);
    
private:
    template<typename TDecl, typename TType, typename... TArgs>
    TType* MakeInstanceType(InstanceTypeKeyUnorderedMap<TDecl, TType>& instanceTypes, TDecl* decl, RTypeArguments* typeArgs, TArgs&&... args);
};

using RFactoryPtr = std::shared_ptr<RFactory>;

} // namespace Citron
