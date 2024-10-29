#include "NCommonFuncDeclComponent.h"

#include <variant>
#include <cassert>

#include "NLambdaDecl.h"
#include "NStmt.h"

using namespace std;

namespace Citron
{

NCommonFuncDeclComponent::NCommonFuncDeclComponent() = default;

void NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn funcReturn, vector<RFuncParameter> funcParameters, bool bLastParameterVariadic)
{
    funcReturnAndParams = FuncReturnAndParams{move(funcReturn), move(funcParameters), bLastParameterVariadic};
}

void NCommonFuncDeclComponent::InitBody(vector<NStmtPtr> body)
{
    this->body = move(body);
}

NCommonFuncDeclComponent::~NCommonFuncDeclComponent() = default;

RTypePtr NCommonFuncDeclComponent::GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    assert(funcReturnAndParams);

    if (auto* confirmedReturn = get_if<RFuncReturn_Set>(&funcReturnAndParams->funcReturn))
        return confirmedReturn->type->Apply(typeArgs, factory);

    return nullptr;
}

vector<RTypePtr> NCommonFuncDeclComponent::GetParamIds()
{
    assert(funcReturnAndParams);

    vector<RTypePtr> result;
    for (auto& param : funcReturnAndParams->funcParameters)
        result.push_back(param.type);

    return result;
}

}