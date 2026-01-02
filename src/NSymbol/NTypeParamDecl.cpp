#include "NTypeParamDecl.h"
#include <cassert>
#include "RSymbol/RTypeArguments.h"

namespace Citron {

NTypeParamDecl::NTypeParamDecl(NDecl* outer, RName&& name, size_t globalIndex)
    : outer{outer}, name{move(name)}, globalIndex{globalIndex}
{
}

NTypeParamDecl::~NTypeParamDecl() = default;

RMember NTypeParamDecl::ToRMember(RTypeArguments* typeArgs)
{
    assert(typeArgs->GetCount() == 0);
    return RMember_TypeVar(this);
}


} // namespace Citron