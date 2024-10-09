#pragma once
#include "IR0Config.h"
#include <unordered_map>
#include <memory>

#include "RType.h" // for RFuncType::Parameter
#include "RTypeArguments.h"
#include "RDeclId.h"

namespace Citron {

class RClassDecl;
class RStructDecl;
class REnumDecl;
class REnumElemDecl;
class RInterfaceDecl;
class RLambdaDecl;

namespace IR0 {

struct FuncTypeKey
{
    bool bLocal;
    RTypePtr retType;
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
    std::shared_ptr<TDecl> decl;
    RTypeArgumentsPtr typeArgs;

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
    std::vector<RTypePtr> items;

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
class RTypeFactory
{
    // inner type -> nullable type
    std::unordered_map<RTypePtr, std::shared_ptr<RType_NullableValue>> nullableValueTypes;
    std::unordered_map<RTypePtr, std::shared_ptr<RType_NullableRef>> nullableRefTypes;
    std::unordered_map<int, std::shared_ptr<RType_TypeVar>> typeVarTypes;
    std::shared_ptr<RType_Void> voidType;
    std::unordered_map<std::vector<RTupleMemberVar>, std::shared_ptr<RType_Tuple>> tupleTypes;
    std::unordered_map<IR0::FuncTypeKey, std::shared_ptr<RType_Func>, IR0::FuncTypeKeyHasher> funcTypes;
    std::unordered_map<RTypePtr, std::shared_ptr<RType_LocalPtr>> localPtrTypes;
    std::unordered_map<RTypePtr, std::shared_ptr<RType_BoxPtr>> boxPtrTypes;

    template<typename TDecl, typename TType>
    using InstanceTypeKeyUnorderedMap = std::unordered_map<IR0::InstanceTypeKey<TDecl>, std::shared_ptr<TType>, IR0::InstanceTypeKeyHasher<TDecl>>;

    InstanceTypeKeyUnorderedMap<RClassDecl, RType_Class> classTypes;
    InstanceTypeKeyUnorderedMap<RStructDecl, RType_Struct> structTypes;
    InstanceTypeKeyUnorderedMap<REnumDecl, RType_Enum> enumTypes;
    InstanceTypeKeyUnorderedMap<REnumElemDecl, RType_EnumElem> enumElemTypes;
    InstanceTypeKeyUnorderedMap<RInterfaceDecl, RType_Interface> interfaceTypes;
    InstanceTypeKeyUnorderedMap<RLambdaDecl, RType_Lambda> lambdaTypes;

    std::unordered_map<IR0::TypeArgumentsKey, RTypeArgumentsPtr, IR0::TypeArgumentsKeyHasher> map;

public:
    IR0_API RTypeFactory();
    
    IR0_API std::shared_ptr<RType_NullableValue> MakeNullableValueType(RTypePtr innerType);
    IR0_API std::shared_ptr<RType_NullableRef> MakeNullableRefType(RTypePtr innerType);
    IR0_API std::shared_ptr<RType_TypeVar> MakeTypeVarType(int index);
    IR0_API std::shared_ptr<RType_Void> MakeVoidType();
    IR0_API std::shared_ptr<RType_Tuple> MakeTupleType(std::vector<RTupleMemberVar>&& memberVar);
    IR0_API std::shared_ptr<RType_Func> MakeFuncType(bool bLocal, RTypePtr&& retType, std::vector<RType_Func::Parameter>&& params);
    IR0_API std::shared_ptr<RType_LocalPtr> MakeLocalPtrType(RTypePtr&& innerType);
    IR0_API std::shared_ptr<RType_BoxPtr> MakeBoxPtrType(RTypePtr&& innerType);

    IR0_API std::shared_ptr<RType_Class> MakeClassType(const std::shared_ptr<RClassDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    IR0_API std::shared_ptr<RType_Struct> MakeStructType(const std::shared_ptr<RStructDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    IR0_API std::shared_ptr<RType_Enum> MakeEnumType(const std::shared_ptr<REnumDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    IR0_API std::shared_ptr<RType_EnumElem> MakeEnumElemType(const std::shared_ptr<REnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    IR0_API std::shared_ptr<RType_Interface> MakeInterfaceType(const std::shared_ptr<RInterfaceDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    IR0_API std::shared_ptr<RType_Lambda> MakeLambdaType(const std::shared_ptr<RLambdaDecl>& decl, const RTypeArgumentsPtr& typeArgs);


    IR0_API RTypeArgumentsPtr MakeTypeArguments(const std::vector<RTypePtr>& items);
    IR0_API RTypeArgumentsPtr MergeTypeArguments(RTypeArguments& typeArgs0, RTypeArguments& typeArgs1);

    // utilities
    IR0_API RTypePtr MakeBoolType();
    IR0_API RTypePtr MakeIntType();
    IR0_API RTypePtr MakeStringType();
    IR0_API RTypePtr MakeListType(const RTypePtr& itemType);

    // declIds
    IR0_API RDeclIdPtr MakeDeclId(std::string&& moduleName, RIdentifier&& identifier);
    IR0_API RDeclIdPtr MakeChildDeclId(RDeclIdPtr&& id, RIdentifier&& identifier);

private:
    template<typename TDecl, typename TType>
    std::shared_ptr<TType> MakeInstanceType(InstanceTypeKeyUnorderedMap<TDecl, TType>& instanceTypes, const std::shared_ptr<TDecl>& decl, const RTypeArgumentsPtr& typeArgs);

};

} // namespace Citron
