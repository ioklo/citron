#pragma once
#include "IR0IR1TranslatorConfig.h"
#include "QIR/QData.h"
#include "QIR/QFuncBody.h"

#include <memory>
#include <expected>

namespace Citron {

struct MFuncBody;
class MData;
using QFactoryPtr = std::shared_ptr<class QFactory>;
using DiagPtr = std::shared_ptr<struct Diag>;

IR0IR1TRANSLATOR_API std::expected<QData*, DiagPtr> TranslateMDataToQData(MData* mData, QFactoryPtr& qFactory);

}