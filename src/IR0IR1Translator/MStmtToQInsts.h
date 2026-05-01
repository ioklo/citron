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

struct MqTranslationContexts;

template<typename T>
struct MqEmitState;

using MqLazyBlockPtr = std::shared_ptr<class MqLazyBlock>;

std::expected<MqEmitState<void>, DiagPtr> TranslateMStmtsToQInsts(std::vector<MStmt*>& mStmts, MqTranslationContexts& contexts);
std::expected<MqEmitState<void>, DiagPtr> TranslateMStmt_ScopeToQInsts_Default(MStmt_Scope* scope, MqTranslationContexts& contexts);
std::expected<MqEmitState<void>, DiagPtr> TranslateMStmt_ScopeToQInsts_Loop(MStmt_Scope* scope, QBlock* contBlock, QBlock* breakBlock, MqTranslationContexts& contexts);
std::expected<MqEmitState<void>, DiagPtr> TranslateMStmt_ScopeToQInsts_Switch(MStmt_Scope* scope, QBlock* breakBlock, MqTranslationContexts& contexts);
std::expected<MqEmitState<void>, DiagPtr> TranslateMStmt_ScopeToQInsts_Inline(MStmt_Scope* scope, const MqLazyBlockPtr& leaveBlock, size_t leaveSlotIndex, MqTranslationContexts& contexts);

std::expected<MqEmitState<void>, DiagPtr> TranslateMStmtToQInsts(MStmt* mStmt, MqTranslationContexts& contexts);
} // namespace Citron