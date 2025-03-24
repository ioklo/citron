#include "NCommonFuncDeclComponent.h"

#include <variant>
#include <cassert>

#include <Infra/Variants.h>

#include "NLambdaDecl.h"
#include "NStmt.h"

using namespace std;

namespace Citron
{

NCommonFuncDeclComponent::NCommonFuncDeclComponent(bool bStatic, bool bSeqFunc, std::vector<std::string>&& typeParams)
    : bStatic(bStatic), bSeqFunc(bSeqFunc), typeParams(std::move(typeParams))
{
}

void NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn&& funcReturn, vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    funcReturnAndParams = FuncReturnAndParams{std::move(funcReturn), std::move(funcParameters), bLastParameterVariadic};
}

void NCommonFuncDeclComponent::InitBody(vector<NStmtPtr>&& body)
{
    this->body = Body_Set(std::move(body));
}

void NCommonFuncDeclComponent::InitBodyWillBeGenerated()
{
    this->body = Body_WillBeGenerated();
}

NCommonFuncDeclComponent::~NCommonFuncDeclComponent() = default;

size_t NCommonFuncDeclComponent::GetTypeParamCount()
{
    return typeParams.size();
}

size_t NCommonFuncDeclComponent::GetParamCount()
{
    assert(funcReturnAndParams);
    return funcReturnAndParams->funcParameters.size();
}

RFuncReturn NCommonFuncDeclComponent::GetUnboundFuncReturn()
{
    assert(funcReturnAndParams);
    return funcReturnAndParams->funcReturn;
}

RTypePtr NCommonFuncDeclComponent::GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    assert(funcReturnAndParams);

    auto* setReturn = get_if<RFuncReturn_Set>(&funcReturnAndParams->funcReturn);
    assert(setReturn);

    return setReturn->type->Apply(typeArgs, factory);
}

RFuncParameter& NCommonFuncDeclComponent::GetUnboundFuncParam(size_t i)
{
    assert(funcReturnAndParams);
    return funcReturnAndParams->funcParameters[i];
}

vector<RTypePtr> NCommonFuncDeclComponent::GetParamIds()
{
    assert(funcReturnAndParams);

    vector<RTypePtr> result;
    for (auto& param : funcReturnAndParams->funcParameters)
        result.push_back(param.type);

    return result;
}

optional<RMember> NCommonFuncDeclComponent::ResolveIdentifier(size_t baseTypeParamCount, const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    auto* normalName = get_if<RName_Normal>(&name);
    if (!normalName) return nullopt;

    size_t typeParamCount = typeParams.size();
    for (size_t i = 0; i < typeParamCount; i++)
        if (typeParams[i] == normalName->text)
            return RMember_TypeVar(baseTypeParamCount + i);

    return nullopt;
} 

}