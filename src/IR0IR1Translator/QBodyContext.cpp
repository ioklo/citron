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

QBodyContext::QBodyContext(const RFactoryPtr& rFactory, const QFactoryPtr& qFactory, RType* rRetType)
    : rFactory{rFactory}
    , qFactory{qFactory}
{
    scopes.emplace_back();
    curScope = &scopes.back();

    auto* qRetType = GetQTypeFromRType(rRetType);
    if (qRetType != qFactory->MakeVoidType())
        o_retSlotIndex = NewSlot(qRetType);

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
    case QInst_IntrinsicKind::GetListIterator_List: throw NotImplementedException{};

    case QInst_IntrinsicKind::LogicalNot_Bool:
        return QIntrinsicResultType_Slot{qFactory->MakeBoolType()};

    case QInst_IntrinsicKind::UnaryMinus_Int:
        return QIntrinsicResultType_Slot{qFactory->MakeIntType()};

    case QInst_IntrinsicKind::ToString_Bool:
    case QInst_IntrinsicKind::ToString_Int: 
    case QInst_IntrinsicKind::Add_String_String:
        return QIntrinsicResultType_Slot{qFactory->MakeStringType()};

    case QInst_IntrinsicKind::PrefixInc_Int: 
    case QInst_IntrinsicKind::PrefixDec_Int:
    case QInst_IntrinsicKind::PostfixInc_Int:
    case QInst_IntrinsicKind::PostfixDec_Int:

    case QInst_IntrinsicKind::Multiply_Int_Int:
    case QInst_IntrinsicKind::Divide_Int_Int:
    case QInst_IntrinsicKind::Modulo_Int_Int:
    case QInst_IntrinsicKind::Add_Int_Int:
    case QInst_IntrinsicKind::Subtract_Int_Int:
        return QIntrinsicResultType_Slot{qFactory->MakeIntType()};
    
    case QInst_IntrinsicKind::LessThan_Int_Int:
    case QInst_IntrinsicKind::LessThan_String_String:
    case QInst_IntrinsicKind::GreaterThan_Int_Int:
    case QInst_IntrinsicKind::GreaterThan_String_String:
    case QInst_IntrinsicKind::LessThanOrEqual_Int_Int:
    case QInst_IntrinsicKind::LessThanOrEqual_String_String:
    case QInst_IntrinsicKind::GreaterThanOrEqual_Int_Int:
    case QInst_IntrinsicKind::GreaterThanOrEqual_String_String:
    case QInst_IntrinsicKind::Equal_Int_Int:
    case QInst_IntrinsicKind::Equal_Bool_Bool:
    case QInst_IntrinsicKind::Equal_String_String:
        return QIntrinsicResultType_Slot{qFactory->MakeBoolType()};
    }

    throw NotImplementedException{};
}

QBlock* QBodyContext::AddBlock(std::string&& debugText)
{
    auto* newBlock = qFactory->MakeQBlock(blocks.size(), format("b{}_{}", blocks.size(), move(debugText)));
    blocks.push_back(newBlock);
    return newBlock;
}

expected<void, DiagPtr> QBodyContext::EmitInstInternal(QInst&& inst)
{
    if (!curBlock) return unexpected{MakePtr<Error_Unreachable>()};

    curBlock->EmitInst(std::move(inst));
    return {};
}

expected<void, DiagPtr> QBodyContext::EmitIntrinsic(QInst_IntrinsicKind kind, optional<QArg_Slot> oDest, std::vector<QArg_Input>&& args)
{
    auto resultType = GetIntrinsicResultType(kind);
    return visit([this, kind, &oDest, &args](auto& resultType) -> expected<void, DiagPtr>{
        using T = remove_cvref_t<decltype(resultType)>;

        if constexpr (same_as<T, QIntrinsicResultType_Slot>)
        {
            QArg_Slot resultSlot = oDest ? *oDest : QArg_Slot{NewSlot(resultType.qType)};

            if (!curBlock) return unexpected{MakePtr<Error_Unreachable>()};
            curBlock->EmitInst(QInst_Intrinsic{kind, resultSlot, move(args)});
            return {};
        }
        else if constexpr (same_as<T, QIntrinsicKindResult_Void>)
        {
            assert(!oDest);

            if (!curBlock) return unexpected{MakePtr<Error_Unreachable>()};
            curBlock->EmitInst(QInst_Intrinsic{kind, nullopt, move(args)});
            return {};
        }
        else static_assert(false);
        
    }, resultType);
}

std::expected<void, DiagPtr> QBodyContext::EmitTermInst(QTermInst&& termInst)
{
    if (!curBlock) return unexpected{MakePtr<Error_Unreachable>()};

    curBlock->EmitInst(Cast<QInst>(termInst));
    curBlock = nullptr;
    return {};
}

QBlock* QBodyContext::MakeCleanUpForReturnBlock(size_t scopeIndex)
{
    auto& scope = scopes[scopeIndex];

    // 지금 처리해야 할 slots의 갯수가 크다면, scope.recentCleanUpForReturn 업데이트
    if (scope.coveredSlots < scope.slotIndices.size())
    {
        // 1. cleanUp블록이 필요하지 않으면, 그냥 coveredSlots만 맞춰주고 리턴

        // TODO: String을 세는게 아니라, 각 타입의 소멸자가 있는지 검사
        QBlock* newCleanUpForRet = nullptr;
        for (size_t i = scope.coveredSlots, end = scope.slotIndices.size(); i < end; i++)
        {
            size_t slotIndex = scope.slotIndices[i];
            if (slotInfos[slotIndex].qType == GetStringQType())
            {
                if (newCleanUpForRet == nullptr)
                    newCleanUpForRet = AddBlock("cleanUpForRet"); // TODO: 뒤에 디버그용 번호 붙이기

                newCleanUpForRet->EmitInst(QInst_Dtor_String{QArg_Slot{slotIndex}});
            }
        }

        scope.coveredSlots = scope.slotIndices.size();

        if (newCleanUpForRet)
        {
            if (scope.recentCleanUpForReturn)
            {
                newCleanUpForRet->EmitInst(QInst_Jump{scope.recentCleanUpForReturn});
                scope.recentCleanUpForReturn = newCleanUpForRet;
            }
            else
            {
                if (scopeIndex != 0)
                {
                    auto* parentCleanUpForRet = MakeCleanUpForReturnBlock(scopeIndex - 1);
                    newCleanUpForRet->EmitInst(QInst_Jump{parentCleanUpForRet});
                    scope.recentCleanUpForReturn = newCleanUpForRet;
                }
                else // 0이면?
                {
                    if (o_retSlotIndex)
                        newCleanUpForRet->EmitInst(QInst_Return{QInst_ReturnValue{slotInfos[*o_retSlotIndex].qType, QArg_Slot{*o_retSlotIndex}}});
                    else
                        newCleanUpForRet->EmitInst(QInst_Return{});

                    scope.recentCleanUpForReturn = newCleanUpForRet;
                }
            }
        }
    }
    
    if (scope.recentCleanUpForReturn)
        return scope.recentCleanUpForReturn;

    if (scopeIndex != 0)
    {
        return MakeCleanUpForReturnBlock(scopeIndex - 1);
    }
    else // 0이면?
    {
        // 바로 리턴 블록 생성
        auto* retBlock = AddBlock("cleanUpForRet");
        if (o_retSlotIndex)
            retBlock->EmitInst(QInst_Return{QInst_ReturnValue{slotInfos[*o_retSlotIndex].qType, QArg_Slot{*o_retSlotIndex}}});
        else
            retBlock->EmitInst(QInst_Return{});
        scope.recentCleanUpForReturn = retBlock;
        return retBlock;
    }
}

expected<void, DiagPtr> QBodyContext::EmitJumpToCleanUpForReturnBlock()
{
    if (!curBlock) return unexpected{MakePtr<Error_Unreachable>()};

    auto* cleanUpForRetBlock = MakeCleanUpForReturnBlock(scopes.size() - 1);
    curBlock->EmitInst(QInst_Jump{cleanUpForRetBlock});
    curBlock = nullptr;

    return {};
}

QType* QBodyContext::GetMExpQType(MExp* mExp)
{
    return GetQTypeFromRType(mExp->GetType());
}

size_t QBodyContext::GetQTypeSize(QType* qType)
{
    // TODO: HARD CODED
    if (qType == qFactory->MakeBoolType())
        return 1;

    if (qType == qFactory->MakeIntType())
        return 4;

    if (qType == qFactory->MakeStringType())
        return sizeof(string);

    throw NotImplementedException{};
}

QType* QBodyContext::GetQTypeFromRType(RType* rType)
{
    // TODO: HARD CODED
    if (rType == rFactory->MakeVoidType())
        return qFactory->MakeVoidType();

    if (rType == rFactory->MakeBoolType())
        return qFactory->MakeBoolType();

    if (rType == rFactory->MakeIntType())
        return qFactory->MakeIntType();

    if (rType == rFactory->MakeStringType())
        return qFactory->MakeStringType();

    throw NotImplementedException{};
}

QType_Class* QBodyContext::GetStringQType()
{
    return qFactory->MakeStringType();
}

QType* QBodyContext::GetBoolQType()
{
    return qFactory->MakeBoolType();
}

QType* QBodyContext::GetIntQType()
{
    return qFactory->MakeIntType();
}

QType* QBodyContext::GetPtrQType()
{
    return qFactory->MakePtrType();
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

size_t QBodyContext::AddLocalVar(RType* rType, const RName& rName, optional<size_t> oArgIndex)
{   
    size_t slotIndex = slotInfos.size(); // 여기서의 index는 모든 named 변수의 index (local vars가 어디 들어있는지는 별개)
    auto name = format("%s{}_{}", slotIndex, RNameToString(rName));
    QType* qType = GetQTypeFromRType(rType);

    // 1. 함수 entry에서 할당할 목록에 추가
    slotInfos.emplace_back(qType, name, oArgIndex);
    
    // 2. 현재 스코프에 이름 추가
    curScope->localInfos[rName] = QLocalInfo_Var{slotIndex, name};
    curScope->slotIndices.push_back(slotIndex);

    return slotIndex;
}

void QBodyContext::AddLocalRef_Alias(const RName& rName, size_t slotIndex)
{
    curScope->localInfos[rName] = QLocalInfo_RefAlias{slotIndex, RNameToString(rName)};
    // 레퍼런스는 수명을 관리하지 않기 때문에 slotIndices에 추가하지 않는다
}

void QBodyContext::AddLocalRef_Ptr(RType* rType, const RName& rName, size_t slotIndex)
{
    QType* qType = GetQTypeFromRType(rType);
    curScope->localInfos[rName] = QLocalInfo_RefPtr{slotIndex, RNameToString(rName), qType};
}

size_t QBodyContext::NewSlot(QType* qType, optional<size_t> o_argIndex)
{
    size_t slotIndex = slotInfos.size();
    std::string s = format("%s{}", slotIndex);
    slotInfos.emplace_back(qType, s, o_argIndex);

    curScope->slotIndices.push_back(slotIndex);
    return slotIndex;
}

size_t QBodyContext::NewSlotForMExp(MExp* exp)
{
    auto* qType = GetMExpQType(exp);
    return NewSlot(qType);
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

QType* QBodyContext::GetReturnQType(RFuncDecl* rFuncDecl, RTypeArguments& typeArgs)
{
    auto* rType = rFuncDecl->GetReturnType(typeArgs);
    return GetQTypeFromRType(rType);
}

bool QBodyContext::IsVoidQType(QType* qType)
{
    return qType == qFactory->MakeVoidType();
}

void QBodyContext::PushScope()
{
    scopes.push_back(QScope{});
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
    // 순서는 거꾸로
    for (auto slotIndex : curScope->slotIndices | views::reverse)
    {
        // 소멸자 호출
        // TODO: HARD CODED
        if (slotInfos[slotIndex].qType == qFactory->MakeStringType())
        {
            assert(curBlock);
            curBlock->EmitInst(QInst_Dtor_String{QArg_Slot{slotIndex}});
        }
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
