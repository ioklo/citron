#include "NStructConstructorDecl.h"

#include "NStructDecl.h"

using namespace std;

namespace Citron
{

NStructConstructorDecl::NStructConstructorDecl(weak_ptr<NStructDecl> _struct, RAccessor accessor, bool bTrivial)
    : _struct(std::move(_struct))
    , accessor(accessor)
    , bTrivial(bTrivial)
{
}

void NStructConstructorDecl::InitFuncParameters(std::vector<RFuncParameter> parameters, bool bLastParameterVariadic)
{
    NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn_ForConstructor(), std::move(parameters), bLastParameterVariadic);
}

NStructConstructorDecl::~NStructConstructorDecl() = default;

NDecl* NStructConstructorDecl::GetOuter()
{
    return _struct.lock().get();
}

RIdentifier NStructConstructorDecl::GetIdentifier()
{
    return RIdentifier { RName_Reserved("Constructor"), 0, NCommonFuncDeclComponent::GetParamIds() };
}

NDecl* NStructConstructorDecl::GetDecl()
{
    return this;
}

shared_ptr<RStructDecl> NStructConstructorDecl::GetStructDecl()
{
    return _struct.lock();
}

}

