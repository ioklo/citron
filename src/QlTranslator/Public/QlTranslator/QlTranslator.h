#pragma once
#include "QlTranslatorConfig.h"
#include <memory>
#include <string>
#include "Infra/RefPtr.h"

namespace Citron {

class QData;
struct LDataImpl;
class LContextImpl;
class RFactory;
class QFactory;

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
    QLTRANSLATOR_API LContext(InRefPtr<RFactory> rFactory, InRefPtr<QFactory> qFactory);
    QLTRANSLATOR_API ~LContext();
};

QLTRANSLATOR_API LData TranslateQDataToLData(QData* qData, LContext& context);

}