#include "RStructConstructorDecl.h"

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

}

