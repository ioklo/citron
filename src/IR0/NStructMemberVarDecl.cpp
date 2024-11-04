#include "NStructMemberVarDecl.h"
#include <cassert>
#include "NStructDecl.h"
#include "RTypeFactory.h"

using namespace std;

namespace Citron {

NStructMemberVarDecl::NStructMemberVarDecl(std::weak_ptr<NStructDecl> _struct, RAccessor accessor, bool bStatic, std::string name)
    : _struct(std::move(_struct))
    , accessor(accessor)
    , bStatic(bStatic)
    , name(std::move(name))
{
}

void NStructMemberVarDecl::InitDeclType(const RTypePtr& declType)
{
    this->declType = declType;
}

RTypePtr NStructMemberVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    assert(declType != nullptr);
    return declType->Apply(typeArgs, factory);
}

RDecl* NStructMemberVarDecl::GetROuter()
{
    return _struct.lock().get();
}

RIdentifier NStructMemberVarDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
}

optional<RMember> NStructMemberVarDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

} // namespace Citron