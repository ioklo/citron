#include "RLambdaDecl.h"

namespace Citron {

RLambdaDecl::RLambdaDecl(RFuncDeclOuterPtr&& outer, RName name)
    : outer(outer), name(name)
{
}

void RLambdaDecl::Init(std::vector<RLambdaMemberVarDecl>&& memberVars, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic, std::vector<RStmtPtr>&& body)
{
    memberVars = std::move(memberVars);
    RCommonFuncDeclComponent::InitFuncReturnAndParams(std::move(funcReturn), std::move(funcParameters), bLastParameterVariadic);
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