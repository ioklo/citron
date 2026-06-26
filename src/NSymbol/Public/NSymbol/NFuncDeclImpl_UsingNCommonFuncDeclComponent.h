#pragma once
#include <span>

#include "NCommonFuncDeclComponent.h"
#include "RSymbol/RFuncReturn.h"
#include "RSymbol/RFuncParameter.h"

namespace Citron {

class RType;
class RTypeArguments;

template<typename TRFuncDeclBase> requires std::derived_from<TRFuncDeclBase, RFuncDeclBase>
class NFuncDeclImpl_UsingNCommonFuncDeclComponent
    : public TRFuncDeclBase
    , protected NCommonFuncDeclComponent
{
public:
    NFuncDeclImpl_UsingNCommonFuncDeclComponent(bool bSeqFunc)
        : NCommonFuncDeclComponent{bSeqFunc}
    {
    }

    // from RFuncDeclBase
    RThisKind GetThisKind() override { return NCommonFuncDeclComponent::GetThisKind(); }
    size_t GetParamCount() override { return NCommonFuncDeclComponent::GetParamCount(); }
    RType* GetReturnType(RTypeArguments* typeArgs) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs); }
    RFuncReturn GetFuncReturn(RTypeArguments* typeArgs) override { return NCommonFuncDeclComponent::GetFuncReturn(typeArgs); }
    RFuncParameter GetFuncParam(RTypeArguments* typeArgs, size_t index) override { return NCommonFuncDeclComponent::GetFuncParam(typeArgs, index); }
    RFuncReturn GetUnboundFuncReturn() override { return NCommonFuncDeclComponent::GetUnboundFuncReturn(); }
    std::span<RFuncParameter> GetUnboundFuncParams() override { return NCommonFuncDeclComponent::GetUnboundFuncParams(); }
};

} // namespace Citron