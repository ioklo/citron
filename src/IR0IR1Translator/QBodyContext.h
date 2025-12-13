#pragma once

#include <string>
#include <memory>
#include <unordered_map>
#include <span>
#include <optional>
#include <expected>
#include <cassert>

#include "RSymbol/RNames.h"

#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QFuncBody.h"

namespace Citron {

class MExp;
class RType;
class RTypeArguments;
struct QSlotInfo;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using DiagPtr = std::shared_ptr<struct Diag>;

enum class QInst_IntrinsicKind;

struct QLocalVarInfo
{
    size_t slotIndex;
    std::string name;
    QType* qType;
};

struct QScope
{
    bool childHasReturn = false; // 이 스코프의 child가 return을 갖고 있는가
    bool handleReturn = false;   // 이 스코프에서 return을 처리했다. 더이상 명령어가 나오면 안된다

    // "a_16" -> slotIndex
    std::unordered_map<RName, QLocalVarInfo> localVarInfos;
    std::vector<size_t> slotIndices; // 이 스코프가 관리하는 slot

    // 최근 return용 cleanUp블록
    size_t coveredSlots = 0; // 어느 슬롯까지 커버했는지 count, slots의 인덱스이다 [0, slots.size())
    QBlock* recentCleanUpForReturn = nullptr; // return시 정리 블록
};

struct QIntrinsicResultType_Slot { QType* qType; };
struct QIntrinsicKindResult_Void {};
using QIntrinsicResultType = std::variant<
    QIntrinsicResultType_Slot,
    QIntrinsicKindResult_Void>;

class QBodyContext
{
    RFactoryPtr rFactory;
    QFactoryPtr qFactory;

    // 함수의 스택 변수
    QScope* curScope;
    std::vector<QSlotInfo> slotInfos;
    std::vector<QScope> scopes;
    std::optional<QArg_Slot> retSlot; // 함수의 반환값 slot

    QBlock* curBlock;
    std::vector<QBlock*> blocks;

public:
    QBodyContext(const RFactoryPtr& rFactory, const QFactoryPtr& qFactory, RType* rRetType);

    QIntrinsicResultType GetIntrinsicResultType(QInst_IntrinsicKind kind);

    QBlock* AddBlock(std::string&& debugText);
    void SetCurBlock(QBlock* block) { assert(block); curBlock = block; } // SetCurBlock으로 Unreachable 상태를 만들지 않도록 한다
    QBlock* GetCurBlock() { return curBlock; }
    bool IsUnreachable() { return curBlock == nullptr; }

private:
    std::expected<void, DiagPtr> EmitInstInternal(QInst&& inst);

public:
    template<typename TQInst, typename... TArgs> 
        requires std::convertible_to<TQInst, QInst> 
            && (!std::same_as<TQInst, QInst_Intrinsic>)
            && (!std::convertible_to<TQInst, QTermInst>)
    std::expected<void, DiagPtr> EmitInst(TQInst&& inst)
    {   
        return EmitInstInternal(std::forward<TQInst>(inst));
    }
    std::expected<void, DiagPtr> EmitIntrinsic(QInst_IntrinsicKind kind, std::optional<QArg_Slot> oDest, std::vector<QArg_Input>&& args);
    std::expected<void, DiagPtr> EmitTermInst(QTermInst&& termInst);
    
private:
    QBlock* MakeCleanUpForReturnBlock(size_t scopeIndex);
public:
    std::expected<void, DiagPtr> EmitJumpToCleanUpForReturnBlock();

public:
    QType* GetMExpQType(MExp* mExp);    
    size_t GetQTypeSize(QType* qType);

    QType* GetQTypeFromRType(RType* rType);
    QType_Class* GetStringQType();
    QType* GetBoolQType();
    QType* GetIntQType();
    QType* GetPtrQType();

    QArg_Slot GetRetSlot();

    size_t AddLocalVar(RType* type, const RName& name, std::optional<size_t> oArgIndex);
    size_t GetLocalVarSlotIndex(const RName& name);

    QArg_Slot NewSlot(QType* qType);
    std::span<QSlotInfo> GetStackSlotInfos() { return slotInfos; }
    std::span<QBlock*> GetBlocks() { return blocks; }

    QArg_Slot NewSlotForMExp(MExp* exp);
    void VerifyBlocks();

    QType* GetReturnQType(RFuncDecl* rFuncDecl, RTypeArguments& typeArgs);
    bool IsVoidQType(QType* qType);

    void PushScope();
    void PopScope();
    void CleanUpScope();

    void MarkReturnHandledOnCurScope();
    bool IsReturnHandledOnCurScope();
    
};


} // namespace Citron