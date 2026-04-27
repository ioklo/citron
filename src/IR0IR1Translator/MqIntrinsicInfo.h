#pragma once

#include <vector>
#include "RSymbol/RFuncReturn.h"
#include "RSymbol/RFuncParameter.h"

namespace Citron {

enum class QInst_IntrinsicKind;

struct MqIntrinsicInfo
{
    QInst_IntrinsicKind kind;
    RFuncReturn funcRet;
    std::vector<RFuncParameter> funcParams;
};

} // namespace Citron