#include "RLambdaDecl.h"
#include "Infra/Exceptions.h"
#include "RLambdaVarDecl.h"
#include "RFactory.h"

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
    commonFuncDeclComp.InitFuncReturnAndParams(move(funcReturn), move(thisKind), move(funcParameters), bLastParameterVariadic);
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

RIdentifier RLambdaDecl::GetIdentifier()
{
    return RIdentifier{name, {}};
}

size_t RLambdaDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParamDecl* RLambdaDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeDecl* RLambdaDecl::GetTypeMember(InRef<RName> name)
{
    return genericsComp.GetTypeMember(name);
}

optional<RDeclRes> RLambdaDecl::ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    auto i = varsMap.find(*name);
    if (i == varsMap.end()) return nullopt;

    return RDeclRes_LambdaVar(typeArgs, i->second);
}

optional<RDeclRes> RLambdaDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = genericsComp.ResolveTypeParam(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    // Lambda에서 검색하지 않고, FuncContext에서 검색한다
    throw RuntimeFatalException();
}

RDecl* RLambdaDecl::RTypeDecl_GetDecl()
{
    return this;
}

RType* RLambdaDecl::GetOpenType()
{
    return rFactory->MakeLambdaType(this, MakeOpenTypeArgs(*rFactory));
}

RDeclRes RLambdaDecl::ToRDeclRes(RTypeArguments* typeArgs)
{
    throw RuntimeFatalException(); // 들어올수가 없다
}

void RLambdaDecl::Accept(RTypeDeclVisitor& visitor)
{
    visitor.Visit(this);
}

} // namespace Citron