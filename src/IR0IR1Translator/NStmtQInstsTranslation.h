#pragma once
#include <expected>
#include <memory>
#include <vector>

namespace Citron {
class Diag;
using DiagPtr = std::shared_ptr<Diag>;

class NStmt;
class QBlock;
class QFactory;

namespace IR0IR1Translator {
class QBodyContext;

std::expected<QBlock*, DiagPtr> TranslateNStmtsToQInsts(std::vector<NStmt*>& stmts, QBlock* block, QBodyContext* context, QFactory* facrtory);
std::expected<QBlock*, DiagPtr> TranslateNStmtToQInsts(NStmt* stmt, QBlock* block, QBodyContext* bodyContext, QFactory* factory);
} // namespace IR0IR1Translator
} // namespace Citron