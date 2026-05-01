#pragma once

#include <string>
#include <memory>
#include <unordered_map>
#include <span>
#include <optional>
#include <expected>
#include <cassert>

#include "Infra/SmallMap.h"
#include "RSymbol/RNames.h"

#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QFuncBody.h"

#include "QJumpBlockInfo.h"

namespace Citron {

class RType;
class RTypeArguments;
struct QSlotInfo;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using DiagPtr = std::shared_ptr<struct Diag>;
class QBodyContext;
class QAbi;

enum class QInst_IntrinsicKind;

struct QLocalInfo_Var
{
    size_t slotIndex;
    RName name;
};

struct QLocalInfo_RefAlias
{
    size_t slotIndex; // slotIndex를 다른 var와 공유
    RName name;
};

struct QLocalInfo_RefPtr
{
    size_t slotIndex; // ptr slot
    RName name;
    RType* type; // 참조하는 타입
};

using QLocalInfo = std::variant<QLocalInfo_Var, QLocalInfo_RefAlias, QLocalInfo_RefPtr>;

struct QCleanUpKind_Return {};
struct QCleanUpKind_Continue { size_t labelId; };
struct QCleanUpKind_Break { size_t labelId; };
struct QCleanUpKind_Leave { size_t labelId; };

inline bool operator==(QCleanUpKind_Return const& x, QCleanUpKind_Return const& y) { return true; }
inline bool operator==(QCleanUpKind_Continue const& x, QCleanUpKind_Continue const& y) { return x.labelId == y.labelId; }
inline bool operator==(QCleanUpKind_Break const& x, QCleanUpKind_Break const& y) { return x.labelId == y.labelId; }
inline bool operator==(QCleanUpKind_Leave const& x, QCleanUpKind_Leave const& y) { return x.labelId == y.labelId; }

using QCleanUpKind = std::variant<
    QCleanUpKind_Return,
    QCleanUpKind_Continue,
    QCleanUpKind_Break,
    QCleanUpKind_Leave>;

struct QCleanUpInfo
{
    size_t managedCoveredSlots = 0; // 어느 슬롯까지 커버했는지 count, slots의 인덱스이다 [0, slots.size())
    QBlock* recentCleanUpBlock = nullptr; // return시 정리 블록
};

struct QScope
{
    std::optional<size_t> o_labelId; // continue, break에 필요하다

    // "a_16" -> slotIndex
    std::unordered_map<RName, QLocalInfo> localInfos;
    std::vector<size_t> managedSlotIndices; // 이 스코프가 관리하는 slot

    SmallMap<QCleanUpKind, QCleanUpInfo> cleanUpInfos; // 이 스코프에서 관리하는 cleanUp 정보들. return/continue/break마다 하나씩 필요할 수 있다

    QCleanUpInfo& GetOrAddCleanUpInfo(QCleanUpKind kind);
};

struct QJumpBlockScopeGuard
{
    QBodyContext& bodyContext;
    QJumpBlockScopeGuard(QJumpBlockInfo&& info, QBodyContext& bodyContext);
    ~QJumpBlockScopeGuard();
};

class QBodyContext
{
    RFactoryPtr rFactory;
    QFactoryPtr qFactory;

    // 함수의 스택 변수
    QScope* curScope;
    std::vector<QSlotInfo> slotInfos;
    std::vector<QScope> scopes;
    
    QBlock* curBlock;
    std::vector<QBlock*> blocks;
    std::vector<QJumpBlockInfo> jumpBlockInfos; // break, continue할 때 필요한 블록 정보들. 스코프가 바뀔 때마다 push/pop한다.
    SmallMap<std::string, size_t> labelIds;

public:
    QBodyContext(const RFactoryPtr& rFactory, const QFactoryPtr& qFactory, RType* rRetType);

    QBlock* AddBlock(std::string&& debugText);
    void SetCurBlock(QBlock* block) { assert(block); curBlock = block; } // SetCurBlock으로 Unreachable 상태를 만들지 않도록 한다
    QBlock* GetCurBlock() { return curBlock; }

    QBlock* GetContinueBlock(size_t labelId);
    QBlock* GetBreakBlock(size_t labelId);
    QBlock* GetLeaveBlock(size_t labelId);

private:
    void EmitInstInternal(QInst&& inst);

public:
    template<typename TQInst> 
        requires std::convertible_to<TQInst, QInst> 
            && (!std::same_as<TQInst, QInst_Intrinsic>)
            && (!std::convertible_to<TQInst, QTermInst>)
    void EmitInst(TQInst&& inst)
    {   
        return EmitInstInternal(std::forward<TQInst>(inst));
    }
    void EmitIntrinsic(QInst_IntrinsicKind kind, std::optional<QArg_Dest> o_dest, std::vector<QArg_CallArg>&& args);
    void EmitTermInst(QTermInst&& termInst);

private:
    bool IsFinalScope(QCleanUpKind kind, size_t scopeIndex);
    QBlock* MakeCleanUpBlockWithoutFinalize(std::span<size_t> managedSlotIndices);
    void FinalizeCleanupBlock(QBlock* block, QCleanUpKind kind);
    QBlock* GetCleanUpBlock(QCleanUpKind kind, size_t scopeIndex);

public:
    void EmitJumpToCleanUpBlock(QCleanUpKind kind);

public:
    RType* GetStringType();
    RType* GetBoolType();
    RType* GetIntType();
    RType* GetPtrType();
    RType* GetPtrType(RType* innerType);

    std::optional<size_t> GetLeaveSlotIndex(size_t labelId);

    std::optional<QLocalInfo> GetLocalInfo(const RName& name);

    size_t AddLocalVar(RType* type, const RName& name);
    size_t AddArgument(RType* type, const RName& name, size_t index);
    void AddRefArgument(RType* type, const RName& name, size_t index);

    void AddLocalRef_Alias(const RName& rName, size_t slotIndex);
    void AddLocalRef_Ptr(RType* rType, const RName& rName, size_t slotIndex);

    size_t AddParameter(RType* type, QAbi* abi);
    size_t AddTemp(RType* type, std::string&& debugText);
    size_t AddThis(RType* type);
    std::span<QSlotInfo> GetStackSlotInfos() { return slotInfos; }
    std::span<QBlock*> GetBlocks() { return blocks; }
    RType* GetSlotType(size_t i) { return slotInfos[i].type; }
    QSlotRole& GetSlotRole(size_t i) { return slotInfos[i].role; }

    void VerifyBlocks();

    bool IsVoidType(RType* rType);

    void PushScope(std::optional<size_t> o_labelId);
    void PopScope();
    void CleanUpScope();

    void PushJumpBlockInfo(QJumpBlockInfo&& info) { jumpBlockInfos.push_back(std::move(info)); }
    void PopJumpBlockInfo() { jumpBlockInfos.pop_back(); }

    void MarkReturnHandledOnCurScope();
    bool IsReturnHandledOnCurScope();
    
};


} // namespace Citron