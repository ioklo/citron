#pragma once
#include "QIRConfig.h"

#include <vector>
#include <span>

namespace Citron {

struct QFuncBody;

class QData
{
    std::vector<QFuncBody> bodies;

public:
    QIR_API QData(std::vector<QFuncBody>&& bodies);
    QIR_API std::span<QFuncBody> GetAllBodies();
};

} // namespace Citron
