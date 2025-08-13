#pragma once
#include "IR0Config.h"

#include <unordered_map>
#include <memory>

#include "RTypes.h" // for RFuncType::Parameter

namespace Citron {

class RNamespaceDeclGroup;
class RTypeArguments;
class IR0Factory;

class NNamespaceDecl;

namespace IR0 {

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

} // namespace IR0

// TODO: weak처리
// flyweight
class IR0Factory
{
    // inner type -> nullable type
    std::unordered_map<RType*, std::unique_ptr<RType_NullableValue>> nullableValueTypes;
    std::unordered_map<RType*, std::unique_ptr<RType_NullableRef>> nullableRefTypes;
    std::unordered_map<int, std::unique_ptr<RType_TypeVar>> typeVarTypes;
    std::unique_ptr<RType_Void> voidType;
    std::unordered_map<std::vector<RTupleVar>, std::unique_ptr<RType_Tuple>> tupleTypes;
    std::unordered_map<IR0::FuncTypeKey, std::unique_ptr<RType_Func>, IR0::FuncTypeKeyHasher> funcTypes;
    std::unordered_map<RType*, std::unique_ptr<RType_LocalPtr>> localPtrTypes;
    std::unordered_map<RType*, std::unique_ptr<RType_BoxPtr>> boxPtrTypes;

    template<typename TDecl, typename TType>
    using InstanceTypeKeyUnorderedMap = std::unordered_map<IR0::InstanceTypeKey<TDecl>, std::unique_ptr<TType>, IR0::InstanceTypeKeyHasher<TDecl>>;

    InstanceTypeKeyUnorderedMap<RClassDecl, RType_Class> classTypes;
    InstanceTypeKeyUnorderedMap<RStructDecl, RType_Struct> structTypes;
    InstanceTypeKeyUnorderedMap<REnumDecl, RType_Enum> enumTypes;
    InstanceTypeKeyUnorderedMap<REnumElemDecl, RType_EnumElem> enumElemTypes;
    InstanceTypeKeyUnorderedMap<RInterfaceDecl, RType_Interface> interfaceTypes;
    InstanceTypeKeyUnorderedMap<RLambdaDecl, RType_Lambda> lambdaTypes;

    std::unordered_map<IR0::TypeArgumentsKey, std::unique_ptr<RTypeArguments>, IR0::TypeArgumentsKeyHasher> typeArgsMap;

    // 기본 타입
    std::unique_ptr<RType> boolType;
    std::unique_ptr<RType> intType;
    std::unique_ptr<RType> stringType;

    std::unique_ptr<RClassDecl> listDecl;

    std::vector<std::unique_ptr<NNamespaceDecl>> namespaceDecls;
    // namespace group
    std::unordered_map<std::vector<std::string>, std::unique_ptr<RNamespaceDeclGroup>> nsGroupsMap;

public:
    IR0_API IR0Factory();

    IR0_API RType_NullableValue* MakeNullableValueType(RType* innerType);
    IR0_API RType_NullableRef* MakeNullableRefType(RType* innerType);
    IR0_API RType_TypeVar* MakeTypeVarType(int index);
    IR0_API RType_Void* MakeVoidType();
    IR0_API RType_Tuple* MakeTupleType(std::vector<RTupleVar>&& vars);
    IR0_API RType_Func* MakeFuncType(bool bLocal, RType* retType, std::vector<RType_Func::Parameter>&& params);
    IR0_API RType_LocalPtr* MakeLocalPtrType(RType* innerType);
    IR0_API RType_BoxPtr* MakeBoxPtrType(RType* innerType);

    IR0_API RType_Class* MakeClassType(RClassDecl* decl, RTypeArguments* typeArgs);
    IR0_API RType_Struct* MakeStructType(RStructDecl* decl, RTypeArguments* typeArgs);
    IR0_API RType_Enum* MakeEnumType(REnumDecl* decl, RTypeArguments* typeArgs);
    IR0_API RType_EnumElem* MakeEnumElemType(REnumElemDecl* decl, RTypeArguments* typeArgs);
    IR0_API RType_Interface* MakeInterfaceType(RInterfaceDecl* decl, RTypeArguments* typeArgs, bool bLocal);
    IR0_API RType_Lambda* MakeLambdaType(RLambdaDecl* decl, RTypeArguments* typeArgs);

    IR0_API RTypeArguments* MakeTypeArguments(const std::vector<RType*>& items);
    IR0_API RTypeArguments* MergeTypeArguments(RTypeArguments& typeArgs0, RTypeArguments& typeArgs1);

    // utilities
    IR0_API RType* MakeBoolType();
    IR0_API RType* MakeIntType();
    IR0_API RType* MakeStringType();
    IR0_API RType* MakeListType(RType* itemType);

    IR0_API bool IsListType(RType* type, RType** outItemType);

    NNamespaceDecl* NewRootNamespaceDecl(); // TU당 하나씩 만들어지는 namespace
    NNamespaceDecl* NewChildNamespaceDecl(NNamespaceDecl* outer, const std::string& name);
    RNamespaceDeclGroup* GetNamespaceDeclGroup(const std::vector<std::string>& name);

private:
    template<typename TDecl, typename TType, typename... TArgs>
    TType* MakeInstanceType(InstanceTypeKeyUnorderedMap<TDecl, TType>& instanceTypes, TDecl* decl, RTypeArguments* typeArgs, TArgs&&... args);
};

using IR0FactoryPtr = std::shared_ptr<IR0Factory>;

} // namespace Citron
