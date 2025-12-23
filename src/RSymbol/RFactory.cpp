#include "RFactory.h"

#include <cassert>
#include <algorithm>

#include "Infra/Hash.h"
#include "Infra/Ptr.h"

#include "RTypes.h"
#include "RStructDecl.h"
#include "RNamespaceDeclGroup.h"
#include "RTypeArguments.h"
#include "RClassDecl.h"

using namespace std;

namespace Citron {

RFactory::RFactory()
    : voidType{new RType_Void()}
    , boolType{new RType_Primitive(RType_PrimitiveKind::Bool)}
    , intType{new RType_Primitive(RType_PrimitiveKind::Int32)}
{
    // TODO: 아직 MakeStructType, MakeClassType과는 연결이 되지 않은 상태
    stringType = unique_ptr<RType_Struct>(new RType_Struct(nullptr, MakeTypeArguments({})));
}

RFactory::~RFactory()
{

}

RType_NullableValue* RFactory::MakeNullableValueType(RType* innerType)
{   
    auto i = nullableValueTypes.find(innerType);

    if (i != nullableValueTypes.end())
        return i->second.get();

    auto* key = innerType;
    unique_ptr<RType_NullableValue> newType{new RType_NullableValue(innerType)};
    auto* pNewType = newType.get();
    nullableValueTypes.emplace(innerType, move(newType));

    return pNewType;
}

RType_NullableRef* RFactory::MakeNullableRefType(RType* innerType)
{
    auto i = nullableRefTypes.find(innerType);

    if (i != nullableRefTypes.end())
        return i->second.get();

    auto* key = innerType;
    unique_ptr<RType_NullableRef> newType{new RType_NullableRef{innerType}};    
    auto pNewType = newType.get();
    nullableRefTypes.emplace(key, move(newType));

    return pNewType;
}

RType_TypeVar* RFactory::MakeTypeVarType(int index)
{
    auto i = typeVarTypes.find(index);
    if (i != typeVarTypes.end())
        return i->second.get();

    unique_ptr<RType_TypeVar> newTypeVarType{new RType_TypeVar(index)};
    auto pNewTypeVarType = newTypeVarType.get();
    typeVarTypes.emplace(index, move(newTypeVarType));
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
    unique_ptr<RType_Tuple> tupleType{new RType_Tuple{move(vars)}};
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
    
    unique_ptr<RType_Func> newFuncType{new RType_Func{bLocal, retType, move(params)}};
    auto pNewFuncType = newFuncType.get();
    funcTypes.emplace(key, move(newFuncType));
    return pNewFuncType;
}

RType_Ptr* RFactory::MakePtrType(RType* innerType)
{
    auto i = ptrTypes.find(innerType);
    if (i != ptrTypes.end())
        return i->second.get();

    unique_ptr<RType_Ptr> newType{new RType_Ptr{innerType}};
    auto pNewType = newType.get();
    ptrTypes.emplace(innerType, move(newType));
    return pNewType;
}

RType_Shared* RFactory::MakeSharedType(RType* innerType)
{
    auto i = sharedTypes.find(innerType);
    if (i != sharedTypes.end())
        return i->second.get();

    unique_ptr<RType_Shared> newType{new RType_Shared{innerType}};
    auto pNewType = newType.get();
    sharedTypes.emplace(innerType, move(newType));
    return pNewType;
}

RType_Box* RFactory::MakeBoxType(RType* innerType)
{
    auto i = boxTypes.find(innerType);
    if (i != boxTypes.end())
        return i->second.get();

    unique_ptr<RType_Box> newType{new RType_Box{innerType}};
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
    
    unique_ptr<TType> newType{new TType{decl, typeArgs, std::forward<TArgs>(args)...}};
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

RTypeArguments* RFactory::MakeTypeArguments(const vector<RType*>& items)
{
    RTypeArgumentsKey key{items};

    auto i = typeArgsMap.find(key);
    if (i != typeArgsMap.end())
        return i->second.get();

    unique_ptr<RTypeArguments> v{new RTypeArguments{items}};
    auto pv = v.get();
    typeArgsMap.emplace(key, move(v));
    return pv;
}

RTypeArguments* RFactory::MergeTypeArguments(RTypeArguments& typeArgs0, RTypeArguments& typeArgs1)
{
    auto items = typeArgs0.items;
    items.insert(items.end(), typeArgs1.items.begin(), typeArgs1.items.end());

    RTypeArgumentsKey key{items};
    auto i = typeArgsMap.find(key);
    if (i != typeArgsMap.end())
        return i->second.get();

    unique_ptr<RTypeArguments> v{new RTypeArguments{move(items)}};
    auto pv = v.get();
    typeArgsMap.emplace(key, move(v));
    return pv;
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
    return stringType.get();
}

RType* RFactory::MakeListType(RType* itemType)
{
    auto typeArgs = MakeTypeArguments({ itemType });
    return MakeClassType(listDecl.get(), typeArgs);
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

RNamespaceDeclGroup* RFactory::GetNamespaceDeclGroup(const std::vector<std::string>& name)
{
    auto i = nsGroupsMap.find(name);
    if (i != nsGroupsMap.end())
        return i->second.get();
    
    auto newGroup = make_unique<RNamespaceDeclGroup>();
    auto pNewGroup = newGroup.get();
    nsGroupsMap.emplace(name, move(newGroup));
    return pNewGroup;
}

} // Citron