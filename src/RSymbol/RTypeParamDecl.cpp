#include "RTypeParamDecl.h"
#include <cassert>
#include "RFactory.h"
#include "RTypeArguments.h"

using namespace std;

namespace Citron {

RTypeParamDecl::RTypeParamDecl(RDecl* outer, RName&& name, size_t globalIndex, TakeRef<RFactoryPtr> rFactory)
    : outer{outer}
    , name{move(name)}
    , globalIndex{globalIndex}
    , rFactory{rFactory.Take()}
{
}

RDecl* RTypeParamDecl::GetOuter()
{
    return outer;
}

RIdentifier RTypeParamDecl::GetIdentifier()
{
    return RIdentifier{name, {}};
}

size_t RTypeParamDecl::GetTypeParamCount()
{
    return 0;
}

RTypeParamDecl* RTypeParamDecl::GetTypeParam(size_t index)
{
    return nullptr;
}

RTypeDecl* RTypeParamDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RDeclRes> RTypeParamDecl::ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<RDeclRes> RTypeParamDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

RDecl* RTypeParamDecl::RTypeDecl_GetDecl()
{
    return this;
}

RType* RTypeParamDecl::GetOpenType()
{
    return rFactory->MakeTypeVarType(this);
}

RDeclRes RTypeParamDecl::ToRDeclRes(RTypeArguments* typeArgs)
{
    assert(typeArgs->GetCount() == 0);
    return RDeclRes_TypeVar(this);
}

void RTypeParamDecl::Accept(RTypeDeclVisitor& visitor)
{
    return visitor.Visit(this);
}


} // namespace Citron