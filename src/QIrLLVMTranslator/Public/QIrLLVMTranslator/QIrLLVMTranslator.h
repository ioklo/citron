#pragma once
#include "QIrLLVMTranslatorConfig.h"

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
    QIRLLVMTRANSLATOR_API ~LData();
};

struct LContext
{
    std::unique_ptr<LContextImpl> impl;

public:
    QIRLLVMTRANSLATOR_API LContext(const RFactoryPtr& rFactory, const QFactoryPtr& qFactory);
    QIRLLVMTRANSLATOR_API ~LContext();
};

QIRLLVMTRANSLATOR_API LData TranslateQDataToLData(QData* qData, LContext& context);

}