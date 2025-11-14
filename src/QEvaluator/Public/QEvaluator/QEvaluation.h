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

class IEvalQDataCommandHandler
{
};

using IEvalQDataCommandHandlerPtr = std::shared_ptr<IEvalQDataCommandHandler>;

QEVALUATOR_API std::expected<void, DiagPtr> EvaluateQData(std::span<RModule*> modules, QData* qData, NGlobalFuncDecl* nEntry, IEvalQDataCommandHandlerPtr&& cmdHandler);

} // namespace Citron
