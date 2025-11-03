#pragma once
#include "IR0IR1TranslatorConfig.h"
#include "QIR/QData.h"

#include <memory>
#include <expected>

namespace Citron {

struct MData;
class QFactory;
using QFactoryPtr = std::shared_ptr<QFactory>;
struct Diag;
using DiagPtr = std::shared_ptr<Diag>;

IR0IR1TRANSLATOR_API std::expected<QFuncBody, DiagPtr> TranslateMFuncBodyToQFuncBody(MFuncBody& mFuncBody, QFactoryPtr& factory)
IR0IR1TRANSLATOR_API std::expected<QData*, DiagPtr> TranslateMDataToQData(MData* mData, QFactoryPtr& factory);

}