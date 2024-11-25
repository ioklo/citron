#include "NCommonFuncDeclComponent.h"

#include <variant>
#include <cassert>

#include "NLambdaDecl.h"
#include "NStmt.h"

using namespace std;

namespace Citron
{

NCommonFuncDeclComponent::NCommonFuncDeclComponent(std::vector<std::string>&& typeParams, bool bSeqFunc)
    : typeParams(std::move(typeParams)), bSeqFunc(bSeqFunc)
{
}

void NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn&& funcReturn, vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    funcReturnAndParams = FuncReturnAndParams{std::move(funcReturn), std::move(funcParameters), bLastParameterVariadic};
}

void NCommonFuncDeclComponent::InitBody(vector<NStmtPtr>&& body)
{
    this->body = std::move(body);
}

NCommonFuncDeclComponent::~NCommonFuncDeclComponent() = default;

size_t NCommonFuncDeclComponent::GetTypeParamCount()
{
    return typeParams.size();
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

RFuncParameter& NCommonFuncDeclComponent::GetUnboundFuncParam(int i)
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