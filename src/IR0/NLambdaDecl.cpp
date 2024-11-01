#include "NLambdaDecl.h"

#include <Infra/Exceptions.h>

using namespace std;

namespace Citron {

NLambdaDecl::NLambdaDecl(NFuncDeclOuterWPtr&& outer, RName&& name, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
    : NCommonFuncDeclComponent(/*typeParams*/ {}), outer(std::move(outer)), name(std::move(name))
{
    NCommonFuncDeclComponent::InitFuncReturnAndParams(std::move(funcReturn), std::move(funcParameters), bLastParameterVariadic);
}

void NLambdaDecl::Init(std::vector<std::shared_ptr<NLambdaMemberVarDecl>>&& memberVars, std::vector<NStmtPtr>&& body)
{
    for (auto& memberVar : memberVars)
        memberVarsMap.emplace(memberVar->name, memberVar);

    this->memberVars = std::move(memberVars);

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

RMember NLambdaDecl::ToRMember(const std::shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs)
{
    throw RuntimeFatalException(); // 들어올수가 
}

optional<RMember> NLambdaDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    auto i = memberVarsMap.find(name);
    if (i == memberVarsMap.end()) return nullopt;

    return RMember_LambdaMemberVar(typeArgs, i->second);
}

} // namespace Citron