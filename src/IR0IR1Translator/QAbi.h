#pragma once

#include "MIR/MCallable.h"

namespace Citron {

class RFuncDecl;
class RType;
class RTypeArguments;
struct QFuncInfo;
struct QIntrinsicInfo;

class QAbi
{
public:
    virtual ~QAbi() = default;
    virtual size_t GetTypeSize(RType* type) = 0;
    virtual QFuncInfo GetFuncInfo(RFuncDecl* rFuncDecl, RTypeArguments* typeArgs) = 0;
    virtual QFuncInfo GetFuncInfo(QIntrinsicInfo* intrinsicInfo, RTypeArguments* typeArgs) = 0;
};


} // namespace Citron
