#include "RLambdaDecl.h"
#include "Infra/Exceptions.h"
#include "RLambdaVarDecl.h"
#include "RFactory.h"
#include "RTypeRes.h"
#include "RMember.h"

using namespace std;

namespace Citron {

RLambdaDecl::RLambdaDecl(RFuncDecl* outer, RName&& name, TakeRef<RFactoryPtr> rFactory)
    : outer{outer}
    , name{move(name)}
    , genericsComp{}
    , commonFuncDeclComp{/*bSeqFunc*/false}
    , ImplRFuncDeclUsingCommonComponents{this, commonFuncDeclComp}
    , rFactory{rFactory.Take()}

{
    genericsComp.InitTypeParams({});
}

void RLambdaDecl::InitFuncReturnAndParameters(RFuncReturn&& funcReturn, RThisKind&& thisKind, vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    commonFuncDeclComp.InitFuncSignature(move(funcReturn), move(thisKind), move(funcParameters), bLastParameterVariadic);
}

void RLambdaDecl::InitVars(vector<RLambdaVarDecl*>&& vars)
{
    for (auto& var : vars)
        varsMap.emplace(var->GetName(), var);

    this->vars = move(vars);
}

RDecl* RLambdaDecl::GetOuter()
{
    return outer->RFuncDecl_GetDecl();
}

size_t RLambdaDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParam* RLambdaDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeParam* RLambdaDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComp.GetTypeParam(name);
}

RTypeDecl* RLambdaDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RLambdaDecl::GetMember(InRef<RName> name)
{
    // TODO: [65] 2026-07-06, RLambdaDecl제거, RStructDecl을 쓰도록 변경
    throw NotImplementedException{};
}

RDecl* RLambdaDecl::RTypeDecl_GetDecl()
{
    return this;
}

void RLambdaDecl::Accept(RTypeDeclVisitor& visitor)
{
    visitor.Visit(this);
}

} // namespace Citron