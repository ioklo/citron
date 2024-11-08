#include "NEnumElemMemberVarDecl.h"

#include <Infra/Exceptions.h>

#include "NEnumDecl.h"

using namespace std;

namespace Citron {

NEnumElemMemberVarDecl::NEnumElemMemberVarDecl(std::weak_ptr<NEnumElemDecl> enumElem, const std::string& name)
    : enumElem(std::move(enumElem))
    , name(name)
{
}

void Citron::NEnumElemMemberVarDecl::InitDeclType(RTypePtr&& declType)
{
    this->declType = std::move(declType);
}

NDecl* NEnumElemMemberVarDecl::GetNOuter()
{
    return enumElem.lock().get();
}

RDecl* NEnumElemMemberVarDecl::GetROuter()
{
    return enumElem.lock().get();
}

RIdentifier NEnumElemMemberVarDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
}

optional<RMember> NEnumElemMemberVarDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RMember> NEnumElemMemberVarDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    // MemberVarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

RTypePtr NEnumElemMemberVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return declType->Apply(typeArgs, factory);
}

}