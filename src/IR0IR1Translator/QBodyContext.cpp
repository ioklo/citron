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

#include "QLazyBlock.h"
#include "MqIntrinsicInfo.h"
#include "QAbi_Citron_X64.h"

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

    auto* firstBlock = AddBlock("entry");
    this->curBlock = firstBlock;
}

QBlock* QBodyContext::AddBlock(std::string&& debugText)
{
    auto* newBlock = qFactory->MakeQBlock(format("b{}_{}", blocks.size(), move(debugText)));
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

QBlock* QBodyContext::GetLeaveBlock(size_t labelId)
{
    for (auto& info : jumpBlockInfos | views::reverse)
    {
        if (auto* leaveInfo = get_if<QJumpBlockInfo_Inline>(&info))
            if (leaveInfo->labelId == labelId)
                return leaveInfo->lazyLeaveBlock->GetBlock(this);
    }

    return nullptr;
}


void QBodyContext::EmitInstInternal(QInst&& inst)
{
    assert(curBlock);
    curBlock->EmitInst(std::move(inst));
}

void QBodyContext::EmitIntrinsic(QInst_IntrinsicKind kind, optional<QArg_Dest> o_dest, std::vector<QArg_CallArg>&& args)
{
    assert(curBlock);
    curBlock->EmitInst(QInst_Intrinsic{kind, move(o_dest), move(args)});
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
        if constexpr (same_as<T, QCleanUpKind_Return>)
            return scopeIndex == 0;
        else if constexpr (same_as<T, QCleanUpKind_Continue>)
            return scopes[scopeIndex].o_labelId == kind.labelId;
        else if constexpr (same_as<T, QCleanUpKind_Break>)
            return scopes[scopeIndex].o_labelId == kind.labelId;
        else if constexpr (same_as<T, QCleanUpKind_Leave>)
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
            newBlock->EmitInst(QInst_Intrinsic{QInst_IntrinsicKind::Dtor_Void_StringRef, nullopt, {QArg_CallArg_AddrOfSlot{slotIndex}}});
        }
    }

    return newBlock;
}

void QBodyContext::FinalizeCleanupBlock(QBlock* block, QCleanUpKind kind)
{
    visit([this, block](auto& kind) {
        using T = remove_cvref_t<decltype(kind)>;
        if constexpr (same_as<T, QCleanUpKind_Return>)
        {
            // 바로 리턴 블록 생성
            block->EmitInst(QInst_Return{});
        }
        else if constexpr (same_as<T, QCleanUpKind_Continue>)
        {   
            auto* contBlock = GetContinueBlock(kind.labelId);
            block->EmitInst(QInst_Jump{contBlock});
        }
        else if constexpr (same_as<T, QCleanUpKind_Break>)
        {   
            auto* breakBlock = GetBreakBlock(kind.labelId);
            block->EmitInst(QInst_Jump{breakBlock});
        }
        else if constexpr (same_as<T, QCleanUpKind_Leave>)
        {
            auto* leaveBlock = GetLeaveBlock(kind.labelId);
            block->EmitInst(QInst_Jump{leaveBlock});
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

optional<size_t> QBodyContext::GetLeaveSlotIndex(size_t labelId)
{
    for (auto& jumpBlockInfo : jumpBlockInfos | views::reverse)
    {
        if (auto* inlineInfo = get_if<QJumpBlockInfo_Inline>(&jumpBlockInfo))
        {
            if (inlineInfo->labelId == labelId)
                return inlineInfo->leaveSlotIndex;
        }
    }

    return nullopt;
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

size_t QBodyContext::AddLocalVar(RType* type, const RName& rName)
{   
    size_t slotIndex = slotInfos.size(); // 여기서의 index는 모든 named 변수의 index (local vars가 어디 들어있는지는 별개)

    // 1. 함수 entry에서 할당할 목록에 추가
    slotInfos.emplace_back(type, slotIndex, QSlotRole_Local{rName});
    
    // 2. 현재 스코프에 이름 추가
    curScope->localInfos[rName] = QLocalInfo_Var{slotIndex, rName};

    // 3. managedSlot에 추가
    if (type->GetCopyStrategy() == RCopyStrategy::NonBitwise)
        curScope->managedSlotIndices.push_back(slotIndex);

    return slotIndex;
}

size_t QBodyContext::AddArgument(RType* type, const RName& rName, size_t index)
{
    size_t slotIndex = slotInfos.size(); // 여기서의 index는 모든 named 변수의 index (local vars가 어디 들어있는지는 별개)

    // 1. 함수 entry에서 할당할 목록에 추가
    slotInfos.emplace_back(type, slotIndex, QSlotRole_Argument{rName, index});

    // 2. 현재 스코프에 이름 추가
    curScope->localInfos[rName] = QLocalInfo_Var{slotIndex, rName};
    if (type->GetCopyStrategy() == RCopyStrategy::NonBitwise)
        curScope->managedSlotIndices.push_back(slotIndex);

    return slotIndex;
}

// ptr을 갖고 있게 된다
void QBodyContext::AddRefArgument(RType* type, const RName& rName, size_t index)
{
    size_t slotIndex = slotInfos.size(); // 여기서의 index는 모든 named 변수의 index (local vars가 어디 들어있는지는 별개)

    auto* ptrType = GetPtrType(type);

    // 1. 함수 entry에서 할당할 목록에 추가
    slotInfos.emplace_back(ptrType, slotIndex, QSlotRole_Argument{rName, index});

    // 2. 현재 스코프에 이름 추가
    curScope->localInfos[rName] = QLocalInfo_RefPtr{slotIndex, rName};

    // 3. managedSlot에 추가하지 않는다
}

void QBodyContext::AddLocalRef_Alias(const RName& rName, size_t slotIndex)
{
    curScope->localInfos[rName] = QLocalInfo_RefAlias{slotIndex, rName};
    // 레퍼런스는 수명을 관리하지 않기 때문에 slotIndices에 추가하지 않는다
}

void QBodyContext::AddLocalRef_Ptr(RType* rType, const RName& rName, size_t slotIndex)
{   
    curScope->localInfos[rName] = QLocalInfo_RefPtr{slotIndex, rName, rType};
}

size_t QBodyContext::AddParameter(RType* type, QAbi* abi)
{
    size_t slotIndex = slotInfos.size();

    // 1. 함수 entry에서 할당할 목록에 추가
    slotInfos.emplace_back(type, slotIndex, QSlotRole_Parameter{});

    // 2. 스코프에 이름은 추가하지 않고

    // 3. managedSlot에 추가하지 않기 (Citron X64 동작)
    assert(dynamic_cast<QAbi_Citron_X64*>(abi));
    // x64에서는 dtor를 callee가 호출한다

    return slotIndex;
}

size_t QBodyContext::AddTemp(RType* type, std::string&& debugText)
{
    size_t slotIndex = slotInfos.size();

    // 1. 함수 entry에서 할당할 목록에 추가
    slotInfos.emplace_back(type, slotIndex, QSlotRole_Temp{move(debugText)});

    // 2. 스코프에 이름은 추가하지 않고

    // 3. managedSlot에 추가
    if (type->GetCopyStrategy() == RCopyStrategy::NonBitwise)
        curScope->managedSlotIndices.push_back(slotIndex);

    return slotIndex;
}

size_t QBodyContext::AddThis(RType* type)
{
    size_t slotIndex = slotInfos.size();
    slotInfos.emplace_back(type, slotIndex, QSlotRole_This{});
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
    scopes.pop_back();
    curScope = (!scopes.empty()) ? &scopes.back() : nullptr;
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
        // 소멸자 호출
        // TODO: HARD CODED

        assert(curBlock);
        curBlock->EmitInst(QInst_Intrinsic{QInst_IntrinsicKind::Dtor_Void_StringRef, nullopt, {QArg_CallArg_AddrOfSlot{slotIndex}}});
    }
}

} // Citron
