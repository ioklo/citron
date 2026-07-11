#pragma once
#include "MqTranslatorConfig.h"
#include "QIR/QData.h"
#include "QIR/QFuncBody.h"

#include <memory>
#include <expected>

namespace Citron {

struct MFuncBody;
class MData;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using QFactoryPtr = std::shared_ptr<class QFactory>;
using DiagPtr = std::shared_ptr<struct Diag>;

MQTRANSLATOR_API std::expected<QData*, DiagPtr> TranslateMDataToQData(MData* mData, TakeRef<RFactoryPtr> rFactory, TakeRef<QFactoryPtr> qFactory);

}