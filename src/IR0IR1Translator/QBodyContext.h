#pragma once

#include <string>
#include <memory>
#include <unordered_map>
#include <span>
#include <optional>

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

enum class QInst_IntrinsicKind;

class QBlockWriter
{
    QBlock* curBlock;
    std::vector<QBlock*> blocks;
    QFactoryPtr qFactory;

public:
    QBlockWriter(const QFactoryPtr& qFactory, std::string&& blockName);
    QBlock* AddBlock(std::string&& debugText);

    QBlock* GetCurBlock() { return curBlock; }
    std::span<QBlock*> GetBlocks() { return blocks; }
    void SetCurBlock(QBlock* block);
    void Verify();

    bool CurBlockEndsWithTermInst();
};

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

class QBodyContext : QBlockWriter
{
    RFactoryPtr rFactory;
    QFactoryPtr qFactory;

    // 함수의 스택 변수
    QBlock* bodyBlock;
    QScope* curScope;
    std::vector<QSlotInfo> slotInfos;
    std::vector<QScope> scopes;
    std::optional<QArg_Slot> retSlot; // 함수의 반환값 slot

public:
    QBodyContext(const RFactoryPtr& rFactory, const QFactoryPtr& qFactory, RType* rRetType);

    QIntrinsicResultType GetIntrinsicResultType(QInst_IntrinsicKind kind);

    QBlock* AddBlock(std::string&& debugText) { return QBlockWriter::AddBlock(std::move(debugText)); }
    template<typename TQInst, typename... TArgs> 
        requires std::convertible_to<TQInst, QInst>
            && (!std::same_as<TQInst, QInst_Intrinsic>)
    void EmitInst(TQInst&& inst)
    {
        QBlockWriter::GetCurBlock()->EmitInst(std::move(inst));
    }
    void EmitIntrinsic(QInst_IntrinsicKind kind, std::optional<QArg_Slot> oDest, std::vector<QArg_Input>&& args);
    QBlock* MakeCleanUpForReturnBlock(size_t scopeIndex);
    void EmitJumpToCleanUpForReturnBlock();

    void SetCurBlock(QBlock* block) { QBlockWriter::SetCurBlock(block); }
    bool CurBlockEndsWithTermInst() { return QBlockWriter::CurBlockEndsWithTermInst(); }

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
    std::span<QBlock*> GetBlocks() { return QBlockWriter::GetBlocks(); }

    QArg_Slot NewSlotForMExp(MExp* exp);

    void CompleteFunc();

    QType* GetReturnQType(RFuncDecl* rFuncDecl, RTypeArguments& typeArgs);
    bool IsVoidQType(QType* qType);

    void PushScope();
    void PopScope();
    void CleanUpScope();

    void MarkReturnHandledOnCurScope();
    bool IsReturnHandledOnCurScope();
    
};


} // namespace Citron