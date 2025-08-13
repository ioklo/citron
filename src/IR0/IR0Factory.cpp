#include "IR0Factory.h"

#include <cassert>

#include "Infra/Hash.h"
#include "Infra/Ptr.h"

#include "RTypes.h"
#include "RStructDecl.h"
#include "RNamespaceDeclGroup.h"

#include "RTypeArguments.h"
#include "RClassDecl.h"

#include "NNamespaceDecl.h"

using namespace std;

namespace Citron {

IR0Factory::IR0Factory()
    : voidType{new RType_Void()}
{
}

RType_NullableValue* IR0Factory::MakeNullableValueType(RType* innerType)
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

RType_NullableRef* IR0Factory::MakeNullableRefType(RType* innerType)
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

RType_TypeVar* IR0Factory::MakeTypeVarType(int index)
{
    auto i = typeVarTypes.find(index);
    if (i != typeVarTypes.end())
        return i->second.get();

    unique_ptr<RType_TypeVar> newTypeVarType{new RType_TypeVar(index)};
    auto pNewTypeVarType = newTypeVarType.get();
    typeVarTypes.emplace(index, move(newTypeVarType));
    return pNewTypeVarType;
}

RType_Void* IR0Factory::MakeVoidType()
{
    return voidType.get();
}

// (a: int, b: string)과 (c: int, d: string)은 같은 타입처럼 써야 하는데, 멤버 이름이 달라서
// 같은 타입이라고 하지 않고, 호환되는 타입이라고 하자
// 대입같은걸 할때 호환타입도 같이 검색해야 한다
RType_Tuple* IR0Factory::MakeTupleType(vector<RTupleVar>&& vars)
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

RType_Func* IR0Factory::MakeFuncType(bool bLocal, RType* retType, vector<RType_Func::Parameter>&& params)
{
    auto key = IR0::FuncTypeKey { bLocal, retType, params };
    auto i = funcTypes.find(key);
    if (i != funcTypes.end())
        return i->second.get();
    
    unique_ptr<RType_Func> newFuncType{new RType_Func{bLocal, retType, move(params)}};
    auto pNewFuncType = newFuncType.get();
    funcTypes.emplace(key, move(newFuncType));
    return pNewFuncType;
}

RType_LocalPtr* IR0Factory::MakeLocalPtrType(RType* innerType)
{
    auto i = localPtrTypes.find(innerType);
    if (i != localPtrTypes.end())
        return i->second.get();

    unique_ptr<RType_LocalPtr> newType{new RType_LocalPtr{innerType}};
    auto pNewType = newType.get();
    localPtrTypes.emplace(innerType, move(newType));
    return pNewType;
}

RType_BoxPtr* IR0Factory::MakeBoxPtrType(RType* innerType)
{
    auto i = boxPtrTypes.find(innerType);
    if (i != boxPtrTypes.end())
        return i->second.get();

    unique_ptr<RType_BoxPtr> newType{new RType_BoxPtr{innerType}};
    auto pNewType = newType.get();
    boxPtrTypes.emplace(innerType, move(newType));
    return pNewType;
}

template<typename TDecl, typename TType, typename... TArgs>
TType* IR0Factory::MakeInstanceType(InstanceTypeKeyUnorderedMap<TDecl, TType>& instanceTypes, TDecl* decl, RTypeArguments* typeArgs, TArgs&&... args)
{
    auto key = IR0::InstanceTypeKey<TDecl> { decl, typeArgs };
    auto i = instanceTypes.find(key);
    if (i != instanceTypes.end())
        return i->second.get();
    
    unique_ptr<TType> newType{new TType{decl, typeArgs, std::forward<TArgs>(args)...}};
    auto pNewType = newType.get();
    instanceTypes.emplace(key, move(newType));
    return pNewType;
}

RType_Class* IR0Factory::MakeClassType(RClassDecl* decl, RTypeArguments* typeArgs)
{
    return MakeInstanceType(classTypes, decl, typeArgs);
}

RType_Struct* IR0Factory::MakeStructType(RStructDecl* decl, RTypeArguments* typeArgs)
{
    return MakeInstanceType(structTypes, decl, typeArgs);
}

RType_Enum* IR0Factory::MakeEnumType(REnumDecl* decl, RTypeArguments* typeArgs)
{
    return MakeInstanceType(enumTypes, decl, typeArgs);
}

RType_EnumElem* IR0Factory::MakeEnumElemType(REnumElemDecl* decl, RTypeArguments* typeArgs)
{
    return MakeInstanceType(enumElemTypes, decl, typeArgs);
}

RType_Interface* IR0Factory::MakeInterfaceType(RInterfaceDecl* decl, RTypeArguments* typeArgs, bool bLocal)
{
    return MakeInstanceType(interfaceTypes, decl, typeArgs, bLocal);
}

RType_Lambda* IR0Factory::MakeLambdaType(RLambdaDecl* decl, RTypeArguments* typeArgs)
{
    return MakeInstanceType(lambdaTypes, decl, typeArgs);
}

RTypeArguments* IR0Factory::MakeTypeArguments(const vector<RType*>& items)
{
    auto key = IR0::TypeArgumentsKey{ items };

    auto i = typeArgsMap.find(key);
    if (i != typeArgsMap.end())
        return i->second.get();

    unique_ptr<RTypeArguments> v{new RTypeArguments{items}};
    auto pv = v.get();
    typeArgsMap.emplace(key, move(v));
    return pv;
}

RTypeArguments* IR0Factory::MergeTypeArguments(RTypeArguments& typeArgs0, RTypeArguments& typeArgs1)
{
    auto items = typeArgs0.items;
    items.insert(items.end(), typeArgs1.items.begin(), typeArgs1.items.end());

    auto key = IR0::TypeArgumentsKey { items };
    auto i = typeArgsMap.find(key);
    if (i != typeArgsMap.end())
        return i->second.get();

    unique_ptr<RTypeArguments> v{new RTypeArguments{move(items)}};
    auto pv = v.get();
    typeArgsMap.emplace(key, move(v));
    return pv;
}

RType* IR0Factory::MakeBoolType()
{   
    return boolType.get();
}

RType* IR0Factory::MakeIntType()
{
    return intType.get();
}

RType* IR0Factory::MakeStringType()
{
    return stringType.get();
}

RType* IR0Factory::MakeListType(RType* itemType)
{
    auto typeArgs = MakeTypeArguments({ itemType });
    return MakeClassType(listDecl.get(), typeArgs);
}

bool IR0Factory::IsListType(RType* type, RType** outItemType)
{
    auto* classType = dynamic_cast<RType_Class*>(type);
    if (!classType) return false;

    if (classType->decl != listDecl.get()) return false;
    if (classType->typeArgs->GetCount() != 1) return false;

    *outItemType = classType->typeArgs->Get(0);
    return true;
}

NNamespaceDecl* IR0Factory::NewRootNamespaceDecl()
{
    // root namespace면 
    auto group = GetNamespaceDeclGroup({});
    unique_ptr<NNamespaceDecl> newDecl{new NNamespaceDecl{nullptr, "", group}};
    auto pNewDecl = newDecl.get();
    namespaceDecls.push_back(move(newDecl));

    group->Add(pNewDecl);
    return pNewDecl;
}

NNamespaceDecl* IR0Factory::NewChildNamespaceDecl(NNamespaceDecl* outer, const string& name)
{
    assert(outer && !name.empty());

    // root namespace면 
    std::vector<std::string> id;

    id.push_back(name);
    auto curNS = outer;

    while (curNS)
    {
        auto curOuter = curNS->outer;

        if (!curOuter)
        {
            // root 라면 그만둔다
            assert(curNS->name.empty());
            break;
        }

        id.push_back(curNS->name);
        curNS = curOuter;
    }

    reverse(id.begin(), id.end());

    auto group = GetNamespaceDeclGroup(id);
    unique_ptr<NNamespaceDecl> newDecl{new NNamespaceDecl{outer, name, group}};
    auto pNewDecl = newDecl.get();
    namespaceDecls.push_back(move(newDecl));

    group->Add(pNewDecl);
    return pNewDecl;
}

RNamespaceDeclGroup* IR0Factory::GetNamespaceDeclGroup(const std::vector<std::string>& name)
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