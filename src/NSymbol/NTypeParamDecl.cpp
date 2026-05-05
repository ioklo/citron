#include "NTypeParamDecl.h"
#include <cassert>
#include "RSymbol/RTypeArguments.h"
#include "RSymbol/RFactory.h"

namespace Citron {

NTypeParamDecl::NTypeParamDecl(NDecl* outer, RName&& name, size_t globalIndex, const RFactoryPtr& rFactory)
    : outer{outer}, name{move(name)}, globalIndex{globalIndex}, rFactory{rFactory}
{
}

NTypeParamDecl::~NTypeParamDecl() = default;

RDeclRes NTypeParamDecl::ToRDeclRes(RTypeArguments* typeArgs)
{
    assert(typeArgs->GetCount() == 0);
    return RDeclRes_TypeVar(this);
}

RType* NTypeParamDecl::GetOpenType()
{
    return rFactory->MakeTypeVarType(this);
}


} // namespace Citron