#include "NLambdaDecl.h"

#include "Infra/Exceptions.h"

using namespace std;

namespace Citron {

NLambdaDecl::NLambdaDecl(NFuncDeclOuter* outer, RName&& name)
    : outer{outer}
    , name{move(name)}
    , NCommonFuncDeclComponent{RThisKind::Ptr, /*bSeqFunc*/false} // TODO: Ptr을 instance로 넣지 않는 최적화 가능
{   
    NGenericsComponent::InitTypeParams({});
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
    return NGenericsComponent::GetTypeMember(name, typeParamCount);
}

RDeclRes NLambdaDecl::ToRDeclRes(RTypeArguments* typeArgs)
{
    throw RuntimeFatalException(); // 들어올수가 없다
}

optional<RDeclRes> NLambdaDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    auto i = varsMap.find(name);
    if (i == varsMap.end()) return nullopt;

    return RDeclRes_LambdaVar(typeArgs, i->second);
}

optional<RDeclRes> NLambdaDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = NGenericsComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    // Lambda에서 검색하지 않고, FuncContext에서 검색한다
    throw RuntimeFatalException();
}

} // namespace Citron