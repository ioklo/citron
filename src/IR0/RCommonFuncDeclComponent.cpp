#include "RCommonFuncDeclComponent.h"

#include <variant>
#include <cassert>

#include "RLambdaDecl.h"
#include "RStmt.h"

using namespace std;

namespace Citron
{

RCommonFuncDeclComponent::RCommonFuncDeclComponent() = default;

void RCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn funcReturn, vector<RFuncParameter> funcParameters, bool bLastParameterVariadic)
{
    funcReturnAndParams = FuncReturnAndParams{move(funcReturn), move(funcParameters), bLastParameterVariadic};
}

void RCommonFuncDeclComponent::InitBody(vector<RStmtPtr> body)
{
    this->body = move(body);
}

RCommonFuncDeclComponent::~RCommonFuncDeclComponent() = default;

RTypePtr RCommonFuncDeclComponent::GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    assert(funcReturnAndParams);

    if (auto* confirmedReturn = get_if<RConfirmedFuncReturn>(&funcReturnAndParams->funcReturn))
        return confirmedReturn->type->Apply(typeArgs, factory);

    return nullptr;
}

vector<RTypePtr> RCommonFuncDeclComponent::GetParamIds()
{
    assert(funcReturnAndParams);

    vector<RTypePtr> result;
    for (auto& param : funcReturnAndParams->funcParameters)
        result.push_back(param.type);

    return result;
}

}