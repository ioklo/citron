#include "QBodyContext.h"

#include <variant>
#include <format>
#include <ranges>
#include <unordered_set>

#include "Infra/Unreachable.h"
#include "Infra/Exceptions.h"
#include "Infra/Variants.h"
#include "Infra/Ptr.h"

#include "Logging/Diag.h"

#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RFuncDecl.h"

#include "MIR/MExp.h"

#include "QIR/QInsts.h"
#include "QIR/QArgs.h"
#include "QIR/QFactory.h"

using namespace std;

namespace Citron {

namespace {

inline bool IsTerminator(QInst& inst)
{
    return holds_alternative<QInst_Jump>(inst)
        || holds_alternative<QInst_CondJump>(inst)
        || holds_alternative<QInst_Return>(inst);
}
}

QCleanUpInfo& QScope::GetOrAddCleanUpInfo(QCleanUpKind kind)
{
    if (auto* value = cleanUpInfos.Find(kind))
        return *value;

    return cleanUpInfos.Add(kind, QCleanUpInfo{});
}

QJumpBlockScopeGuard::QJumpBlockScopeGuard(QJumpBlockInfo&& info, QBodyContext& bodyContext)
    : bodyContext{bodyContext}
{
    bodyContext.PushJumpBlockInfo(std::move(info));
}

QJumpBlockScopeGuard::~QJumpBlockScopeGuard()
{
    bodyContext.PopJumpBlockInfo();
}

QBodyContext::QBodyContext(const RFactoryPtr& rFactory, const QFactoryPtr& qFactory, RType* rRetType)
    : rFactory{rFactory}
    , qFactory{qFactory}
{
    scopes.emplace_back();
    curScope = &scopes.back();
    
    if (rRetType != rFactory->MakeVoidType())
        o_retSlotIndex = NewSlot(rRetType);

    auto* firstBlock = AddBlock("entry");
    this->curBlock = firstBlock;
}

// GetIntrinsicResultType
// 1. result = intrinsic kind, args, ...
// 2. intrinsic kind, &result, args, ...
// 3. intrinsic kind, args, ... (void)
QIntrinsicResultType QBodyContext::GetIntrinsicResultType(QInst_IntrinsicKind kind)
{
    switch (kind)
    {
    case QInst_IntrinsicKind::Command_Items: 
        return QIntrinsicKindResult_Void{};

    case QInst_IntrinsicKind::Alloc_Int: throw NotImplementedException{};

    case QInst_IntrinsicKind::Memcpy_Ptr_Ptr_Int:
        return QIntrinsicKindResult_Void{};

    case QInst_IntrinsicKind::NewList_Items: throw NotImplementedException{};
    case QInst_IntrinsicKind::GetIterator_ListPtr_ListIterator: throw NotImplementedException{};

    case QInst_IntrinsicKind::LogicalNot_Bool_Bool:
        return QIntrinsicResultType_Slot{rFactory->MakeBoolType()};

    case QInst_IntrinsicKind::UnaryMinus_Int_Int:
        return QIntrinsicResultType_Slot{rFactory->MakeIntType()};

    case QInst_IntrinsicKind::ToString_Bool_String:
    case QInst_IntrinsicKind::ToString_Int_String: 
    case QInst_IntrinsicKind::Add_StringPtr_StringPtr_String:
        return QIntrinsicResultType_Slot{rFactory->MakeStringType()};

    case QInst_IntrinsicKind::PrefixInc_Int_Int: 
    case QInst_IntrinsicKind::PrefixDec_Int_Int:
    case QInst_IntrinsicKind::PostfixInc_Int_Int:
    case QInst_IntrinsicKind::PostfixDec_Int_Int:

    case QInst_IntrinsicKind::Multiply_Int_Int_Int:
    case QInst_IntrinsicKind::Divide_Int_Int_Int:
    case QInst_IntrinsicKind::Modulo_Int_Int_Int:
    case QInst_IntrinsicKind::Add_Int_Int_Int:
    case QInst_IntrinsicKind::Subtract_Int_Int_Int:
        return QIntrinsicResultType_Slot{rFactory->MakeIntType()};
    
    case QInst_IntrinsicKind::LessThan_Int_Int_Bool:
    case QInst_IntrinsicKind::LessThan_StringPtr_StringPtr_Bool:
    case QInst_IntrinsicKind::GreaterThan_Int_Int_Bool:
    case QInst_IntrinsicKind::GreaterThan_StringPtr_StringPtr_Bool:
    case QInst_IntrinsicKind::LessThanOrEqual_Int_Int_Bool:
    case QInst_IntrinsicKind::LessThanOrEqual_StringPtr_StringPtr_Bool:
    case QInst_IntrinsicKind::GreaterThanOrEqual_Int_Int_Bool:
    case QInst_IntrinsicKind::GreaterThanOrEqual_StringPtr_StringPtr_Bool:
    case QInst_IntrinsicKind::Equal_Int_Int_Bool:
    case QInst_IntrinsicKind::Equal_Bool_Bool_Bool:
    case QInst_IntrinsicKind::Equal_StringPtr_StringPtr_Bool:
        return QIntrinsicResultType_Slot{rFactory->MakeBoolType()};

    case QInst_IntrinsicKind::CopyCtor_StringPtr_StringPtr_Void:
    case QInst_IntrinsicKind::MoveCtor_StringPtr_StringPtr_Void:
    case QInst_IntrinsicKind::Dtor_StringPtr_Void:
    case QInst_IntrinsicKind::CopyAssign_StringPtr_StringPtr_Void:
    case QInst_IntrinsicKind::MoveAssign_StringPtr_StringPtr_Void:
        return QIntrinsicResultType_Slot{rFactory->MakeVoidType()};
    }

    throw NotImplementedException{};
}

QBlock* QBodyContext::AddBlock(std::string&& debugText)
{
    auto* newBlock = qFactory->MakeQBlock(blocks.size(), format("b{}_{}", blocks.size(), move(debugText)));
    blocks.push_back(newBlock);
    return newBlock;
}

QBlock* QBodyContext::GetContinueBlock(size_t labelId)
{
    for(auto& info : jumpBlockInfos | views::reverse)
    {
        if (auto* loopInfo = get_if<QJumpBlockInfo_Loop>(&info))
            if (loopInfo->labelId == labelId)
                return loopInfo->contBlock;
    }

    return nullptr;
}

QBlock* QBodyContext::GetBreakBlock(size_t labelId)
{
    for (auto& info : jumpBlockInfos | views::reverse)
    {
        if (auto* loopInfo = get_if<QJumpBlockInfo_Loop>(&info))
        {
            if (loopInfo->labelId == labelId)
                return loopInfo->breakBlock;
        }
        else if (auto* switchInfo = get_if<QJumpBlockInfo_Switch>(&info))
        {
            if (switchInfo->labelId == labelId)
                return loopInfo->breakBlock;
        }
    }

    return nullptr;
}

void QBodyContext::EmitInstInternal(QInst&& inst)
{
    assert(curBlock);

    curBlock->EmitInst(std::move(inst));
}

void QBodyContext::EmitIntrinsic(QInst_IntrinsicKind kind, optional<QArg_Slot> o_dest, std::vector<QArg_Input>&& args)
{
    auto resultType = GetIntrinsicResultType(kind);
    visit([this, kind, &o_dest, &args](auto& resultType) {
        using T = remove_cvref_t<decltype(resultType)>;

        if constexpr (same_as<T, QIntrinsicResultType_Slot>)
        {
            QArg_Slot resultSlot = o_dest ? *o_dest : QArg_Slot{NewSlot(resultType.type)};

            assert(curBlock);
            curBlock->EmitInst(QInst_Intrinsic{kind, resultSlot, move(args)});
        }
        else if constexpr (same_as<T, QIntrinsicKindResult_Void>)
        {
            assert(!o_dest);
            assert(curBlock);
            curBlock->EmitInst(QInst_Intrinsic{kind, nullopt, move(args)});
        }
        else static_assert(false);
        
    }, resultType);
}

void QBodyContext::EmitTermInst(QTermInst&& termInst)
{
    assert(curBlock);

    curBlock->EmitInst(Cast<QInst>(termInst));
    curBlock = nullptr;
}

bool QBodyContext::IsFinalScope(QCleanUpKind kind, size_t scopeIndex)
{
    return visit([this, scopeIndex](auto& kind) -> bool {
        using T = remove_cvref_t<decltype(kind)>;
        if constexpr (same_as<T, QCleanUpInfoKey_Return>)
            return scopeIndex == 0;
        else if constexpr (same_as<T, QCleanUpInfoKey_Continue>)
            return scopes[scopeIndex].o_labelId == kind.labelId;
        else if constexpr (same_as<T, QCleanUpInfoKey_Break>)
            return scopes[scopeIndex].o_labelId == kind.labelId;
        else static_assert(false);

    }, kind);
}

QBlock* QBodyContext::MakeCleanUpBlockWithoutFinalize(span<size_t> managedSlotIndices)
{
    assert(!managedSlotIndices.empty());

    QBlock* newBlock = AddBlock("cleanUp");
    RType* stringType = GetStringType();
    RType* ptrStringType = GetPtrType(stringType);

    for (auto& slotIndex : managedSlotIndices)
    {   
        if (slotInfos[slotIndex].type == stringType)
        {
            auto ptrSlotIndex = NewTempSlot(ptrStringType);
            newBlock->EmitInst(QInst_AddrOf{QArg_Slot{ptrSlotIndex}, QArg_Slot{slotIndex}});
            newBlock->EmitInst(QInst_Intrinsic{QInst_IntrinsicKind::Dtor_StringPtr_Void, nullopt, {QArg_Slot{ptrSlotIndex}}});
        }
    }

    return newBlock;
}

void QBodyContext::FinalizeCleanupBlock(QBlock* block, QCleanUpKind kind)
{
    visit([this, block](auto& kind) {
        using T = remove_cvref_t<decltype(kind)>;
        if constexpr (same_as<T, QCleanUpInfoKey_Return>)
        {
            // 바로 리턴 블록 생성
            if (o_retSlotIndex)
                block->EmitInst(QInst_Return{QInst_ReturnValue{slotInfos[*o_retSlotIndex].type, QArg_Slot{*o_retSlotIndex}}});
            else
                block->EmitInst(QInst_Return{});
        }
        else if constexpr (same_as<T, QCleanUpInfoKey_Continue>)
        {   
            auto* contBlock = GetContinueBlock(kind.labelId);
            block->EmitInst(QInst_Jump{contBlock});
        }
        else if constexpr (same_as<T, QCleanUpInfoKey_Break>)
        {   
            auto* breakBlock = GetBreakBlock(kind.labelId);
            block->EmitInst(QInst_Jump{breakBlock});
        }
        else static_assert(false);
    }, kind);
}

// FinalScope를 먼저 보지 않는 버전
QBlock* QBodyContext::GetCleanUpBlock(QCleanUpKind kind, size_t scopeIndex)
{
    QScope& scope = scopes[scopeIndex];
    auto* cleanUpInfo = scope.cleanUpInfos.Find(kind);

    if (!cleanUpInfo)
    {
        if (IsFinalScope(kind, scopeIndex))
        {
            if (!scope.managedSlotIndices.empty())
            {
                QBlock* block = MakeCleanUpBlockWithoutFinalize(scope.managedSlotIndices);
                FinalizeCleanupBlock(block, kind);
                scope.cleanUpInfos.Add(kind, QCleanUpInfo{scope.managedSlotIndices.size(), block});
                return block;
            }
            else
            {
                QBlock* block = AddBlock("cleanUp");
                FinalizeCleanupBlock(block, kind);
                scope.cleanUpInfos.Add(kind, QCleanUpInfo{scope.managedSlotIndices.size(), block});
                return block;
            }
        }
        else
        {
            if (!scope.managedSlotIndices.empty())
            {
                QBlock* block = MakeCleanUpBlockWithoutFinalize(scope.managedSlotIndices);
                auto* parentCleanUpBlock = GetCleanUpBlock(kind, scopeIndex - 1);
                block->EmitInst(QInst_Jump{parentCleanUpBlock});
                scope.cleanUpInfos.Add(kind, QCleanUpInfo{scope.managedSlotIndices.size(), block});
                return block;
            }
            else
            {
                auto* parentCleanUpBlock = GetCleanUpBlock(kind, scopeIndex - 1);
                scope.cleanUpInfos.Add(kind, QCleanUpInfo{scope.managedSlotIndices.size(), parentCleanUpBlock});
                return parentCleanUpBlock;
            }
        }
    }
    else
    {
        if (cleanUpInfo->managedCoveredSlots == scope.managedSlotIndices.size())
        {
            return cleanUpInfo->recentCleanUpBlock;
        }
        else
        {
            span<size_t> managedSlotIndices{
                scope.managedSlotIndices.data() + cleanUpInfo->managedCoveredSlots, 
                scope.managedSlotIndices.size() - cleanUpInfo->managedCoveredSlots
            };

            QBlock* block = MakeCleanUpBlockWithoutFinalize(managedSlotIndices);
            block->EmitInst(QInst_Jump{cleanUpInfo->recentCleanUpBlock});
            cleanUpInfo->recentCleanUpBlock = block;
            cleanUpInfo->managedCoveredSlots = scope.managedSlotIndices.size();
            return block;
        }
    }
}

void QBodyContext::EmitJumpToCleanUpBlock(QCleanUpKind kind)
{
    assert(curBlock);

    auto* cleanUpBlock = GetCleanUpBlock(kind, scopes.size() - 1);
    curBlock->EmitInst(QInst_Jump{cleanUpBlock});
    curBlock = nullptr;
}

size_t QBodyContext::GetTypeSize(RType* type)
{
    // TODO: HARD CODED
    if (type == rFactory->MakeBoolType())
        return 1;

    if (type == rFactory->MakeIntType())
        return 4;

    if (type == rFactory->MakeStringType())
        return sizeof(string);

    throw NotImplementedException{};
}

RType* QBodyContext::GetStringType()
{
    return rFactory->MakeStringType();
}

RType* QBodyContext::GetBoolType()
{
    return rFactory->MakeBoolType();
}

RType* QBodyContext::GetIntType()
{
    return rFactory->MakeIntType();
}

RType* QBodyContext::GetPtrType()
{
    return rFactory->MakePtrType(rFactory->MakeVoidType());
}

RType* QBodyContext::GetPtrType(RType* innerType)
{
    return rFactory->MakePtrType(innerType);
}

size_t QBodyContext::GetRetSlotIndex()
{
    return *o_retSlotIndex;
}

optional<QLocalInfo> QBodyContext::GetLocalInfo(const RName& name)
{
    for (auto& scope : scopes | views::reverse)
    {
        auto i = scope.localInfos.find(name);
        if (i != scope.localInfos.end())
            return i->second;
    }

    return nullopt;
}

size_t QBodyContext::AddLocalVar(RType* rType, const RName& rName, optional<size_t> o_argIndex)
{   
    size_t slotIndex = slotInfos.size(); // 여기서의 index는 모든 named 변수의 index (local vars가 어디 들어있는지는 별개)
    auto name = format("%s{}_{}", slotIndex, RNameToString(rName));

    // 1. 함수 entry에서 할당할 목록에 추가
    slotInfos.emplace_back(rType, name, o_argIndex);
    
    // 2. 현재 스코프에 이름 추가
    curScope->localInfos[rName] = QLocalInfo_Var{slotIndex, name};
    curScope->managedSlotIndices.push_back(slotIndex);

    return slotIndex;
}

void QBodyContext::AddLocalRef_Alias(const RName& rName, size_t slotIndex)
{
    curScope->localInfos[rName] = QLocalInfo_RefAlias{slotIndex, RNameToString(rName)};
    // 레퍼런스는 수명을 관리하지 않기 때문에 slotIndices에 추가하지 않는다
}

void QBodyContext::AddLocalRef_Ptr(RType* rType, const RName& rName, size_t slotIndex)
{   
    curScope->localInfos[rName] = QLocalInfo_RefPtr{slotIndex, RNameToString(rName), rType};
}

size_t QBodyContext::NewSlot(RType* rType, optional<size_t> o_argIndex)
{
    size_t slotIndex = slotInfos.size();
    std::string s = format("%s{}", slotIndex);
    slotInfos.emplace_back(rType, s, o_argIndex);

    if (rType->GetCopyStrategy() == RCopyStrategy::NonBitwise)
        curScope->managedSlotIndices.push_back(slotIndex);

    return slotIndex;
}

// 어느 scope에도 속하지 않는 임시 슬롯
size_t QBodyContext::NewTempSlot(RType* rType)
{
    // 임시 슬롯은 BC타입이어야 한다
    assert(rType->GetCopyStrategy() == RCopyStrategy::Bitwise); 

    size_t slotIndex = slotInfos.size();
    std::string s = format("%s{}", slotIndex);
    slotInfos.emplace_back(rType, s, nullopt);

    return slotIndex;
}

void QBodyContext::VerifyBlocks()
{
    // blocks의 모든 block에 대해서
    // 1. 모두 terminator로 끝나는지, block들이 비어있진 않은지 (terminator로 끝나면 비진 않았으니)
    // 2. terminator가 여러번 들어갔는지
    // 3. 그 entry부터 block으로 가는 path가 있는지 => 그래프 순회

    // 1, 2 검사
    for (auto* block : blocks)
    {
        auto insts = block->GetInsts();
        assert(!insts.empty());

        for (size_t i = 0, count = insts.size(); i < count; i++)
        {
            bool bLast = (i == count - 1);
            bool bTerminator = IsTerminator(insts[i]);

            // bLast: true, bTerminator: true
            // bLast: false, bTerminator: false
            assert(bLast == bTerminator);
        }
    }

    // 3 검사
    unordered_set<QBlock*> visited; // 큐잉을 포함해서, 한번 도달했는지
    visited.reserve(blocks.size());

    vector<QBlock*> stack;
    stack.reserve(blocks.size()); // 이렇게 크게 갈리가 없는데

    // front부터 시작해서 도달했는지 검사
    visited.insert(blocks.front());
    stack.push_back(blocks.front());

    while (!stack.empty())
    {
        auto* block = stack.back();
        stack.pop_back();

        auto& termInst = block->GetInsts().back();
        visit([&visited, &stack](auto& termInst)
        {
            using T = remove_cvref_t<decltype(termInst)>;

            if constexpr (same_as<T, QInst_Jump>)
            {
                if (visited.find(termInst.block) == visited.end())
                {
                    visited.insert(termInst.block);
                    stack.push_back(termInst.block);
                }
            }
            else if constexpr (same_as<T, QInst_CondJump>)
            {
                if (visited.find(termInst.trueBlock) == visited.end())
                {
                    visited.insert(termInst.trueBlock);
                    stack.push_back(termInst.trueBlock);
                }

                if (visited.find(termInst.falseBlock) == visited.end())
                {
                    visited.insert(termInst.falseBlock);
                    stack.push_back(termInst.falseBlock);
                }
            }
            else if constexpr (same_as<T, QInst_Return>)
            {
                // no exit
            }
            else
            {
                assert(false);
            }

        }, termInst);
    }

    assert(visited.size() == blocks.size()); // 모두 도달했어야
}

bool QBodyContext::IsVoidType(RType* rType)
{
    return rType == rFactory->MakeVoidType();
}

void QBodyContext::PushScope(std::optional<size_t> o_labelId)
{
    scopes.push_back(QScope{.o_labelId = o_labelId});
    curScope = &scopes.back();
}

void QBodyContext::PopScope()
{
    bool childHasReturn = scopes.back().childHasReturn;
    scopes.pop_back();

    curScope = (!scopes.empty()) ? &scopes.back() : nullptr;

    if (curScope)
        curScope->childHasReturn |= childHasReturn;
}

// 일반적인 CleanUp
void QBodyContext::CleanUpScope()
{
    vector<size_t> slotsNeedingDtor;

    // 순서는 거꾸로
    for (auto slotIndex : curScope->managedSlotIndices | views::reverse)
    {   
        if (slotInfos[slotIndex].type == rFactory->MakeStringType())
            slotsNeedingDtor.push_back(slotIndex);
    }

    auto* stringType = GetStringType();
    auto* stringPtrType = GetPtrType(stringType);

    for (auto slotIndex : slotsNeedingDtor)
    {
        auto ptrSlotIndex = NewTempSlot(stringPtrType);

        // 소멸자 호출
        // TODO: HARD CODED

        assert(curBlock);
        curBlock->EmitInst(QInst_AddrOf{QArg_Slot{ptrSlotIndex}, QArg_Slot{slotIndex}});
        curBlock->EmitInst(QInst_Intrinsic{QInst_IntrinsicKind::Dtor_StringPtr_Void, nullopt, {QArg_Slot{ptrSlotIndex}}});
    }
}

void QBodyContext::MarkReturnHandledOnCurScope()
{
    assert(!curScope->handleReturn);
    curScope->handleReturn = true;
}

bool QBodyContext::IsReturnHandledOnCurScope()
{
    return curScope->handleReturn;
}

} // Citron
