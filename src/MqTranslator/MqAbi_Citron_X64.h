#pragma once
#include <memory>
#include "RSymbol/RFuncReturn.h"
#include "MqAbi.h"
#include "MqFuncInfo.h"

namespace Citron {

using RFactoryPtr = std::shared_ptr<class RFactory>;

class MqAbi_Citron_X64 : public MqAbi
{
    RFactoryPtr rFactory;

public:
    MqAbi_Citron_X64(const RFactoryPtr& rFactory);

    size_t GetTypeSize(RType* type) override;    
    MqFuncInfo GetFuncInfo(RFuncDecl& rFuncDecl, RTypeArguments* typeArgs) override;
    MqFuncInfo GetFuncInfo(MqIntrinsicInfo& intrinsicInfo, RTypeArguments* typeArgs) override;

private:
    MqReturnPassingMode GetReturnPassingMode(RFuncReturn funcRet, size_t* outCurArgIndex);
};

} // namespace Citron
