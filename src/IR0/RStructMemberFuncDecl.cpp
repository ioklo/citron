#include "RStructMemberFuncDecl.h"

namespace Citron
{

RStructMemberFuncDecl::RStructMemberFuncDecl(std::weak_ptr<RStructDecl> _struct, RAccessor accessor, std::string name, std::vector<std::string> typeParams, bool bStatic)
    : _struct(std::move(_struct))
    , accessor(accessor)
    , name(std::move(name))
    , typeParams(std::move(typeParams))
    , bStatic(bStatic)
{
}

void RStructMemberFuncDecl::InitFuncReturnAndParams(RTypePtr funcReturn, std::vector<RFuncParameter> funcParameters, bool bLastParameterVariadic)
{
    RCommonFuncDeclComponent::InitFuncReturnAndParams(RConfirmedFuncReturn(std::move(funcReturn)), std::move(funcParameters), bLastParameterVariadic);
}

}