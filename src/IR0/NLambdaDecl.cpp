module Citron.NDecls:NLambdaDecl;

import Citron.Exceptions;

using namespace std;

namespace Citron {

NLambdaDecl::NLambdaDecl(NFuncDeclOuterWPtr&& outer, RName&& name, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
    : NCommonFuncDeclComponent(/*bStatic*/ false, /*bSeqFunc*/ false, /*typeParams*/ {}), outer(move(outer)), name(move(name))
{
    NCommonFuncDeclComponent::InitFuncReturnAndParams(move(funcReturn), move(funcParameters), bLastParameterVariadic);
}

void NLambdaDecl::Init(std::vector<std::shared_ptr<NLambdaVarDecl>>&& vars, std::vector<NStmtPtr>&& body)
{
    for (auto& var : vars)
        varsMap.emplace(var->name, var);

    this->vars = move(vars);

    NCommonFuncDeclComponent::InitBody(move(body));
}

NDecl* NLambdaDecl::GetNOuter()
{
    return outer.lock()->GetNDecl();
}

RDecl* NLambdaDecl::GetROuter()
{
    return outer.lock()->GetNDecl()->GetRDecl();
}

RIdentifier NLambdaDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

RMember NLambdaDecl::ToRMember(const std::shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs)
{
    throw RuntimeFatalException(); // 들어올수가 없다
}

optional<RMember> NLambdaDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    auto i = varsMap.find(name);
    if (i == varsMap.end()) return nullopt;

    return RMember_LambdaVar(typeArgs, i->second);
}

optional<RMember> NLambdaDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    // Lambda에서 검색하지 않고, FuncContext에서 검색한다
    throw RuntimeFatalException();
}

} // namespace Citron