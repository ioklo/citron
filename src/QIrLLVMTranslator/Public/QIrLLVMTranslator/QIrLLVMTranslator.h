#pragma once
#include <memory>

namespace Citron {

class QData;
struct LDataImpl;
struct LContextImpl;

struct LData
{
    std::unique_ptr<LDataImpl> impl;
};

struct LContext
{
    std::unique_ptr<LContextImpl> impl;
};

LData* TranslateQDataToLData(QData* qData, LContext& context);

}