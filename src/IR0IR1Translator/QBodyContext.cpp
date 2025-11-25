#include "QBodyContext.h"

#include <variant>
#include <format>

#include "Infra/Unreachable.h"
#include "Infra/Exceptions.h"
#include "Infra/Variants.h"

#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RNames.h"

#include "MIR/MExp.h"

#include "QIR/QInsts.h"
#include "QIR/QArgs.h"
#include "QIR/QFactory.h"


using namespace std;

namespace Citron::IR0IR1Translator {

QBlockWriter::QBlockWriter(const QFactoryPtr& qFactory, string&& firstBlockName)
    : qFactory{qFactory}
{
    auto* firstBlock = qFactory->MakeQBlock(format("b{}_{}", blocks.size(), move(firstBlockName)));
    this->curBlock = firstBlock;
    this->blocks.push_back(firstBlock);
    this->state = QBlockWriterState::CanWrite;
}

void QBlockWriter::EmitInstInternal(QInst&& inst)
{
    switch (state)
    {
    case QBlockWriterState::CanWrite:
        curBlock->EmitInst(std::move(inst));
        return;

    case QBlockWriterState::EndOfBlock:
        assert(false);
    }

    unreachable();
}

QBlock* QBlockWriter::AddBlock(string&& debugText)
{
    auto* newBlock = qFactory->MakeQBlock(format("b{}_{}", blocks.size(), move(debugText)));
    blocks.push_back(newBlock);
    pendingBlocks.push_back(newBlock);
    return newBlock;
}

void QBlockWriter::CompleteBlock(QTermInst&& termInst)
{
    switch(state)
    {
    case QBlockWriterState::CanWrite:
        EmitInstInternal(visit([](auto&& termInst) -> QInst { return termInst; }, termInst));
        state = QBlockWriterState::EndOfBlock;
        return;

    case QBlockWriterState::EndOfBlock:
        assert(false);
        return;
    }
}

void QBlockWriter::SetCurBlock(QBlock* block)
{
    switch (state)
    {
    case QBlockWriterState::CanWrite:
        assert(false);

    case QBlockWriterState::EndOfBlock:
        for (size_t i = 0, count = pendingBlocks.size(); i < count; i++)
        {
            if (pendingBlocks[i] == block)
            {   
                // swap
                if (size_t lastIndex = count - 1; i != lastIndex)
                {
                    auto* t = pendingBlocks[lastIndex];
                    pendingBlocks[lastIndex] = pendingBlocks[i];
                    pendingBlocks[i] = t;
                }

                pendingBlocks.pop_back();
                curBlock = block;
                state = QBlockWriterState::CanWrite;
                return;
            }
        }
        assert(false);
    }
}

void QBlockWriter::Verify()
{
    assert(state == QBlockWriterState::EndOfBlock);
    assert(pendingBlocks.empty());
}

QBodyContext::QBodyContext(const RFactoryPtr& rFactory, const QFactoryPtr& qFactory)
    : rFactory{rFactory}
    , qFactory{qFactory}
    , QBlockWriter{qFactory, "body"}
    , entryBlock{nullptr}
{
    scopes.emplace_back();
    bodyBlock = QBlockWriter::GetCurBlock();
}

// GetIntrinsicResultType
// 1. result = intrinsic kind, args, ...
// 2. intrinsic kind, &result, args, ...
// 3. intrinsic kind, args, ... (void)
QIntrinsicKindResultType QBodyContext::GetIntrinsicResultType(QInst_IntrinsicKind kind)
{
    switch (kind)
    {
    case QInst_IntrinsicKind::DebugPrint_Items: 
        return QIntrinsicKindResultType_Void{};

    case QInst_IntrinsicKind::Command_Items: 
        return QIntrinsicKindResultType_Void{};

    case QInst_IntrinsicKind::Alloc_Int: throw NotImplementedException{};

    case QInst_IntrinsicKind::Memcpy_Ptr_Ptr_Int:
        return QIntrinsicKindResultType_Void{};

    case QInst_IntrinsicKind::NewList_Items: throw NotImplementedException{};
    case QInst_IntrinsicKind::GetListIterator_List: throw NotImplementedException{};

    case QInst_IntrinsicKind::LogicalNot_Bool:
        return QIntrinsicKindResultType_Register{QRegisterType::Int1};

    case QInst_IntrinsicKind::UnaryMinus_Int:
        return QIntrinsicKindResultType_Register{QRegisterType::Int32};

    case QInst_IntrinsicKind::ToString_Bool:
    case QInst_IntrinsicKind::ToString_Int: 
    case QInst_IntrinsicKind::Add_String_String:
        return QIntrinsicKindResultType_StackSlot{qFactory->MakeStringType()};

    case QInst_IntrinsicKind::PrefixInc_Int: 
    case QInst_IntrinsicKind::PrefixDec_Int:
    case QInst_IntrinsicKind::PostfixInc_Int:
    case QInst_IntrinsicKind::PostfixDec_Int:

    case QInst_IntrinsicKind::Multiply_Int_Int:
    case QInst_IntrinsicKind::Divide_Int_Int:
    case QInst_IntrinsicKind::Modulo_Int_Int:
    case QInst_IntrinsicKind::Add_Int_Int:
    case QInst_IntrinsicKind::Subtract_Int_Int:
        return QIntrinsicKindResultType_Register{QRegisterType::Int32};
    
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
        return QIntrinsicKindResultType_Register{QRegisterType::Int1};
    }

    throw NotImplementedException{};
}

void QBodyContext::EmitIntrinsic(QInst_IntrinsicKind kind, optional<QArg_Loc> oResult, std::vector<QArg_Input>&& args)
{
    auto resultType = GetIntrinsicResultType(kind);
    
    visit(overloaded{
        [kind, this, &args, oResult](QIntrinsicKindResultType_Register& regType)
        {
            // intrinsic은 Register에 리턴값을 주려고 하는데, oResult가 nullopt거나 StackSlot일 수도 있다.
            // intrinsic은 함수 경계로 봐야 하고. 함수의 리턴값 정책은 정해져 있으므로. exp에 값을 저장할 공간을 미리 줬더라도 쓰지 않을 수 있다
            // 일단 최대한 쓰는 쪽으로는 가겠지만, 인자에 따라서 안쓰더라도 제대로 동작은 해야한다
            // 이건 call도 마찬가지일 것 같다
            if (!oResult)
            {
                // 레지스터를 하나 생성하고, Intrinsic을 실행한 다음, 바로 종료
                auto reg = NewRegister(regType.type);
                QBlockWriter::EmitInst(QInst_Intrinsic{kind, reg, move(args)});
            }
            else if (auto* reg = get_if<QArg_Register>(&*oResult))
            {
                // 모든것이 맞아떨어지는 경우
                QBlockWriter::EmitInst(QInst_Intrinsic{kind, *reg, move(args)});
            }
            else if (auto* slot = get_if<QArg_StackSlot>(&*oResult))
            {
                // 레지스터를 하나 생성하고
                auto reg = NewRegister(regType.type);
                QBlockWriter::EmitInst(QInst_Intrinsic{kind, reg, move(args)});

                // stack에 바로 store
                QBlockWriter::EmitInst(QInst_Store{regType.type, *slot, reg});
            }
        },
        [kind, this, &args, oResult](QIntrinsicKindResultType_StackSlot& slotType)
        {
            args.insert(args.begin(), get<QArg_StackSlot>(*oResult));
            QBlockWriter::EmitInst(QInst_Intrinsic{kind, nullopt, move(args)});
        },
        [kind, this, &args](QIntrinsicKindResultType_Void&) 
        { 
            QBlockWriter::EmitInst(QInst_Intrinsic{kind, nullopt, move(args)});
        }
    }, resultType);
}

optional<QRegisterType> QBodyContext::GetRegisterType(QType* qType)
{
    // TODO: HARD CODED
    if (qType == qFactory->MakeBoolType())
        return QRegisterType::Int1;

    if (qType == qFactory->MakeIntType())
        return QRegisterType::Int32;

    return nullopt;
}

QArg_Register QBodyContext::NewRegister(QRegisterType type)
{
    size_t index = registerInfos.size();

    string name;
    switch (type)
    {
    case QRegisterType::Ptr: name = format("%p{}", index); break;
    case QRegisterType::Int1: name = format("%b{}", index); break;
    case QRegisterType::Int32: name = format("%i{}", index); break;
    default: unreachable();
    }

    registerInfos.emplace_back(type, name);
    return {index};
}

QType* QBodyContext::GetMExpQType(MExp* mExp)
{
    return MakeQType(mExp->GetType(*rFactory));
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

string RNameToString(const RName& name)
{
    return visit(overloaded{
        [](const RName_Normal& n) { return n.text; },
        [](const RName_Reserved& n) { return format("${}", n.text); },
        [](const RName_Lambda& n) { return format("$$lambdaVar{}>", n.index); },
        [](const RName_CtorParam& n) { return format("$$ctor_{}", n.paramText); }
    }, name);
}

QType* QBodyContext::MakeQType(RType* rType)
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

QType_Class* QBodyContext::MakeQStringType()
{
    return qFactory->MakeStringType();
}

QArg_StackSlot QBodyContext::AddLocalVar(RType* rType, const RName& rName)
{   
    size_t slotIndex = slotInfos.size(); // 여기서의 index는 모든 named 변수의 index (local vars가 어디 들어있는지는 별개)
    auto name = format("%s{}_{}", slotIndex, RNameToString(rName));
    QType* qType = MakeQType(rType);

    // 1. 함수 entry에서 할당할 목록에 추가
    slotInfos.emplace_back(name, qType);
    
    // 2. 현재 스코프에 이름 추가
    scopes.back().localVarInfos[rName] = QLocalVarInfo{slotIndex, name, qType};

    return {slotIndex};
}

QArg_StackSlot QBodyContext::GetLocalVar(const RName& name)
{   
    auto localVarInfo = scopes.back().localVarInfos[name];
    return {localVarInfo.slotIndex};
}

QArg_StackSlot QBodyContext::NewStackSlot(QType* qType)
{
    size_t slotIndex = slotInfos.size();
    std::string s = format("%s{}", slotIndex);
    slotInfos.emplace_back(s, qType);
    return {slotIndex};
}

QArg_Loc QBodyContext::NewContainerForMExp(MExp* exp)
{
    auto* qType = GetMExpQType(exp);
    
    if (auto oRegType = GetRegisterType(qType))
    {
        return NewRegister(*oRegType);
    }
    else
    {
        return NewStackSlot(qType);
    }
}

void QBodyContext::CompleteFunc()
{
    // make entry
    entryBlock = QBlockWriter::AddBlock("entry");
    QBlockWriter::SetCurBlock(entryBlock);

    // entry를 일단은 살려둠

    QBlockWriter::CompleteBlock(QInst_Jump{bodyBlock});
    QBlockWriter::Verify();
}


} // Citron::IR0IR1Translator
