#pragma once
#include "QEvaluatorConfig.h"

#include <span>
#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class RModule;
class QData;
class NGlobalFuncDecl;

QEVALUATOR_API std::expected<void, DiagPtr> Evaluate(std::span<RModule> modules, QData& qData, NGlobalFuncDecl* nEntry);

} // namespace Citron
