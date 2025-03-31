export module Citron.RDecls:RTypeFactory;

import "IR0Config.h";
import <unordered_map>;
import <memory>;

import :RTypes; // for RFuncType::Parameter

namespace Citron {

export class RNamespaceDeclGroup;
export using RNamespaceDeclGroupPtr = std::shared_ptr<RNamespaceDeclGroup>;

export class RTypeArguments;
export using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

export class RTypeFactory;

namespace IR0 {

export struct FuncTypeKey
{
    bool bLocal;
    RTypePtr retType;
    std::vector<RType_Func::Parameter> params;

    bool operator==(const FuncTypeKey& other) const noexcept
    {
        return bLocal == other.bLocal && retType == other.retType && params == other.params;
    }
};

export struct FuncTypeKeyHasher
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

export template<typename TDecl>
struct InstanceTypeKey
{
    std::shared_ptr<TDecl> decl;
    RTypeArgumentsPtr typeArgs;

    bool operator==(const InstanceTypeKey& other) const noexcept
    {
        return decl == other.decl && typeArgs == other.typeArgs;
    }
};

export template<typename TDecl>
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

export struct TypeArgumentsKey
{
    std::vector<RTypePtr> items;

    bool operator==(const TypeArgumentsKey& other) const noexcept
    {
        return items == other.items;
    }
};

export struct TypeArgumentsKeyHasher
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
export class RTypeFactory
{
    // inner type -> nullable type
    std::unordered_map<RTypePtr, std::shared_ptr<RType_NullableValue>> nullableValueTypes;
    std::unordered_map<RTypePtr, std::shared_ptr<RType_NullableRef>> nullableRefTypes;
    std::unordered_map<int, std::shared_ptr<RType_TypeVar>> typeVarTypes;
    std::shared_ptr<RType_Void> voidType;
    std::unordered_map<std::vector<RTupleVar>, std::shared_ptr<RType_Tuple>> tupleTypes;
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

    std::unordered_map<IR0::TypeArgumentsKey, RTypeArgumentsPtr, IR0::TypeArgumentsKeyHasher> typeArgsMap;

    // 기본 타입
    RTypePtr boolType;
    RTypePtr intType;
    RTypePtr stringType;

    std::shared_ptr<RClassDecl> listDecl;

    // namespace group
    std::unordered_map<std::vector<std::string>, RNamespaceDeclGroupPtr> nsGroupsMap;

public:
    IR0_API RTypeFactory();

    IR0_API std::shared_ptr<RType_NullableValue> MakeNullableValueType(RTypePtr innerType);
    IR0_API std::shared_ptr<RType_NullableRef> MakeNullableRefType(RTypePtr innerType);
    IR0_API std::shared_ptr<RType_TypeVar> MakeTypeVarType(int index);
    IR0_API std::shared_ptr<RType_Void> MakeVoidType();
    IR0_API std::shared_ptr<RType_Tuple> MakeTupleType(std::vector<RTupleVar>&& vars);
    IR0_API std::shared_ptr<RType_Func> MakeFuncType(bool bLocal, RTypePtr&& retType, std::vector<RType_Func::Parameter>&& params);
    IR0_API std::shared_ptr<RType_LocalPtr> MakeLocalPtrType(RTypePtr&& innerType);
    IR0_API std::shared_ptr<RType_BoxPtr> MakeBoxPtrType(RTypePtr&& innerType);

    IR0_API std::shared_ptr<RType_Class> MakeClassType(const std::shared_ptr<RClassDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    IR0_API std::shared_ptr<RType_Struct> MakeStructType(const std::shared_ptr<RStructDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    IR0_API std::shared_ptr<RType_Enum> MakeEnumType(const std::shared_ptr<REnumDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    IR0_API std::shared_ptr<RType_EnumElem> MakeEnumElemType(const std::shared_ptr<REnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    IR0_API std::shared_ptr<RType_Interface> MakeInterfaceType(const std::shared_ptr<RInterfaceDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool bLocal);
    IR0_API std::shared_ptr<RType_Lambda> MakeLambdaType(const std::shared_ptr<RLambdaDecl>& decl, const RTypeArgumentsPtr& typeArgs);


    IR0_API RTypeArgumentsPtr MakeTypeArguments(const std::vector<RTypePtr>& items);
    IR0_API RTypeArgumentsPtr MergeTypeArguments(RTypeArguments& typeArgs0, RTypeArguments& typeArgs1);

    // utilities
    IR0_API RTypePtr MakeBoolType();
    IR0_API RTypePtr MakeIntType();
    IR0_API RTypePtr MakeStringType();
    IR0_API RTypePtr MakeListType(const RTypePtr& itemType);

    IR0_API bool IsListType(const RTypePtr& type, RTypePtr* outItemType);

    RNamespaceDeclGroupPtr GetNamespaceDeclGroup(const std::vector<std::string>& name);

private:
    template<typename TDecl, typename TType, typename... TArgs>
    std::shared_ptr<TType> MakeInstanceType(InstanceTypeKeyUnorderedMap<TDecl, TType>& instanceTypes, const std::shared_ptr<TDecl>& decl, const RTypeArgumentsPtr& typeArgs, TArgs&&... args);

};

} // namespace Citron
