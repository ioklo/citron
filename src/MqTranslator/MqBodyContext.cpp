#include "MqBodyContext.h"

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

#include "MqLazyBlock.h"
#include "MqIntrinsicInfo.h"
#include "MqAbi_Citron_X64.h"

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

MqCleanUpInfo& MqScope::GetOrAddCleanUpInfo(MqCleanUpKind kind)
{
    if (auto* value = cleanUpInfos.Find(kind))
        return *value;

    return cleanUpInfos.Add(kind, MqCleanUpInfo{});
}

MqJumpBlockScopeGuard::MqJumpBlockScopeGuard(MqJumpBlockInfo&& info, MqBodyContext& bodyContext)
    : bodyContext{bodyContext}
{
    bodyContext.PushJumpBlockInfo(std::move(info));
}

MqJumpBlockScopeGuard::~MqJumpBlockScopeGuard()
{
    bodyContext.PopJumpBlockInfo();
}

MqBodyContext::MqBodyContext(TakeRef<RFactoryPtr> rFactory, TakeRef<QFactoryPtr> qFactory, RType* rRetType)
    : rFactory{rFactory.Take()}
    , qFactory{qFactory.Take()}
{
    scopes.emplace_back();
    curScope = &scopes.back();

    auto* firstBlock = AddBlock("entry");
    this->curBlock = firstBlock;
}

QBlock* MqBodyContext::AddBlock(std::string&& debugText)
{
    auto* newBlock = qFactory->MakeQBlock(format("b{}_{}", blocks.size(), move(debugText)));
    blocks.push_back(newBlock);
    return newBlock;
}

QBlock* MqBodyContext::GetContinueBlock(size_t labelId)
{
    for(auto& info : jumpBlockInfos | views::reverse)
    {
        if (auto* loopInfo = get_if<MqJumpBlockInfo_Loop>(&info))
            if (loopInfo->labelId == labelId)
                return loopInfo->contBlock;
    }

    return nullptr;
}

QBlock* MqBodyContext::GetBreakBlock(size_t labelId)
{
    for (auto& info : jumpBlockInfos | views::reverse)
    {
        if (auto* loopInfo = get_if<MqJumpBlockInfo_Loop>(&info))
        {
            if (loopInfo->labelId == labelId)
                return loopInfo->breakBlock;
        }
        else if (auto* switchInfo = get_if<MqJumpBlockInfo_Switch>(&info))
        {
            if (switchInfo->labelId == labelId)
                return loopInfo->breakBlock;
        }
    }

    return nullptr;
}

QBlock* MqBodyContext::GetLeaveBlock(size_t labelId)
{
    for (auto& info : jumpBlockInfos | views::reverse)
    {
        if (auto* leaveInfo = get_if<MqJumpBlockInfo_Inline>(&info))
            if (leaveInfo->labelId == labelId)
                return leaveInfo->lazyLeaveBlock->GetBlock(this);
    }

    return nullptr;
}


void MqBodyContext::EmitInstInternal(QInst&& inst)
{
    assert(curBlock);
    curBlock->EmitInst(std::move(inst));
}

void MqBodyContext::EmitIntrinsic(QInst_IntrinsicKind kind, optional<QArg_Dest> o_dest, std::vector<QArg_CallArg>&& args)
{
    assert(curBlock);
    curBlock->EmitInst(QInst_Intrinsic{kind, move(o_dest), move(args)});
}

void MqBodyContext::EmitTermInst(QTermInst&& termInst)
{
    assert(curBlock);

    curBlock->EmitInst(Cast<QInst>(termInst));
    curBlock = nullptr;
}

bool MqBodyContext::IsFinalScope(MqCleanUpKind kind, size_t scopeIndex)
{
    return visit([this, scopeIndex](auto& kind) -> bool {
        using T = remove_cvref_t<decltype(kind)>;
        if constexpr (same_as<T, MqCleanUpKind_Return>)
            return scopeIndex == 0;
        else if constexpr (same_as<T, MqCleanUpKind_Continue>)
            return scopes[scopeIndex].o_labelId == kind.labelId;
        else if constexpr (same_as<T, MqCleanUpKind_Break>)
            return scopes[scopeIndex].o_labelId == kind.labelId;
        else if constexpr (same_as<T, MqCleanUpKind_Leave>)
            return scopes[scopeIndex].o_labelId == kind.labelId;
        else static_assert(false);

    }, kind);
}

QBlock* MqBodyContext::MakeCleanUpBlockWithoutFinalize(span<size_t> managedSlotIndices)
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

void MqBodyContext::FinalizeCleanupBlock(QBlock* block, MqCleanUpKind kind)
{
    visit([this, block](auto& kind) {
        using T = remove_cvref_t<decltype(kind)>;
        if constexpr (same_as<T, MqCleanUpKind_Return>)
        {
            // 바로 리턴 블록 생성
            block->EmitInst(QInst_Return{});
        }
        else if constexpr (same_as<T, MqCleanUpKind_Continue>)
        {   
            auto* contBlock = GetContinueBlock(kind.labelId);
            block->EmitInst(QInst_Jump{contBlock});
        }
        else if constexpr (same_as<T, MqCleanUpKind_Break>)
        {   
            auto* breakBlock = GetBreakBlock(kind.labelId);
            block->EmitInst(QInst_Jump{breakBlock});
        }
        else if constexpr (same_as<T, MqCleanUpKind_Leave>)
        {
            auto* leaveBlock = GetLeaveBlock(kind.labelId);
            block->EmitInst(QInst_Jump{leaveBlock});
        }
        else static_assert(false);
    }, kind);
}

// FinalScope를 먼저 보지 않는 버전
QBlock* MqBodyContext::GetCleanUpBlock(MqCleanUpKind kind, size_t scopeIndex)
{
    MqScope& scope = scopes[scopeIndex];
    auto* cleanUpInfo = scope.cleanUpInfos.Find(kind);

    if (!cleanUpInfo)
    {
        if (IsFinalScope(kind, scopeIndex))
        {
            if (!scope.managedSlotIndices.empty())
            {
                QBlock* block = MakeCleanUpBlockWithoutFinalize(scope.managedSlotIndices);
                FinalizeCleanupBlock(block, kind);
                scope.cleanUpInfos.Add(kind, MqCleanUpInfo{scope.managedSlotIndices.size(), block});
                return block;
            }
            else
            {
                QBlock* block = AddBlock("cleanUp");
                FinalizeCleanupBlock(block, kind);
                scope.cleanUpInfos.Add(kind, MqCleanUpInfo{scope.managedSlotIndices.size(), block});
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
                scope.cleanUpInfos.Add(kind, MqCleanUpInfo{scope.managedSlotIndices.size(), block});
                return block;
            }
            else
            {
                auto* parentCleanUpBlock = GetCleanUpBlock(kind, scopeIndex - 1);
                scope.cleanUpInfos.Add(kind, MqCleanUpInfo{scope.managedSlotIndices.size(), parentCleanUpBlock});
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

void MqBodyContext::EmitJumpToCleanUpBlock(MqCleanUpKind kind)
{
    assert(curBlock);

    auto* cleanUpBlock = GetCleanUpBlock(kind, scopes.size() - 1);
    curBlock->EmitInst(QInst_Jump{cleanUpBlock});
    curBlock = nullptr;
}

RType* MqBodyContext::GetStringType()
{
    return rFactory->MakeStringType();
}

RType* MqBodyContext::GetBoolType()
{
    return rFactory->MakeBoolType();
}

RType* MqBodyContext::GetIntType()
{
    return rFactory->MakeIntType();
}

RType* MqBodyContext::GetPtrType()
{
    return rFactory->MakePtrType(rFactory->MakeVoidType());
}

RType* MqBodyContext::GetPtrType(RType* innerType)
{
    return rFactory->MakePtrType(innerType);
}

optional<size_t> MqBodyContext::GetLeaveSlotIndex(size_t labelId)
{
    for (auto& jumpBlockInfo : jumpBlockInfos | views::reverse)
    {
        if (auto* inlineInfo = get_if<MqJumpBlockInfo_Inline>(&jumpBlockInfo))
        {
            if (inlineInfo->labelId == labelId)
                return inlineInfo->leaveSlotIndex;
        }
    }

    return nullopt;
}

optional<MqLocalInfo> MqBodyContext::GetLocalInfo(InRef<RName> name)
{
    for (auto& scope : scopes | views::reverse)
    {
        auto i = scope.localInfos.find(*name);
        if (i != scope.localInfos.end())
            return i->second;
    }

    return nullopt;
}

size_t MqBodyContext::AddLocalVar(RType* type, InRef<RName> rName)
{   
    size_t slotIndex = slotInfos.size(); // 여기서의 index는 모든 named 변수의 index (local vars가 어디 들어있는지는 별개)

    // 1. 함수 entry에서 할당할 목록에 추가
    slotInfos.emplace_back(type, slotIndex, QSlotRole_Local{*rName});
    
    // 2. 현재 스코프에 이름 추가
    curScope->localInfos[*rName] = MqLocalInfo_Var{slotIndex, *rName};

    // 3. managedSlot에 추가
    if (type->GetCopyStrategy() == RCopyStrategy::NonBitwise)
        curScope->managedSlotIndices.push_back(slotIndex);

    return slotIndex;
}

size_t MqBodyContext::AddIndirectReturn(RType* type)
{
    size_t slotIndex = slotInfos.size();
    assert(slotIndex == 0);

    auto* ptrType = GetPtrType(type);

    // 1. 함수 entry에서 할당할 목록에 추가
    slotInfos.emplace_back(ptrType, slotIndex, QSlotRole_IndirectReturn{});

    // 2. 현재 스코프에 이름 추가 x

    // 3. managedSlot에 추가 x
    // indirect return은 caller에서 소멸자 처리한다

    return slotIndex;
}

size_t MqBodyContext::AddArgument_Direct(RType* type, InRef<RName> rName, size_t index)
{
    size_t slotIndex = slotInfos.size(); // 여기서의 index는 모든 named 변수의 index (local vars가 어디 들어있는지는 별개)

    // 1. 함수 entry에서 할당할 목록에 추가
    slotInfos.emplace_back(type, slotIndex, QSlotRole_Argument{*rName, index, QSlotRole_ArgumentKind::Direct});

    // 2. 현재 스코프에 이름 추가
    curScope->localInfos[*rName] = MqLocalInfo_Var{slotIndex, *rName};

    // 3. direct는 BC이므로 managedSlot에 추가하지 않는다
    assert(type->GetCopyStrategy() == RCopyStrategy::Bitwise);

    return slotIndex;
}

size_t MqBodyContext::AddArgument_Indirect(RType* type, InRef<RName> rName, size_t index, MqAbi* abi)
{
    size_t slotIndex = slotInfos.size(); // 여기서의 index는 모든 named 변수의 index (local vars가 어디 들어있는지는 별개)
    auto* ptrType = GetPtrType(type);

    // 1. 함수 entry에서 할당할 목록에 추가
    slotInfos.emplace_back(ptrType, slotIndex, QSlotRole_Argument{*rName, index, QSlotRole_ArgumentKind::Indirect});

    // 2. 현재 스코프에 이름 추가
    curScope->localInfos[*rName] = MqLocalInfo_RefPtr{slotIndex, *rName, type};

    // 3. managedSlot에 추가한다.
    // Citron_X64에서는 callee에서 해제하도록 managedSlot에 추가한다. 다른 ABI에서 다른 처리가 필요해지면 이 부분을 수정한다.
    assert(dynamic_cast<MqAbi_Citron_X64*>(abi)); 
    if (type->GetCopyStrategy() == RCopyStrategy::NonBitwise)
        curScope->managedSlotIndices.push_back(slotIndex);

    return slotIndex;
}

// ptr을 갖고 있게 된다
void MqBodyContext::AddArgument_Ref(RType* type, InRef<RName> rName, size_t index)
{
    size_t slotIndex = slotInfos.size(); // 여기서의 index는 모든 named 변수의 index (local vars가 어디 들어있는지는 별개)

    auto* ptrType = GetPtrType(type);

    // 1. 함수 entry에서 할당할 목록에 추가
    slotInfos.emplace_back(ptrType, slotIndex, QSlotRole_Argument{*rName, index});

    // 2. 현재 스코프에 이름 추가
    curScope->localInfos[*rName] = MqLocalInfo_RefPtr{slotIndex, *rName};

    // 3. managedSlot에 추가하지 않는다
}

void MqBodyContext::AddLocalRef_Alias(InRef<RName> rName, size_t slotIndex)
{
    curScope->localInfos[*rName] = MqLocalInfo_RefAlias{slotIndex, *rName};
    // 레퍼런스는 수명을 관리하지 않기 때문에 slotIndices에 추가하지 않는다
}

void MqBodyContext::AddLocalRef_Ptr(RType* rType, InRef<RName> rName, size_t slotIndex)
{   
    curScope->localInfos[*rName] = MqLocalInfo_RefPtr{slotIndex, *rName, rType};
}

size_t MqBodyContext::AddTemp(RType* type, std::string&& debugText)
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

size_t MqBodyContext::AddThis(RType* type)
{
    size_t slotIndex = slotInfos.size();
    slotInfos.emplace_back(type, slotIndex, QSlotRole_This{});
    return slotIndex;
}

void MqBodyContext::VerifyBlocks()
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

bool MqBodyContext::IsVoidType(RType* rType)
{
    return rType == rFactory->MakeVoidType();
}

void MqBodyContext::PushScope(std::optional<size_t> o_labelId)
{
    scopes.push_back(MqScope{.o_labelId = o_labelId});
    curScope = &scopes.back();
}

void MqBodyContext::PopScope()
{
    scopes.pop_back();
    curScope = (!scopes.empty()) ? &scopes.back() : nullptr;
}

// 일반적인 CleanUp
void MqBodyContext::CleanUpScope()
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
