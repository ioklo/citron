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

class RType;
class RTypeArguments;
struct QSlotInfo;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using DiagPtr = std::shared_ptr<struct Diag>;

enum class QInst_IntrinsicKind;

struct QLocalInfo_Var
{
    size_t slotIndex;
    std::string name;
};

struct QLocalInfo_RefAlias
{
    size_t slotIndex; // slotIndex를 다른 var와 공유
    std::string name;
};

struct QLocalInfo_RefPtr
{
    size_t slotIndex; // ptr slot
    std::string name;
    RType* type; // 참조하는 타입
};

using QLocalInfo = std::variant<QLocalInfo_Var, QLocalInfo_RefAlias, QLocalInfo_RefPtr>;

struct QScope
{
    bool childHasReturn = false; // 이 스코프의 child가 return을 갖고 있는가
    bool handleReturn = false;   // 이 스코프에서 return을 처리했다. 더이상 명령어가 나오면 안된다

    // "a_16" -> slotIndex
    std::unordered_map<RName, QLocalInfo> localInfos;
    std::vector<size_t> slotIndices; // 이 스코프가 관리하는 slot

    // 최근 return용 cleanUp블록
    size_t coveredSlots = 0; // 어느 슬롯까지 커버했는지 count, slots의 인덱스이다 [0, slots.size())
    QBlock* recentCleanUpForReturn = nullptr; // return시 정리 블록
};

struct QIntrinsicResultType_Slot { RType* type; };
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
    std::optional<size_t> o_retSlotIndex; // 함수의 반환값 slot

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
    void EmitIntrinsic(QInst_IntrinsicKind kind, std::optional<QArg_Slot> o_dest, std::vector<QArg_Input>&& args);
    void EmitTermInst(QTermInst&& termInst);
    
private:
    QBlock* MakeCleanUpForReturnBlock(size_t scopeIndex);
public:
    void EmitJumpToCleanUpForReturnBlock();

public:
    size_t GetTypeSize(RType* type);
    
    RType* GetStringType();
    RType* GetBoolType();
    RType* GetIntType();
    RType* GetPtrType();
    RType* GetPtrType(RType* innerType);

    size_t GetRetSlotIndex();

    std::optional<QLocalInfo> GetLocalInfo(const RName& name);
    size_t AddLocalVar(RType* type, const RName& name, std::optional<size_t> o_argIndex);

    void AddLocalRef_Alias(const RName& rName, size_t slotIndex);
    void AddLocalRef_Ptr(RType* rType, const RName& rName, size_t slotIndex);

    size_t NewSlot(RType* rType, std::optional<size_t> o_argIndex = std::nullopt);
    std::span<QSlotInfo> GetStackSlotInfos() { return slotInfos; }
    std::span<QBlock*> GetBlocks() { return blocks; }
    RType* GetSlotType(size_t i) { return slotInfos[i].type; }
    
    void VerifyBlocks();

    bool IsVoidType(RType* rType);

    void PushScope();
    void PopScope();
    void CleanUpScope();

    void MarkReturnHandledOnCurScope();
    bool IsReturnHandledOnCurScope();
    
};


} // namespace Citron