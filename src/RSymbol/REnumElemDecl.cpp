#include "REnumElemDecl.h"
#include "Infra/Exceptions.h"
#include "REnumDecl.h"
#include "REnumElemVarDecl.h"
#include "RFactory.h"
#include "RTypeRes.h"
#include "RDeclRes.h"
#include "RMember.h"

using namespace std;

namespace Citron {

REnumElemDecl::REnumElemDecl(REnumDecl* _enum, TakeRef<RName> name, TakeRef<RFactoryPtr> rFactory)
    : _enum{_enum}, name{name.Take()}, rFactory{rFactory.Take()}
{
}

void REnumElemDecl::AddVar(REnumElemVarDecl* var)
{
    vars.push_back(var);
    varsMap.emplace(var->GetName(), var);
}

REnumElemVarDecl* REnumElemDecl::GetUnboundVar(InRef<RName> name)
{
    auto i = varsMap.find(*name);
    if (i != varsMap.end()) 
        return i->second;

    return nullptr;
}

RDecl* REnumElemDecl::GetOuter()
{
    return _enum;
}

RIdentifier REnumElemDecl::GetIdentifier()
{
    return RIdentifier{name, {}};
}

size_t REnumElemDecl::GetTypeParamCount()
{
    return 0;
}

RTypeParam* REnumElemDecl::GetTypeParam(size_t index)
{
    return nullptr;
}

RTypeParam* REnumElemDecl::GetTypeParam(InRef<RName> name)
{
    return nullptr;
}

RTypeDecl* REnumElemDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> REnumElemDecl::GetMember(InRef<RName> name)
{
    if (auto* var = GetUnboundVar(name))
        return RMember_EnumElemVar{var};

    return nullopt;
}

RDecl* REnumElemDecl::RTypeDecl_GetDecl()
{
    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

RType* REnumElemDecl::GetOpenType()
{
    return rFactory->MakeEnumElemType(this, MakeOpenTypeArgs(*rFactory));
}

void REnumElemDecl::Accept(RTypeDeclVisitor& visitor)
{
    visitor.Visit(this);
}

} // namespace Citron