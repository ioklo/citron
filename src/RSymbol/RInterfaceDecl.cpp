#include "RInterfaceDecl.h"
#include "Infra/Exceptions.h"
#include "RFactory.h"

using namespace std;

namespace Citron {

RInterfaceDecl::RInterfaceDecl(RTypeDeclOuter outer, RName&& name, TakeRef<RFactoryPtr> rFactory)
    : outer{outer}
    , name{move(name)}
    , rFactory{rFactory.Take()}
{

}

RDecl* RInterfaceDecl::GetOuter()
{
    return outer.GetDecl();
}

RIdentifier RInterfaceDecl::GetIdentifier()
{
    return RIdentifier{name, genericsComp.GetTypeParamCount(), {}};
}

size_t RInterfaceDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParamDecl* RInterfaceDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeDecl* RInterfaceDecl::GetTypeMember(InRef<RName> name, size_t typeParamCount)
{
    return genericsComp.GetTypeMember(name, typeParamCount);
}

optional<RDeclRes> RInterfaceDecl::GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    throw NotImplementedException();
}

optional<RDeclRes> RInterfaceDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = genericsComp.ResolveIdentifierCore(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    throw NotImplementedException();
}

RDecl* RInterfaceDecl::RTypeDecl_GetDecl()
{
    return this;
}

RType* RInterfaceDecl::GetOpenType()
{   
    // bLocal 처리를 못해서 (왠지 빼야 할 것 같다) 일단 NotImplementedException 처리.
    // return rFactory->MakeInterfaceType(this, MakeOpenTypeArgs(*rFactory));
    throw NotImplementedException{};
}

RDeclRes RInterfaceDecl::ToRDeclRes(RTypeArguments* typeArgs)
{
    throw NotImplementedException{};
}

void RInterfaceDecl::Accept(RTypeDeclVisitor& visitor)
{
    visitor.Visit(this);
}

} // namespace Citron