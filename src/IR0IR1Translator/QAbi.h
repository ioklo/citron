#pragma once

#include "MIR/MCallable.h"

namespace Citron {

class RFuncDecl;
class RType;
class RTypeArguments;
struct QFuncInfo;
struct MqIntrinsicInfo;

class QAbi
{
public:
    virtual ~QAbi() = default;
    virtual size_t GetTypeSize(RType* type) = 0;
    virtual QFuncInfo GetFuncInfo(RFuncDecl* rFuncDecl, RTypeArguments* typeArgs) = 0;
    virtual QFuncInfo GetFuncInfo(MqIntrinsicInfo& intrinsicInfo, RTypeArguments* typeArgs) = 0;
};


} // namespace Citron
