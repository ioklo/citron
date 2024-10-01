#include "RTypeFactory.h"
#include <Infra/Hash.h>
#include <Infra/Ptr.h>
#include "RType.h"

using namespace std;

namespace Citron {

RTypeFactory::RTypeFactory()
    : voidType(new RVoidType())
{
}

shared_ptr<RNullableType> RTypeFactory::MakeNullableType(RTypePtr&& innerType)
{
    auto key = innerType;
    auto i = nullableTypes.find(key);

    if (i != nullableTypes.end())
        return i->second;

    shared_ptr<RNullableType> newNullableType { new RNullableType(std::move(innerType)) };
    nullableTypes.emplace(key, newNullableType);

    return newNullableType;
}

shared_ptr<RTypeVarType> RTypeFactory::MakeTypeVarType(int index)
{
    auto i = typeVarTypes.find(index);
    if (i != typeVarTypes.end())
        return i->second;

    shared_ptr<RTypeVarType> newTypeVarType { new RTypeVarType(index) };
    typeVarTypes.emplace(index, newTypeVarType);
    return newTypeVarType;
}

shared_ptr<RVoidType> RTypeFactory::MakeVoidType()
{
    return voidType;
}

// (a: int, b: string)과 (c: int, d: string)은 같은 타입처럼 써야 하는데, 멤버 이름이 달라서
// 같은 타입이라고 하지 않고, 호환되는 타입이라고 하자
// 대입같은걸 할때 호환타입도 같이 검색해야 한다
shared_ptr<RTupleType> RTypeFactory::MakeTupleType(std::vector<RTupleMemberVar>&& memberVars)
{
    auto i = tupleTypes.find(memberVars);
    if (i != tupleTypes.end())
        return i->second;

    auto key = memberVars;
    shared_ptr<RTupleType> tupleType { new RTupleType(std::move(memberVars)) };
    tupleTypes.emplace(std::move(key), tupleType);

    return tupleType;
}

shared_ptr<RFuncType> RTypeFactory::MakeFuncType(bool bLocal, RTypePtr&& retType, std::vector<RFuncType::Parameter>&& params)
{
    auto key = IR0::FuncTypeKey { bLocal, retType, params };
    auto i = funcTypes.find(key);
    if (i != funcTypes.end())
        return i->second;

    shared_ptr<RFuncType> newFuncType { new RFuncType(bLocal, std::move(retType), std::move(params)) };
    funcTypes.emplace(key, newFuncType);
    return newFuncType;
}

shared_ptr<RLocalPtrType> RTypeFactory::MakeLocalPtrType(RTypePtr&& innerType)
{
    auto i = localPtrTypes.find(innerType);
    if (i != localPtrTypes.end())
        return i->second;

    shared_ptr<RLocalPtrType> newType { new RLocalPtrType(std::move(innerType)) };
    localPtrTypes.emplace(innerType, newType);
    return newType;
}

shared_ptr<RBoxPtrType> RTypeFactory::MakeBoxPtrType(RTypePtr&& innerType)
{
    auto i = boxPtrTypes.find(innerType);
    if (i != boxPtrTypes.end())
        return i->second;

    shared_ptr<RBoxPtrType> newType { new RBoxPtrType(std::move(innerType)) };
    boxPtrTypes.emplace(innerType, newType);
    return newType;
}

shared_ptr<RInstanceType> RTypeFactory::MakeInstanceType(const RDeclIdPtr& declId, RTypeArgumentsPtr&& typeArgs)
{
    auto key = IR0::InstanceTypeKey { declId, typeArgs };
    auto i = instanceTypes.find(key);
    if (i != instanceTypes.end())
        return i->second;

    shared_ptr<RInstanceType> newType { new RInstanceType(declId, std::move(typeArgs)) };
    instanceTypes.emplace(key, newType);
    return newType;
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

} // Citron