#pragma once
#include "QEvaluatorConfig.h"

#include <span>
#include <expected>
#include <memory>
#include <string>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class RModule;
class QData;
class NGlobalFuncDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;

class IEvalQDataCommandHandler
{
public:
    virtual void Execute(const std::string& str) = 0;
};

using IEvalQDataCommandHandlerPtr = std::shared_ptr<IEvalQDataCommandHandler>;

QEVALUATOR_API std::expected<void, DiagPtr> EvaluateQData(std::span<RModule*> rModules, QData* qData, NGlobalFuncDecl* nEntry, IEvalQDataCommandHandlerPtr&& cmdHandler, const RFactoryPtr& rFactory);

} // namespace Citron
