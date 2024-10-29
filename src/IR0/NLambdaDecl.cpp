#include "NLambdaDecl.h"

namespace Citron {

NLambdaDecl::NLambdaDecl(NFuncDeclOuterWPtr&& outer, RName&& name, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
    : outer(std::move(outer)), name(std::move(name))
{
    NCommonFuncDeclComponent::InitFuncReturnAndParams(std::move(funcReturn), std::move(funcParameters), bLastParameterVariadic);
}

void NLambdaDecl::Init(std::vector<NLambdaMemberVarDecl>&& memberVars, std::vector<NStmtPtr>&& body)
{
    memberVars = std::move(memberVars);
    NCommonFuncDeclComponent::InitBody(std::move(body));
}

NDecl* NLambdaDecl::GetOuter()
{
    return outer.lock()->GetDecl();
}

RIdentifier NLambdaDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

NDecl* NLambdaDecl::GetDecl()
{
    return this;
}

} // namespace Citron