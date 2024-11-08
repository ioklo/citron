#include "NStructMemberVarDecl.h"
#include <cassert>
#include <Infra/Exceptions.h>
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

NDecl* NStructMemberVarDecl::GetNOuter()
{
    return _struct.lock().get();
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

optional<Citron::RMember> NStructMemberVarDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    // MemberVarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

RTypePtr NStructMemberVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    assert(declType != nullptr);
    return declType->Apply(typeArgs, factory);
}


} // namespace Citron