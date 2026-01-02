#include "NCommonFuncDeclComponent.h"

#include <variant>
#include <cassert>

#include "Infra/Variants.h"

#include "RSymbol/RTypes.h"

#include "NLambdaDecl.h"

using namespace std;

namespace Citron
{

NCommonFuncDeclComponent::NCommonFuncDeclComponent(bool bStatic, bool bSeqFunc, std::vector<std::string>&& typeParams)
    : bStatic{bStatic}
    , bSeqFunc{bSeqFunc}
    , typeParams{move(typeParams)}
{
}

void NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn&& funcReturn, vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    funcReturnAndParams = FuncReturnAndParams{move(funcReturn), move(funcParameters), bLastParameterVariadic};
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

RType* NCommonFuncDeclComponent::GetReturnType(RTypeArguments& typeArgs)
{
    assert(funcReturnAndParams);

    auto* setReturn = get_if<RFuncReturn_Set>(&funcReturnAndParams->funcReturn);
    assert(setReturn);

    return setReturn->type->Apply(typeArgs);
}

RFuncReturn NCommonFuncDeclComponent::GetFuncReturn(RTypeArguments& typeArgs)
{
    assert(funcReturnAndParams);

    return visit([&typeArgs](auto& funcReturn) -> RFuncReturn {
        using T = remove_cvref_t<decltype(funcReturn)>;

        if constexpr (same_as<T, RFuncReturn_ForCtor>) 
            return RFuncReturn_ForCtor{};
        else if constexpr (same_as<T, RFuncReturn_Set>)
            return RFuncReturn_Set{funcReturn.type->Apply(typeArgs)};
        else if constexpr (same_as<T, RFuncReturn_NotSet>)
            return RFuncReturn_NotSet{};
        else static_assert(false);

    }, funcReturnAndParams->funcReturn);
}

span<RFuncParameter> NCommonFuncDeclComponent::GetUnboundFuncParams()
{
    assert(funcReturnAndParams);
    return funcReturnAndParams->funcParameters;
}

RFuncParameter NCommonFuncDeclComponent::GetFuncParam(RTypeArguments& typeArgs, size_t index)
{
    assert(funcReturnAndParams);

    auto& unboundFuncParam = funcReturnAndParams->funcParameters[index];
    return unboundFuncParam.Apply(typeArgs);
}


vector<RType*> NCommonFuncDeclComponent::GetParamIds()
{
    assert(funcReturnAndParams);

    vector<RType*> result;
    for (auto& param : funcReturnAndParams->funcParameters)
        result.push_back(param.type);

    return result;
}

optional<RMember> NCommonFuncDeclComponent::ResolveIdentifier(size_t baseTypeParamCount, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    auto* normalName = get_if<RName_Normal>(&name);
    if (!normalName) return nullopt;

    size_t typeParamCount = typeParams.size();
    for (size_t i = 0; i < typeParamCount; i++)
        if (typeParams[i] == normalName->text)
            return RMember_TypeVar(baseTypeParamCount + i);

    assert(funcReturnAndParams);
    for (auto& param : funcReturnAndParams->funcParameters)
        if (param.name == name) return RMember_LocalVar{param.type, param.name};

    return nullopt;
} 

}