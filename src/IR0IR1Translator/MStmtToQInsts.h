#pragma once
#include <expected>
#include <memory>
#include <vector>

namespace Citron {
using DiagPtr = std::shared_ptr<struct Diag>;

struct MStmt;
struct MStmt_Scope;
class QBlock;
using QFactoryPtr = std::shared_ptr<class QFactory>;

struct QTranslationContexts;

template<typename T>
struct QEmitState;

using QLazyBlockPtr = std::shared_ptr<class QLazyBlock>;

std::expected<QEmitState<void>, DiagPtr> TranslateMStmtsToQInsts(std::vector<MStmt*>& mStmts, QTranslationContexts& contexts);
std::expected<QEmitState<void>, DiagPtr> TranslateMStmt_ScopeToQInsts_Default(MStmt_Scope* scope, QTranslationContexts& contexts);
std::expected<QEmitState<void>, DiagPtr> TranslateMStmt_ScopeToQInsts_Loop(MStmt_Scope* scope, QBlock* contBlock, QBlock* breakBlock, QTranslationContexts& contexts);
std::expected<QEmitState<void>, DiagPtr> TranslateMStmt_ScopeToQInsts_Switch(MStmt_Scope* scope, QBlock* breakBlock, QTranslationContexts& contexts);
std::expected<QEmitState<void>, DiagPtr> TranslateMStmt_ScopeToQInsts_Inline(MStmt_Scope* scope, const QLazyBlockPtr& leaveBlock, size_t leaveSlotIndex, QTranslationContexts& contexts);

std::expected<QEmitState<void>, DiagPtr> TranslateMStmtToQInsts(MStmt* mStmt, QTranslationContexts& contexts);
} // namespace Citron