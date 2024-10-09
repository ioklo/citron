#include "RTypeFactory.h"
#include <Infra/Hash.h>
#include <Infra/Ptr.h>
#include "RType.h"

using namespace std;

namespace Citron {

RTypeFactory::RTypeFactory()
    : voidType(new RType_Void())
{
}

shared_ptr<RType_NullableValue> RTypeFactory::MakeNullableValueType(RTypePtr innerType)
{   
    auto i = nullableValueTypes.find(innerType);

    if (i != nullableValueTypes.end())
        return i->second;

    auto key = innerType;
    shared_ptr<RType_NullableValue> newType { new RType_NullableValue(std::move(innerType)) };
    nullableValueTypes.emplace(key, newType);

    return newType;
}

shared_ptr<RType_NullableRef> RTypeFactory::MakeNullableRefType(RTypePtr innerType)
{
    auto i = nullableRefTypes.find(innerType);

    if (i != nullableRefTypes.end())
        return i->second;

    auto key = innerType;
    shared_ptr<RType_NullableRef> newType { new RType_NullableRef(std::move(innerType)) };
    nullableRefTypes.emplace(key, newType);

    return newType;
}

shared_ptr<RType_TypeVar> RTypeFactory::MakeTypeVarType(int index)
{
    auto i = typeVarTypes.find(index);
    if (i != typeVarTypes.end())
        return i->second;

    shared_ptr<RType_TypeVar> newTypeVarType { new RType_TypeVar(index) };
    typeVarTypes.emplace(index, newTypeVarType);
    return newTypeVarType;
}

shared_ptr<RType_Void> RTypeFactory::MakeVoidType()
{
    return voidType;
}

// (a: int, b: string)과 (c: int, d: string)은 같은 타입처럼 써야 하는데, 멤버 이름이 달라서
// 같은 타입이라고 하지 않고, 호환되는 타입이라고 하자
// 대입같은걸 할때 호환타입도 같이 검색해야 한다
shared_ptr<RType_Tuple> RTypeFactory::MakeTupleType(vector<RTupleMemberVar>&& memberVars)
{
    auto i = tupleTypes.find(memberVars);
    if (i != tupleTypes.end())
        return i->second;

    auto key = memberVars;
    shared_ptr<RType_Tuple> tupleType { new RType_Tuple(std::move(memberVars)) };
    tupleTypes.emplace(std::move(key), tupleType);

    return tupleType;
}

shared_ptr<RType_Func> RTypeFactory::MakeFuncType(bool bLocal, RTypePtr&& retType, vector<RType_Func::Parameter>&& params)
{
    auto key = IR0::FuncTypeKey { bLocal, retType, params };
    auto i = funcTypes.find(key);
    if (i != funcTypes.end())
        return i->second;

    shared_ptr<RType_Func> newFuncType { new RType_Func(bLocal, std::move(retType), std::move(params)) };
    funcTypes.emplace(key, newFuncType);
    return newFuncType;
}

shared_ptr<RType_LocalPtr> RTypeFactory::MakeLocalPtrType(RTypePtr&& innerType)
{
    auto i = localPtrTypes.find(innerType);
    if (i != localPtrTypes.end())
        return i->second;

    shared_ptr<RType_LocalPtr> newType { new RType_LocalPtr(std::move(innerType)) };
    localPtrTypes.emplace(innerType, newType);
    return newType;
}

shared_ptr<RType_BoxPtr> RTypeFactory::MakeBoxPtrType(RTypePtr&& innerType)
{
    auto i = boxPtrTypes.find(innerType);
    if (i != boxPtrTypes.end())
        return i->second;

    shared_ptr<RType_BoxPtr> newType { new RType_BoxPtr(innerType) };
    boxPtrTypes.emplace(innerType, newType);
    return newType;
}

template<typename TDecl, typename TType>
shared_ptr<TType> RTypeFactory::MakeInstanceType(InstanceTypeKeyUnorderedMap<TDecl, TType>& instanceTypes, const shared_ptr<TDecl>& decl, const RTypeArgumentsPtr& typeArgs)
{
    auto key = IR0::InstanceTypeKey<TDecl> { decl, typeArgs };
    auto i = instanceTypes.find(key);
    if (i != instanceTypes.end())
        return i->second;

    shared_ptr<TType> newType { new TType(decl, typeArgs) };
    instanceTypes.emplace(key, newType);
    return newType;
}

shared_ptr<RType_Class> RTypeFactory::MakeClassType(const shared_ptr<RClassDecl>& decl, const RTypeArgumentsPtr& typeArgs)
{
    return MakeInstanceType(classTypes, decl, typeArgs);
}

shared_ptr<RType_Struct> RTypeFactory::MakeStructType(const shared_ptr<RStructDecl>& decl, const RTypeArgumentsPtr& typeArgs)
{
    return MakeInstanceType(structTypes, decl, typeArgs);
}

shared_ptr<RType_Enum> RTypeFactory::MakeEnumType(const shared_ptr<REnumDecl>& decl, const RTypeArgumentsPtr& typeArgs)
{
    return MakeInstanceType(enumTypes, decl, typeArgs);
}

shared_ptr<RType_EnumElem> RTypeFactory::MakeEnumElemType(const shared_ptr<REnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs)
{
    return MakeInstanceType(enumElemTypes, decl, typeArgs);
}

shared_ptr<RType_Interface> RTypeFactory::MakeInterfaceType(const shared_ptr<RInterfaceDecl>& decl, const RTypeArgumentsPtr& typeArgs)
{
    return MakeInstanceType(interfaceTypes, decl, typeArgs);
}

shared_ptr<RType_Lambda> RTypeFactory::MakeLambdaType(const shared_ptr<RLambdaDecl>& decl, const RTypeArgumentsPtr& typeArgs)
{
    return MakeInstanceType(lambdaTypes, decl, typeArgs);
}

shared_ptr<RTypeArguments> RTypeFactory::MakeTypeArguments(const vector<RTypePtr>& items)
{
    auto key = IR0::TypeArgumentsKey { items };

    auto i = map.find(key);
    if (i != map.end())
        return i->second;

    shared_ptr<RTypeArguments> v { new RTypeArguments(items) };
    map.emplace(key, v);
    return v;
}

RTypeArgumentsPtr RTypeFactory::MergeTypeArguments(RTypeArguments& typeArgs0, RTypeArguments& typeArgs1)
{
    auto items = typeArgs0.items;
    items.insert(items.end(), typeArgs1.items.begin(), typeArgs1.items.end());

    auto key = IR0::TypeArgumentsKey { items };
    auto i = map.find(key);
    if (i != map.end())
        return i->second;

    shared_ptr<RTypeArguments> v { new RTypeArguments(std::move(items)) };
    map.emplace(key, v);
    return v;
}



} // Citron