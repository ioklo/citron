#include "REnumElemDecl.h"
#include "Infra/Exceptions.h"
#include "REnumDecl.h"
#include "REnumElemVarDecl.h"
#include "RFactory.h"
#include "RTypeRes.h"
#include "RMember.h"

using namespace std;

namespace Citron {

REnumElemDecl::REnumElemDecl(RDeclKey&& key,REnumDecl* _enum, RName&& name, TakeRef<RFactoryPtr> rFactory)
    : key{move(key)}, _enum{_enum}, name{move(name)}, rFactory{rFactory.Take()}
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

RDeclKey& REnumElemDecl::GetDeclKey()
{
    return key;
}

RDecl* REnumElemDecl::GetOuter()
{
    return _enum;
}

RName* REnumElemDecl::TryGetName()
{
    return &name;
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

void REnumElemDecl::Accept(RTypeDeclVisitor& visitor)
{
    visitor.Visit(this);
}

} // namespace Citron