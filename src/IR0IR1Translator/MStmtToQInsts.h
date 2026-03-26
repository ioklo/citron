#pragma once
#include <expected>
#include <memory>
#include <vector>

namespace Citron {
struct Diag;
using DiagPtr = std::shared_ptr<Diag>;

struct MStmt;
class QBlock;
using QFactoryPtr = std::shared_ptr<class QFactory>;

struct QTranslationContexts;

std::expected<void, DiagPtr> TranslateMStmtsToQInsts(std::vector<MStmt*>& mStmts, QTranslationContexts& context);
std::expected<void, DiagPtr> TranslateMStmtsToQInstsWithNewScope(std::vector<MStmt*>& mStmts, QTranslationContexts& context);
std::expected<void, DiagPtr> TranslateMStmtToQInsts(MStmt* mStmt, QTranslationContexts& qBodyContext);
} // namespace Citron