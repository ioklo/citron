#include "RStructConstructorDecl.h"

#include "RStructDecl.h"

using namespace std;

namespace Citron
{

RStructConstructorDecl::RStructConstructorDecl(weak_ptr<RStructDecl> _struct, RAccessor accessor, bool bTrivial)
    : _struct(std::move(_struct))
    , accessor(accessor)
    , bTrivial(bTrivial)
{
}

void RStructConstructorDecl::InitFuncParameters(std::vector<RFuncParameter> parameters, bool bLastParameterVariadic)
{
    RCommonFuncDeclComponent::InitFuncReturnAndParams(RNoneFuncReturn(), std::move(parameters), bLastParameterVariadic);
}

RStructConstructorDecl::~RStructConstructorDecl() = default;

RDecl* RStructConstructorDecl::GetOuter()
{
    return _struct.lock().get();
}

RIdentifier RStructConstructorDecl::GetIdentifier()
{
    return RIdentifier { RName_Reserved("Constructor"), 0, RCommonFuncDeclComponent::GetParamIds() };
}

RDecl* RStructConstructorDecl::GetDecl()
{
    return this;
}

}

