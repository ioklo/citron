#pragma once
#include <expected>
#include <memory>
#include <vector>

namespace Citron {
struct Diag;
using DiagPtr = std::shared_ptr<Diag>;

class MStmt;
class QBlock;
class QFactory;

namespace IR0IR1Translator {
class QBodyContext;

std::expected<QBlock*, DiagPtr> TranslateMStmtsToQInsts(std::vector<MStmt*>& stmts, QBlock* block, QBodyContext* context, QFactory* facrtory);
std::expected<QBlock*, DiagPtr> TranslateMStmtToQInsts(MStmt* stmt, QBlock* block, QBodyContext* bodyContext, QFactory* factory);
} // namespace IR0IR1Translator
} // namespace Citron