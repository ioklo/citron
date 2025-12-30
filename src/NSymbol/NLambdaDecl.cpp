#include "NLambdaDecl.h"

#include "Infra/Exceptions.h"

using namespace std;

namespace Citron {

NLambdaDecl::NLambdaDecl(NFuncDeclOuter* outer, RName&& name)
    : outer{outer}
    , name{move(name)}
    , NCommonFuncDeclComponent(/*bStatic*/false, /*bSeqFunc*/false, /*typeParams*/{})
{   
}

void NLambdaDecl::Init(RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
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

RTypeDecl* NLambdaDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    // TODO: [26] typeParams에서도 검색 (NTypeParamDecl, RType_TypeVar 추가 필요), lambda에 type params가 추가될까?
    return nullptr;
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