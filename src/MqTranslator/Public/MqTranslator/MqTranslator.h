#pragma once
#include "MqTranslatorConfig.h"
#include "Infra/RefPtr.h"
#include "QIR/QData.h"
#include "QIR/QFuncBody.h"

#include <memory>
#include <expected>

namespace Citron {

struct MFuncBody;
class MData;
class RFactory;
class QFactory;
using DiagPtr = std::shared_ptr<struct Diag>;

MQTRANSLATOR_API std::expected<QData*, DiagPtr> TranslateMDataToQData(MData* mData, InRefPtr<RFactory> rFactory, InRefPtr<QFactory> qFactory);

}