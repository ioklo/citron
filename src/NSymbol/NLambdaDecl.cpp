#include "NLambdaDecl.h"

#include "Infra/Exceptions.h"

using namespace std;

namespace Citron {

NLambdaDecl::NLambdaDecl(NFuncDeclOuter* outer)
    : outer{outer}
{
}

void NLambdaDecl::Init(RName&& name, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{   
    this->name = move(name);

    NCommonFuncDeclComponent::Init(/*bStatic*/false, /*bSeqFunc*/false, /*typeParams*/{});
    NCommonFuncDeclComponent::InitFuncReturnAndParams(move(funcReturn), move(funcParameters), bLastParameterVariadic);
}

void NLambdaDecl::InitVars(std::vector<NLambdaVarDecl*>&& vars)
{
    for (auto& var : vars)
        varsMap.emplace(var->name, var);

    this->vars = move(vars);
}

NDecl* NLambdaDecl::GetNOuter()
{
    return outer->GetNDecl();
}

RDecl* NLambdaDecl::GetROuter()
{
    return outer->GetNDecl()->GetRDecl();
}

RIdentifier NLambdaDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

RMember NLambdaDecl::ToRMember(RTypeArguments* typeArgs)
{
    throw RuntimeFatalException(); // 들어올수가 없다
}

optional<RMember> NLambdaDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    auto i = varsMap.find(name);
    if (i == varsMap.end()) return nullopt;

    return RMember_LambdaVar(typeArgs, i->second);
}

optional<RMember> NLambdaDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory)
{
    // Lambda에서 검색하지 않고, FuncContext에서 검색한다
    throw RuntimeFatalException();
}

} // namespace Citron