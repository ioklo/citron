#include "RLambdaDecl.h"

namespace Citron {

RLambdaDecl::RLambdaDecl(RFuncDeclOuterWPtr&& outer, RName&& name, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
    : outer(std::move(outer)), name(std::move(name))
{
    RCommonFuncDeclComponent::InitFuncReturnAndParams(std::move(funcReturn), std::move(funcParameters), bLastParameterVariadic);
}

void RLambdaDecl::Init(std::vector<RLambdaMemberVarDecl>&& memberVars, std::vector<RStmtPtr>&& body)
{
    memberVars = std::move(memberVars);
    RCommonFuncDeclComponent::InitBody(std::move(body));
}

RDecl* RLambdaDecl::GetOuter()
{
    return outer.lock()->GetDecl();
}

RIdentifier RLambdaDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

RDecl* RLambdaDecl::GetDecl()
{
    return this;
}

} // namespace Citron