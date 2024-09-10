#include "RCommonFuncDeclComponent.h"
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

}