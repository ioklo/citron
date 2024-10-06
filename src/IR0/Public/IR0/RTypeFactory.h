#pragma once
#include "IR0Config.h"
#include <unordered_map>
#include <memory>

#include "RType.h" // for RFuncType::Parameter
#include "RTypeArguments.h"
#include "RDeclId.h"

namespace Citron {

namespace IR0 {

struct FuncTypeKey
{
    bool bLocal;
    RTypePtr retType;
    std::vector<RFuncType::Parameter> params;

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

struct InstanceTypeKey
{
    RDeclPtr decl;
    RTypeArgumentsPtr typeArgs;

    bool operator==(const InstanceTypeKey& other) const noexcept
    {
        return decl == other.decl && typeArgs == other.typeArgs;
    }
};

struct InstanceTypeKeyHasher
{
    size_t operator()(const InstanceTypeKey& key) const noexcept
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
    std::unordered_map<RTypePtr, std::shared_ptr<RNullableValueType>> nullableValueTypes;
    std::unordered_map<RTypePtr, std::shared_ptr<RNullableRefType>> nullableRefTypes;
    std::unordered_map<int, std::shared_ptr<RTypeVarType>> typeVarTypes;
    std::shared_ptr<RVoidType> voidType;
    std::unordered_map<std::vector<RTupleMemberVar>, std::shared_ptr<RTupleType>> tupleTypes;
    std::unordered_map<IR0::FuncTypeKey, std::shared_ptr<RFuncType>, IR0::FuncTypeKeyHasher> funcTypes;
    std::unordered_map<RTypePtr, std::shared_ptr<RLocalPtrType>> localPtrTypes;
    std::unordered_map<RTypePtr, std::shared_ptr<RBoxPtrType>> boxPtrTypes;
    std::unordered_map<IR0::InstanceTypeKey, std::shared_ptr<RInstanceType>, IR0::InstanceTypeKeyHasher> instanceTypes;
    std::unordered_map<IR0::TypeArgumentsKey, RTypeArgumentsPtr, IR0::TypeArgumentsKeyHasher> map;

public:
    IR0_API RTypeFactory();
    
    IR0_API std::shared_ptr<RNullableValueType> MakeNullableValueType(RTypePtr innerType);
    IR0_API std::shared_ptr<RNullableRefType> MakeNullableRefType(RTypePtr innerType);
    IR0_API std::shared_ptr<RTypeVarType> MakeTypeVarType(int index);
    IR0_API std::shared_ptr<RVoidType> MakeVoidType();
    IR0_API std::shared_ptr<RTupleType> MakeTupleType(std::vector<RTupleMemberVar>&& memberVar);
    IR0_API std::shared_ptr<RFuncType> MakeFuncType(bool bLocal, RTypePtr&& retType, std::vector<RFuncType::Parameter>&& params);
    IR0_API std::shared_ptr<RLocalPtrType> MakeLocalPtrType(RTypePtr&& innerType);
    IR0_API std::shared_ptr<RBoxPtrType> MakeBoxPtrType(RTypePtr&& innerType);
    IR0_API std::shared_ptr<RInstanceType> MakeInstanceType(const RDeclPtr& decl, const RTypeArgumentsPtr& typeArgs);
    IR0_API RTypeArgumentsPtr MakeTypeArguments(const std::vector<RTypePtr>& items);

    // utilities
    IR0_API RTypePtr MakeBoolType();
    IR0_API RTypePtr MakeIntType();
    IR0_API RTypePtr MakeStringType();
    IR0_API RTypePtr MakeListType(const RTypePtr& itemType);

    // declIds
    IR0_API RDeclIdPtr MakeDeclId(std::string&& moduleName, RIdentifier&& identifier);
    IR0_API RDeclIdPtr MakeChildDeclId(RDeclIdPtr&& id, RIdentifier&& identifier);
};

} // namespace Citron
