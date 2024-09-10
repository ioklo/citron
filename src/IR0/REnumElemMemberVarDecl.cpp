#include "REnumElemMemberVarDecl.h"

namespace Citron {

REnumElemMemberVarDecl::REnumElemMemberVarDecl(std::weak_ptr<REnumElemDecl> outer, RName name)
    : outer(std::move(outer))
    , name(std::move(name))
{
}

void Citron::REnumElemMemberVarDecl::InitDeclType(RTypePtr declType)
{
    this->declType = std::move(declType);
}

}