#include "SmDeclContext.h"
#include "RSymbol/RDecl.h"
#include "RSymbol/RMember.h"
#include "SmTypeRes.h"
#include "SmDeclRes.h"


using namespace std;

namespace Citron {

optional<SmTypeRes> SmDeclContext::ResolveTypeIdentifier(InRef<RName> name)
{
    auto* decl = GetDecl();    

    if (auto* typeParam = decl->GetTypeParam(name))
        return SmTypeRes_TypeVar{typeParam};

    if (auto* typeDecl = decl->GetTypeMember(name)) // TODO: namespace 처리
    {
        auto* typeArgs = GetTypeArgs();
        return ToSmTypeRes(typeArgs, typeDecl);
    }

    if (auto o_member = ResolveInheritedTypeMember(name))
        return o_member;

    if (auto* outer = GetOuter())
        return outer->ResolveTypeIdentifier(name);

    return nullopt;
}

optional<SmTypeRes> SmDeclContext::ResolveTypeIdentifierInHeader(InRef<RName> name)
{
    auto* decl = GetDecl();

    if (auto* typeParam = decl->GetTypeParam(name))
        return SmTypeRes_TypeVar(typeParam);

    if (auto* outer = GetOuter())
        return outer->ResolveTypeIdentifier(name); // 자식을 지나치는건 처음에만, 그 후로는 ResolveTypeIdentifier방식을 따른다

    return nullopt;
}

optional<SmDeclRes> SmDeclContext::ResolveIdentifier(InRef<RName> name)
{
    auto* decl = GetDecl();

    if (auto* typeParam = decl->GetTypeParam(name))
        return SmDeclRes_TypeVar{typeParam};    

    if (auto o_member = decl->GetMember(name))
    {
        auto* typeArgs = GetTypeArgs();
        return ToSmDeclRes(typeArgs, *o_member);
    }

    if (auto o_member = ResolveInheritedMember(name))
        return o_member;

    if (auto* outer = GetOuter())
        return outer->ResolveIdentifier(name);

    return nullopt;
}

optional<SmTypeRes> SmDeclContext::ResolveInheritedTypeMember(InRef<RName> name)
{
    return nullopt;
}

optional<SmDeclRes> SmDeclContext::ResolveInheritedMember(InRef<RName> name)
{
    return nullopt;
}


} // namespace Citron