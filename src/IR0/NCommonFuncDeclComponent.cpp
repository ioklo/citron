#include "NCommonFuncDeclComponent.h"

#include <variant>
#include <cassert>

#include "Infra/Variants.h"

#include "RTypes.h"

#include "NLambdaDecl.h"
#include "NStmt.h"

using namespace std;

namespace Citron
{
NCommonFuncDeclComponent::NCommonFuncDeclComponent(bool bStatic, bool bSeqFunc, std::vector<std::string>&& typeParams)
    : bStatic(bStatic), bSeqFunc(bSeqFunc), typeParams(move(typeParams))
{
}

void NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn&& funcReturn, vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    funcReturnAndParams = FuncReturnAndParams{move(funcReturn), move(funcParameters), bLastParameterVariadic};
}

void NCommonFuncDeclComponent::InitBody(vector<NStmtPtr>&& body)
{
    this->body = Body_Set(move(body));
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

RFuncReturn NCommonFuncDeclComponent::GetFuncReturn(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    assert(funcReturnAndParams);

    return visit(overloaded{
        [](RFuncReturn_ForCtor&) -> RFuncReturn { return RFuncReturn_ForCtor{}; },
        [&typeArgs, &factory](RFuncReturn_Set& setReturn) -> RFuncReturn { return RFuncReturn_Set{setReturn.type->Apply(typeArgs, factory)}; },
        [](RFuncReturn_NotSet&) -> RFuncReturn { return RFuncReturn_NotSet{}; }
    }, funcReturnAndParams->funcReturn);
}


RFuncParameter& NCommonFuncDeclComponent::GetUnboundFuncParam(size_t i)
{
    assert(funcReturnAndParams);
    return funcReturnAndParams->funcParameters[i];
}

RFuncParameter NCommonFuncDeclComponent::GetFuncParam(RTypeArguments& typeArgs, size_t index, RTypeFactory& factory)
{
    assert(funcReturnAndParams);

    auto& unboundFuncParam = funcReturnAndParams->funcParameters[index];
    return unboundFuncParam.Apply(typeArgs, factory);
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