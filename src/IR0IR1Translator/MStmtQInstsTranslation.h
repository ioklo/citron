#pragma once
#include <expected>
#include <memory>
#include <vector>

namespace Citron {
struct Diag;
using DiagPtr = std::shared_ptr<Diag>;

class MStmt;
class QBlock;
using QFactoryPtr = std::shared_ptr<class QFactory>;

class QBodyContext;

std::expected<void, DiagPtr> TranslateMStmtsToQInsts(std::vector<MStmt*>& mStmts, QBodyContext& context);
std::expected<void, DiagPtr> TranslateMStmtToQInsts(MStmt* mStmt, QBodyContext& qBodyContext);
} // namespace Citron