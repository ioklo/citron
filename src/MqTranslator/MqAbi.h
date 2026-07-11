#pragma once

#include "MIR/MCallable.h"

namespace Citron {

class RType;
class RTypeArguments;
struct MqFuncInfo;
struct MqIntrinsicInfo;

class MqAbi
{
public:
    virtual ~MqAbi() = default;
    virtual size_t GetTypeSize(RType* type) = 0;
    virtual MqFuncInfo GetFuncInfo(RFuncDecl* rFuncDecl, RTypeArguments* typeArgs) = 0;
    virtual MqFuncInfo GetFuncInfo(MqIntrinsicInfo& intrinsicInfo, RTypeArguments* typeArgs) = 0;
};


} // namespace Citron
