#include "SmDeclContext_ClassDecl.h"
#include "RSymbol/RClassDecl.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RTypeArguments.h"
#include "SmTypeRes.h"
#include "SmDeclRes.h"

using namespace std;

namespace Citron {

namespace {

optional<SmTypeRes> ResolveBaseClassTypeMember(RAppliedDecl<RClassDecl> baseClass, InRef<RName> name)
{
    if (auto* baseTypeMember = baseClass.decl->GetTypeMember(name))
        return ToSmTypeRes(baseClass.typeArgs, baseTypeMember);

    // base's base
    if (auto o_unboundBaseClass = baseClass.decl->GetUnboundBaseClass())
    {
        auto* baseClassTypeArgs = o_unboundBaseClass->typeArgs->Apply(baseClass.typeArgs);
        return ResolveBaseClassTypeMember({o_unboundBaseClass->decl, baseClassTypeArgs}, name);
    }

    return nullopt;
}

optional<SmDeclRes> ResolveBaseClassMember(RAppliedDecl<RClassDecl> baseClass, InRef<RName> name)
{
    if (auto o_member = baseClass.decl->GetMember(name))
        return ToSmDeclRes(baseClass.typeArgs, *o_member);

    // base's base
    if (auto o_unboundBaseClass = baseClass.decl->GetUnboundBaseClass())
    {
        auto* baseClassTypeArgs = o_unboundBaseClass->typeArgs->Apply(baseClass.typeArgs);
        return ResolveBaseClassMember({o_unboundBaseClass->decl, baseClassTypeArgs}, name);
    }

    return nullopt;
}

}

SmDeclContext* SmDeclContext_ClassDecl::GetOuter()
{
    return outer.get();
}
RDecl* SmDeclContext_ClassDecl::GetDecl()
{
    return decl;
}

optional<SmTypeRes> SmDeclContext_ClassDecl::ResolveInheritedTypeMember(InRef<RName> name)
{
    // baseClass가 있다면
    if (auto o_unboundBaseClass = decl->GetUnboundBaseClass())
    {
        auto* baseClassTypeArgs = o_unboundBaseClass->typeArgs->Apply(typeArgs);
        return ResolveBaseClassTypeMember({o_unboundBaseClass->decl, baseClassTypeArgs}, name);
    }

    return nullopt;
}

optional<SmDeclRes> SmDeclContext_ClassDecl::ResolveInheritedMember(InRef<RName> name)
{
    if (auto o_unboundBaseClass = decl->GetUnboundBaseClass())
    {
        auto* baseClassTypeArgs = o_unboundBaseClass->typeArgs->Apply(typeArgs);
        return ResolveBaseClassMember({o_unboundBaseClass->decl, baseClassTypeArgs}, name);
    }

    return nullopt;
}

} // namespace Citron