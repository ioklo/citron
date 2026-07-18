#include "RDecl.h"

#include "Infra/Exceptions.h"
#include "Infra/Unreachable.h"

#include "RFactory.h"
#include "RTypeRes.h"
#include "RTypeArguments.h"
#include "RMember.h"

using namespace std;

namespace Citron {

bool RDecl::IsDescendantOf(RDecl* container)
{
    auto* outer = GetOuter();
    if (outer == nullptr) return false;

    if (outer == container) return true;
    return outer->IsDescendantOf(container);
}

void MakeOpenTypeArgsCore(RDecl* decl, vector<RType*>& typeArgItems, size_t totalSize, RFactory& rFactory)
{
    size_t typeParamCount = decl->GetTypeParamCount();

    auto* outer = decl->GetOuter();
    if (!outer)
        typeArgItems.reserve(totalSize); // base case
    else
        MakeOpenTypeArgsCore(outer, typeArgItems, totalSize + typeParamCount, rFactory);

    for (size_t i = 0; i < typeParamCount; i++)
    {
        auto* typeParam = decl->GetTypeParam(i);
        auto* typeVar = rFactory.MakeTypeVarType(typeParam);
        typeArgItems.push_back(typeVar);
    }
}

RTypeArguments* RDecl::MakeOpenTypeArgs(RFactory& factory)
{
    vector<RType*> typeArgItems;
    MakeOpenTypeArgsCore(this, typeArgItems, 0, factory);
    return factory.MakeTypeArguments(typeArgItems);
}

size_t RDecl::GetAllTypeParamCount()
{
    auto* outer = GetOuter();
    if (!outer) return GetTypeParamCount();

    return outer->GetAllTypeParamCount() + GetTypeParamCount();
}

RTypeArguments* GetOuterTypeArgs(RDecl* decl, RTypeArguments* typeArgs)
{
    return typeArgs->Remove(decl->GetTypeParamCount());
}

optional<RTypeRes> RDecl::ResolveTypeIdentifier(RTypeArguments* typeArgs, InRef<RName> name)
{
    if (auto* typeParam = GetTypeParam(name))
        return RTypeRes_TypeVar(typeParam);

    if (auto* typeDecl = GetTypeMember(name))
        return ToRTypeRes(typeArgs, typeDecl);

    if (auto o_member = ResolveInheritedTypeMember(typeArgs, name))
        return o_member;

    if (auto* outer = GetOuter())
    {
        auto* outerTypeArgs = GetOuterTypeArgs(this, typeArgs);
        return outer->ResolveTypeIdentifier(outerTypeArgs, name);
    }

    return nullopt;
}

optional<RTypeRes> RDecl::ResolveTypeIdentifierInHeader(RTypeArguments* typeArgs, InRef<RName> name)
{
    if (auto* typeParam = GetTypeParam(name))
        return RTypeRes_TypeVar(typeParam);

    if (auto* outer = GetOuter())
    {
        auto* outerTypeArgs = GetOuterTypeArgs(this, typeArgs);
        return outer->ResolveTypeIdentifier(outerTypeArgs, name); // 자식을 지나치는건 처음에만, 그 후로는 ResolveTypeIdentifier방식을 따른다
    }

    return nullopt;
}

optional<RDeclRes> RDecl::ResolveIdentifier(RTypeArguments* typeArgs, InRef<RName> name)
{
    if (auto* typeParam = GetTypeParam(name))
        return RDeclRes_TypeVar(typeParam);

    if (auto o_member = GetMember(name))
        return ToRDeclRes(typeArgs, *o_member);

    if (auto o_member = ResolveInheritedMember(typeArgs, name))
        return o_member;

    if (auto* outer = GetOuter())
    {
        auto* outerTypeArgs = GetOuterTypeArgs(this, typeArgs);
        return outer->ResolveIdentifier(outerTypeArgs, name);
    }

    return nullopt;
}

bool RDecl::CanAccess(RDecl* target)
{
    // TODO: [68] 2026-07-11, CanAccess 제대로 구현
    return true;

    //auto accessModifier = target->GetAccessor();
    //auto* targetOuter = target->GetROuter();
    //if (targetOuter == nullptr)
    //    return false;

    //switch (accessModifier)
    //{
    //case RAccessor::Public: return true;
    //case RAccessor::Protected: throw NotImplementedException();
    //case RAccessor::Private:
    //{
    //    // 같은 경우는 허용
    //    if (this == targetOuter)
    //        return true;

    //    // base클래스가 아니라 container에 속하는지를 본다
    //    return IsDescendantOf(targetOuter);
    //}

    //default: unreachable();
    //}
}

optional<RTypeRes> RDecl::ResolveInheritedTypeMember(RTypeArguments* typeArgs, InRef<RName> name)
{
    return nullopt;
}

optional<RDeclRes> RDecl::ResolveInheritedMember(RTypeArguments* typeArgs, InRef<RName> name)
{
    return nullopt;
}

}