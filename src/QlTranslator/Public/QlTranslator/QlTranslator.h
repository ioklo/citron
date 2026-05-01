#pragma once
#include "QlTranslatorConfig.h"

#include <memory>
#include <string>

namespace Citron {

class QData;
struct LDataImpl;
class LContextImpl;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using QFactoryPtr = std::shared_ptr<class QFactory>;

struct LData
{
    std::unique_ptr<LDataImpl> impl;
    std::string debugOutput;
    QLTRANSLATOR_API ~LData();
};

struct LContext
{
    std::unique_ptr<LContextImpl> impl;

public:
    QLTRANSLATOR_API LContext(const RFactoryPtr& rFactory, const QFactoryPtr& qFactory);
    QLTRANSLATOR_API ~LContext();
};

QLTRANSLATOR_API LData TranslateQDataToLData(QData* qData, LContext& context);

}