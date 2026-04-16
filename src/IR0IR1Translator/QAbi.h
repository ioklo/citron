#pragma once

#include "MIR/MCallable.h"

namespace Citron {

class RFuncDecl;
class RType;
class RTypeArguments;
struct QFuncInfo;

class QAbi
{
public:
    virtual ~QAbi() = default;
    virtual size_t GetTypeSize(RType* type) = 0;
    virtual QFuncInfo GetFuncInfo(MCallable& callable) = 0;
};


} // namespace Citron
