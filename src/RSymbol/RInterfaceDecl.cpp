#include "RInterfaceDecl.h"
#include "Infra/Exceptions.h"
#include "RFactory.h"
#include "RTypeRes.h"
#include "RDeclRes.h"
#include "RMember.h"

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
    return RIdentifier{name, {}};
}

size_t RInterfaceDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParam* RInterfaceDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeParam* RInterfaceDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComp.GetTypeParam(name);
}

RTypeDecl* RInterfaceDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RInterfaceDecl::GetMember(InRef<RName> name)
{
    // TODO: [71] 2026-07-18, interface 구현
    throw NotImplementedException{};
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

void RInterfaceDecl::Accept(RTypeDeclVisitor& visitor)
{
    visitor.Visit(this);
}

} // namespace Citron