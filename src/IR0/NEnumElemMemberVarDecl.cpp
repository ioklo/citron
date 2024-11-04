#include "NEnumElemMemberVarDecl.h"
#include "NEnumDecl.h"

using namespace std;

namespace Citron {

NEnumElemMemberVarDecl::NEnumElemMemberVarDecl(std::weak_ptr<NEnumElemDecl> outer, const std::string& name)
    : outer(std::move(outer))
    , name(name)
{
}

void Citron::NEnumElemMemberVarDecl::InitDeclType(RTypePtr&& declType)
{
    this->declType = std::move(declType);
}

RDecl* NEnumElemMemberVarDecl::GetROuter()
{
    return outer.lock().get();
}

RIdentifier NEnumElemMemberVarDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
}

optional<RMember> NEnumElemMemberVarDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

RTypePtr NEnumElemMemberVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return declType->Apply(typeArgs, factory);
}

}