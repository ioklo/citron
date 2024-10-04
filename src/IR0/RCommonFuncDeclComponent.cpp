#include "RCommonFuncDeclComponent.h"

#include <variant>
#include <cassert>

#include "RLambdaDecl.h"
#include "RStmt.h"


namespace Citron
{

RCommonFuncDeclComponent::RCommonFuncDeclComponent() = default;

void RCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn funcReturn, std::vector<RFuncParameter> funcParameters, bool bLastParameterVariadic)
{
    funcReturnAndParams = FuncReturnAndParams{std::move(funcReturn), std::move(funcParameters), bLastParameterVariadic};
}

void RCommonFuncDeclComponent::InitBody(std::vector<RStmtPtr> body)
{
    this->body = std::move(body);
}

RCommonFuncDeclComponent::~RCommonFuncDeclComponent() = default;

RTypePtr RCommonFuncDeclComponent::GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    assert(funcReturnAndParams);

    if (auto* confirmedReturn = get_if<RConfirmedFuncReturn>(&funcReturnAndParams->funcReturn))
        return confirmedReturn->type->Apply(typeArgs, factory);

    return nullptr;
}

}