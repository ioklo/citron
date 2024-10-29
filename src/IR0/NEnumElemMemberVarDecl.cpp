#include "NEnumElemMemberVarDecl.h"
#include "NEnumDecl.h"

namespace Citron {

NEnumElemMemberVarDecl::NEnumElemMemberVarDecl(std::weak_ptr<NEnumElemDecl> outer, RName name)
    : outer(std::move(outer))
    , name(std::move(name))
{
}

void Citron::NEnumElemMemberVarDecl::InitDeclType(RTypePtr&& declType)
{
    this->declType = std::move(declType);
}

RTypePtr NEnumElemMemberVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return declType->Apply(typeArgs, factory);
}

NDecl* NEnumElemMemberVarDecl::GetOuter()
{
    return outer.lock().get();
}

RIdentifier NEnumElemMemberVarDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

}