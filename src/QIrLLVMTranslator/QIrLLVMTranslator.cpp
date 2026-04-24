#pragma warning(disable:4244 4722 4267 4146 4624)
#include "QIrLLVMTranslator.h"

#include <format>

#include <llvm/IR/LLVMContext.h>
#include <llvm/IR/Module.h>
#include <llvm/IR/BasicBlock.h>
#include <llvm/IR/IRBuilder.h>
#include <llvm/Support/raw_ostream.h>

#include "Infra/Variants.h"
#include "Infra/Exceptions.h"
#include "Infra/Unreachable.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RDecl.h"
#include "RSymbol/RFuncDecl.h"
#include "NSymbol/NFuncDecl.h"

#include "QIR/QFactory.h"
#include "QIR/QData.h"
#include "QIR/QFuncBody.h"
#include "QIR/QBlock.h"

using namespace std;

namespace Citron {

struct LDataImpl
{   
    std::unique_ptr<llvm::Module> _module;
};

enum class LRuntimeFuncKind
{
    Command,
    BoolToString,
    IntToString,
    StringCtor,
    StringCopyCtor,
    StringMoveCtor,
    StringCopyAssign,
    StringMoveAssign,
    StringDtor,
    StringConcat,
    StringLessThan,
    StringGreaterThan,
    StringLessThanOrEqual,
    StringGreaterThanOrEqual,
    StringEquals,
    Count,
};

class LContextImpl
{
public:
    llvm::LLVMContext context;

private:
    RFactoryPtr rFactory;
    QFactoryPtr qFactory;
    llvm::Type* stringType;

public:
    LContextImpl(const RFactoryPtr& rFactory, const QFactoryPtr& qFactory)
        : rFactory{rFactory}, qFactory{qFactory}
    {
    }
    
    llvm::Type* GetType(RType* rType)
    {
        // TODO: HARD CODED
        if (rType == rFactory->MakeVoidType())
            return GetVoidType();

        else if (rType == rFactory->MakeBoolType())
            return GetBoolType();

        else if (rType == rFactory->MakeIntType())
            return GetInt32Type();

        // string 을 무슨 타입으로 해야 하는지
        // 1) 현재 VC++ 컴파일러 옵션으로 sizeof(string)를 갖고 있는 타입으로
        else if (rType == rFactory->MakeStringType())
            return GetStringType();
        else throw NotImplementedException{};
    }

    llvm::Type* GetVoidType()
    {
        return llvm::Type::getVoidTy(context);
    }

    llvm::Type* GetStringType()
    {   
        if (stringType)
            return stringType;
        llvm::Type* byteTy = llvm::Type::getInt8Ty(context);
        llvm::ArrayType* bufTy = llvm::ArrayType::get(byteTy, sizeof(string));
        stringType = llvm::StructType::create({bufTy}, "string");
        return stringType;
    }

    llvm::PointerType* GetPtrType()
    {
        return llvm::PointerType::get(context, 0);
    }

    llvm::IntegerType* GetBoolType()
    {
        return llvm::Type::getInt1Ty(context);
    }

    llvm::IntegerType* GetInt32Type()
    {
        return llvm::Type::getInt32Ty(context);
    }
};

LData::~LData() = default;

LContext::LContext(const RFactoryPtr& rFactory, const QFactoryPtr& qFactory)
{
    impl = make_unique<LContextImpl>(rFactory, qFactory);
}

LContext::~LContext() = default;

class LModuleContext
{   
    using RuntimeFuncCtorContainer = array<llvm::Function* (LModuleContext::*)(llvm::Module&, LContextImpl&), (size_t)LRuntimeFuncKind::Count>;
    RuntimeFuncCtorContainer runtimeFuncCtors;
    array<llvm::Function*, (size_t)LRuntimeFuncKind::Count> runtimeFuncs;
    
    void InitRuntimeFuncCtors()
    {   
        using enum LRuntimeFuncKind;

        runtimeFuncCtors[(size_t)Command] = &LModuleContext::Init_Command;
        runtimeFuncCtors[(size_t)BoolToString] = &LModuleContext::Init_BoolToString;
        runtimeFuncCtors[(size_t)IntToString] = &LModuleContext::Init_IntToString;
        runtimeFuncCtors[(size_t)StringCtor] = &LModuleContext::Init_StringCtor;
        runtimeFuncCtors[(size_t)StringCopyCtor] = &LModuleContext::Init_StringCopyCtor;
        runtimeFuncCtors[(size_t)StringMoveCtor] = &LModuleContext::Init_StringMoveCtor;
        runtimeFuncCtors[(size_t)StringCopyAssign] = &LModuleContext::Init_StringCopyAssign;
        runtimeFuncCtors[(size_t)StringMoveAssign] = &LModuleContext::Init_StringMoveAssign;
        runtimeFuncCtors[(size_t)StringDtor] = &LModuleContext::Init_StringDestroy;
        runtimeFuncCtors[(size_t)StringConcat] = &LModuleContext::Init_StringConcat;
        runtimeFuncCtors[(size_t)StringLessThan] = &LModuleContext::Init_StringLessThan;
        runtimeFuncCtors[(size_t)StringGreaterThan] = &LModuleContext::Init_StringGreaterThan;
        runtimeFuncCtors[(size_t)StringLessThanOrEqual] = &LModuleContext::Init_StringLessThanOrEqual;
        runtimeFuncCtors[(size_t)StringGreaterThanOrEqual] = &LModuleContext::Init_StringGreaterThanOrEqual;
        runtimeFuncCtors[(size_t)StringEquals] = &LModuleContext::Init_StringEquals;

        for (auto& ctor : runtimeFuncCtors)
            assert(ctor);
    }

public:
    LModuleContext()
        : runtimeFuncs{}
    {   
        InitRuntimeFuncCtors();
    }

    llvm::Function* GetRuntimeFunc(LRuntimeFuncKind kind, llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto& runtimeFunc = runtimeFuncs[(size_t)kind];
        if (runtimeFunc) return runtimeFunc;

        runtimeFunc = (this->*runtimeFuncCtors[(size_t)kind])(_module, lContextImpl);
        return runtimeFunc;
    }

private:
    llvm::Function* Init_Command(llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetVoidType(), {lContextImpl.GetPtrType()}, /*isVarArg*/false);
        return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_command", _module);
    }

    llvm::Function* Init_BoolToString(llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetVoidType(), {lContextImpl.GetPtrType(), lContextImpl.GetBoolType()}, /*isVarArg*/false);
        return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_bool_to_string", _module);
    }

    llvm::Function* Init_IntToString(llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetVoidType(), {lContextImpl.GetPtrType(), lContextImpl.GetInt32Type()}, /*isVarArg*/false);
        return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_int_to_string", _module);
    }

    llvm::Function* Init_StringCtor(llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetVoidType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
        return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_ctor", _module);
    }

    llvm::Function* Init_StringCopyCtor(llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetVoidType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
        return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_copy_ctor", _module);
    }

    llvm::Function* Init_StringMoveCtor(llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetVoidType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
        return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_move_ctor", _module);
    }

    llvm::Function* Init_StringCopyAssign(llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetVoidType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
        return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_copy_assign", _module);
    }

    llvm::Function* Init_StringMoveAssign(llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetVoidType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
        return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_move_assign", _module);
    }

    llvm::Function* Init_StringDestroy(llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetVoidType(), {lContextImpl.GetPtrType()}, /*isVarArg*/false);
        return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_dtor", _module);
    }

    llvm::Function* Init_StringConcat(llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetVoidType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
        return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_concat", _module);
    }

    llvm::Function* Init_StringLessThan(llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetBoolType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
        return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_less_than", _module);
    }

    llvm::Function* Init_StringGreaterThan(llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetBoolType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
        return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_greater_than", _module);
    }

    llvm::Function* Init_StringLessThanOrEqual(llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetBoolType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
        return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_less_than_eq", _module);
    }

    llvm::Function* Init_StringGreaterThanOrEqual(llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetBoolType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
        return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_greater_than_eq", _module);
    }

    llvm::Function* Init_StringEquals(llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetBoolType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
        return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_eq", _module);
    }
};


class QFuncBodyToLFuncTranslator
{
    QFuncBody& qFuncBody;
    llvm::Module& _module;
    llvm::IRBuilder<>& builder;
    LContextImpl& lContextImpl;
    LModuleContext& lModuleContext;

    vector<llvm::Value*> slotValues;
    unordered_map<QBlock*, llvm::BasicBlock*> lBlocksByQBlock;

public:
    QFuncBodyToLFuncTranslator(QFuncBody& qFuncBody, llvm::Module& _module, llvm::IRBuilder<>& builder, LContextImpl& lContextImpl, LModuleContext& lModuleContext)
        : qFuncBody{qFuncBody}, _module{_module}, builder{builder}, lContextImpl{lContextImpl}, lModuleContext{lModuleContext}
    {   
    }

private:
    void SetSlot(QArg_Slot& slot, llvm::Value* value)
    {
        builder.CreateStore(value, slotValues[slot.index]);
    }

    llvm::Value* GetValue(QArg_Value& qInput, RType* rType)
    {
        return visit([this, rType](auto& qInput) -> llvm::Value*
        {
            using T = remove_cvref_t<decltype(qInput)>;
            if constexpr (same_as<T, QArg_Value_ConstBool>)
            {
                return llvm::ConstantInt::getBool(lContextImpl.context, qInput.value);
            }
            else if constexpr (same_as<T, QArg_Value_ConstInt32>)
            {   
                return llvm::ConstantInt::get(lContextImpl.GetInt32Type(), qInput.value);
            }
            else if constexpr (same_as<T, QArg_Value_Slot>)
            {
                auto* lValueType = lContextImpl.GetType(rType);
                return builder.CreateLoad(lValueType, slotValues[qInput.index]);
            }
            else static_assert(false);
        }, qInput);
    }

    llvm::Value* GetBool(QArg_CallArg& input)
    {
        return visit([this](auto& input) -> llvm::Value*
        {
            using T = remove_cvref_t<decltype(input)>;
            if constexpr (same_as<T, QArg_CallArg_ConstBool>)
            {
                return llvm::ConstantInt::getBool(lContextImpl.context, input.value);
            }
            else if constexpr (same_as<T, QArg_CallArg_Slot>)
            {
                return builder.CreateLoad(lContextImpl.GetBoolType(), slotValues[input.index]);
            }
            else if constexpr (same_as<T, QArg_CallArg_AddrOfSlot>)
            {
                return builder.CreateLoad(lContextImpl.GetBoolType(), slotValues[input.index]);
            }
            else
            {
                assert(false);
                return nullptr;
            }
        }, input);
    }

    llvm::Value* GetBool(QArg_Value& input)
    {
        return visit([this](auto& input) -> llvm::Value*
        {
            using T = remove_cvref_t<decltype(input)>;
            if constexpr (same_as<T, QArg_Value_ConstBool>)
            {
                return llvm::ConstantInt::getBool(lContextImpl.context, input.value);
            }
            else if constexpr (same_as<T, QArg_Value_Slot>)
            {
                return builder.CreateLoad(lContextImpl.GetBoolType(), slotValues[input.index]);
            }
            else
            {
                assert(false);
                return nullptr;
            }
        }, input);
    }

    llvm::Value* GetInt(QArg_Value& input)
    {
        return visit([this](auto& input) -> llvm::Value*
        {
            using T = remove_cvref_t<decltype(input)>;
            if constexpr (same_as<T, QArg_Value_ConstInt32>)
            {
                return llvm::ConstantInt::get(lContextImpl.GetInt32Type(), input.value);
            }
            else if constexpr (same_as<T, QArg_Value_Slot>)
            {
                return builder.CreateLoad(lContextImpl.GetInt32Type(), slotValues[input.index]);
            }
            else if constexpr (same_as<T, QArg_Value_ConstBool>)
            {
                assert(false);
                return nullptr;
            }
            else static_assert(false);
        }, input);
    }

    llvm::Value* GetPtr(QArg_Value& input)
    {
        return visit([this](auto& input) -> llvm::Value*
        {
            using T = remove_cvref_t<decltype(input)>;

            if constexpr (same_as<T, QArg_Value_Slot>)
            {
                return builder.CreateLoad(lContextImpl.GetPtrType(), slotValues[input.index]);
            }
            else if constexpr (same_as<T, QArg_Value_ConstBool>)
            {
                assert(false);
                return nullptr;
            }
            else if constexpr (same_as<T, QArg_Value_ConstInt32>)
            {
                assert(false);
                return nullptr;
            }
            else static_assert(false);

        }, input);
    }

    llvm::Value* GetStringRef(QArg_Value& input)
    {
        return visit([this](auto& input) -> llvm::Value*
        {
            using T = remove_cvref_t<decltype(input)>;
            if constexpr (same_as<T, QArg_Value_Slot>)
            {
                return slotValues[input.index];
            }
            else if constexpr (same_as<T, QArg_Value_ConstBool>)
            {
                assert(false);
                return nullptr;
            }
            else if constexpr (same_as<T, QArg_Value_ConstInt32>)
            {
                assert(false);
                return nullptr;
            }
            else static_assert(false);
        }, input);
    }

    llvm::Value* EmitRuntimeCall(LRuntimeFuncKind kind, llvm::ArrayRef<llvm::Value*> args)
    {
        auto* lFunc = lModuleContext.GetRuntimeFunc(kind, _module, lContextImpl);
        return builder.CreateCall(lFunc, args);
    }

    void EmitIntrinsic(QInst_Intrinsic& qInst)
    {
        switch (qInst.kind)
        {   
            using enum QInst_IntrinsicKind;

            case Command_Item: 
            {   
                for (size_t i = 0, count = qInst.args.size(); i < count; i++)
                {
                    // command에는 slot만 오게 된다
                    size_t slotIndex = get<QArg_CallArg_Slot>(qInst.args[i]).index; // TODO: QArg_CallArg_AddrOfSlot도 올 수 있다. 이 경우에는 slot의 주소를 넘겨주면 된다
                    EmitRuntimeCall(LRuntimeFuncKind::Command, {slotValues[slotIndex]});
                }

                return;
            }

            case Alloc_Int: { throw NotImplementedException{}; }
            case Memcpy_Void_Ptr_Ptr_Int: { throw NotImplementedException{}; }
            case NewList_Items: { throw NotImplementedException{}; }
            case GetIterator_ListPtr_ListIterator: { throw NotImplementedException{}; }

            case LogicalNot_Bool_Bool: 
            { 
                auto* boolValue = GetBool(qInst.args[0]);
                auto* notValue = builder.CreateNot(boolValue);
                SetSlot(*qInst.o_dest, notValue);
                return;
            }

            case UnaryMinus_Int_Int:
            {
                auto* intValue = GetInt(qInst.args[0]);
                auto* negValue = builder.CreateNeg(intValue);
                SetSlot(*qInst.o_dest, negValue);
                return;
            }

            case ToString_String_Bool: 
            { 
                auto* boolValue = GetBool(qInst.args[0]);
                EmitRuntimeCall(LRuntimeFuncKind::BoolToString, {slotValues[qInst.o_dest->index], boolValue});
                return;
            }

            case ToString_String_Int:
            {
                auto* intValue = GetInt(qInst.args[0]);
                EmitRuntimeCall(LRuntimeFuncKind::IntToString, {slotValues[qInst.o_dest->index], intValue});
                return;
            }

            case PrefixInc_Int_Int: 
            { 
                // int* p;
                // p에서 값을 읽어들인다 => r
                // r을 하나 올린다
                // r을 p에 저장한다
                auto* ptrValue = GetPtr(qInst.args[0]);
                auto* loadedValue = builder.CreateLoad(lContextImpl.GetInt32Type(), ptrValue);
                auto* increasedValue = builder.CreateAdd(loadedValue, llvm::ConstantInt::get(lContextImpl.GetInt32Type(), 1));
                builder.CreateStore(increasedValue, ptrValue);
                SetSlot(*qInst.o_dest, increasedValue);
                return;
            }

            case PrefixDec_Int_Int:
            {
                auto* ptrValue = GetPtr(qInst.args[0]);
                auto* loadedValue = builder.CreateLoad(lContextImpl.GetInt32Type(), ptrValue);
                auto* decreasedValue = builder.CreateSub(loadedValue, llvm::ConstantInt::get(lContextImpl.GetInt32Type(), 1));
                builder.CreateStore(decreasedValue, ptrValue);
                SetSlot(*qInst.o_dest, decreasedValue);
                return;
            }

            case PostfixInc_Int_Int: 
            { 
                auto* ptrValue = GetPtr(qInst.args[0]);
                auto* loadedValue = builder.CreateLoad(lContextImpl.GetInt32Type(), ptrValue);
                auto* increasedValue = builder.CreateAdd(loadedValue, llvm::ConstantInt::get(lContextImpl.GetInt32Type(), 1));
                builder.CreateStore(increasedValue, ptrValue);
                SetSlot(*qInst.o_dest, loadedValue);
                return;
            }

            case PostfixDec_Int_Int: 
            {
                auto* ptrValue = GetPtr(qInst.args[0]);
                auto* loadedValue = builder.CreateLoad(lContextImpl.GetInt32Type(), ptrValue);
                auto* decreasedValue = builder.CreateSub(loadedValue, llvm::ConstantInt::get(lContextImpl.GetInt32Type(), 1));
                builder.CreateStore(decreasedValue, ptrValue);
                SetSlot(*qInst.o_dest, loadedValue);
                return;
            }

            case Multiply_Int_Int_Int: 
            { 
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateMul(operand0, operand1);
                SetSlot(*qInst.o_dest, result);
                return;
            }

            case Divide_Int_Int_Int:
            {
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateSDiv(operand0, operand1);
                SetSlot(*qInst.o_dest, result);
                return;
            }

            case Modulo_Int_Int_Int: 
            { 
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateSRem(operand0, operand1);
                SetSlot(*qInst.o_dest, result);
                return;
            }

            case Add_Int_Int_Int: 
            { 
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateAdd(operand0, operand1);
                SetSlot(*qInst.o_dest, result);
                return;
            }

            case Add_String_StringInRef_StringInRef: 
            { 
                auto* str0 = GetStringRef(qInst.args[0]);
                auto* str1 = GetStringRef(qInst.args[1]);
                auto* result = slotValues[qInst.o_dest->index];
                EmitRuntimeCall(LRuntimeFuncKind::StringConcat, {result, str0, str1});
                return;
            }
            
            case Subtract_Int_Int_Int:
            {
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateSub(operand0, operand1);
                SetSlot(*qInst.o_dest, result);
                return;
            }

            case LessThan_Bool_Int_Int: 
            {
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateICmpSLT(operand0, operand1);
                SetSlot(*qInst.o_dest, result);
                return;
            }

            case LessThan_Bool_StringInRef_StringInRef: 
            { 
                auto* str0 = GetStringRef(qInst.args[0]);
                auto* str1 = GetStringRef(qInst.args[1]);                
                auto* result = EmitRuntimeCall(LRuntimeFuncKind::StringLessThan, {str0, str1});
                SetSlot(*qInst.o_dest, result);
                return;
            }

            case GreaterThan_Bool_Int_Int: 
            { 
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateICmpSGT(operand0, operand1);
                SetSlot(*qInst.o_dest, result);
                return;
            }

            case GreaterThan_Bool_StringInRef_StringInRef: 
            { 
                auto* str0 = GetStringRef(qInst.args[0]);
                auto* str1 = GetStringRef(qInst.args[1]);                
                auto* result = EmitRuntimeCall(LRuntimeFuncKind::StringGreaterThan, {str0, str1});
                SetSlot(*qInst.o_dest, result);
                return;
            }

            case LessThanOrEqual_Bool_Int_Int: 
            { 
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateICmpSLE(operand0, operand1);
                SetSlot(*qInst.o_dest, result);
                return;
            }

            case LessThanOrEqual_Bool_StringInRef_StringInRef:
            {
                auto* str0 = GetStringRef(qInst.args[0]);
                auto* str1 = GetStringRef(qInst.args[1]);                
                auto* result = EmitRuntimeCall(LRuntimeFuncKind::StringLessThanOrEqual, {str0, str1});
                SetSlot(*qInst.o_dest, result);
                return;
            }

            case GreaterThanOrEqual_Bool_Int_Int: 
            { 
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateICmpSGE(operand0, operand1);
                SetSlot(*qInst.o_dest, result);
                return;
            }

            case GreaterThanOrEqual_Bool_StringInRef_StringInRef: 
            { 
                auto* str0 = GetStringRef(qInst.args[0]);
                auto* str1 = GetStringRef(qInst.args[1]);                
                auto* result = EmitRuntimeCall(LRuntimeFuncKind::StringGreaterThanOrEqual, {str0, str1});
                SetSlot(*qInst.o_dest, result);
                return;
            }

            case Equal_Bool_Int_Int: 
            {
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateICmpEQ(operand0, operand1);
                SetSlot(*qInst.o_dest, result);
                return;
            }

            case Equal_Bool_Bool_Bool: 
            { 
                auto* operand0 = GetBool(qInst.args[0]);
                auto* operand1 = GetBool(qInst.args[1]);
                auto* result = builder.CreateICmpEQ(operand0, operand1);
                SetSlot(*qInst.o_dest, result);
                return;
            }

            case Equal_Bool_StringInRef_StringInRef:
            {
                auto* str0 = GetStringRef(qInst.args[0]);
                auto* str1 = GetStringRef(qInst.args[1]);                
                auto* result = EmitRuntimeCall(LRuntimeFuncKind::StringEquals, {str0, str1});
                SetSlot(*qInst.o_dest, result);
                return;
            }

            case CopyCtor_Void_StringRef_StringInRef:
            {
                auto* thisPtr = GetPtr(qInst.args[0]);
                auto* otherPtr = GetPtr(qInst.args[1]);
                EmitRuntimeCall(LRuntimeFuncKind::StringCopyCtor, {thisPtr, otherPtr});
                return;
            }

            case MoveCtor_Void_StringRef_StringMoveRef:
            {
                auto* thisPtr = GetPtr(qInst.args[0]);
                auto* otherPtr = GetPtr(qInst.args[1]);
                EmitRuntimeCall(LRuntimeFuncKind::StringMoveCtor, {thisPtr, otherPtr});
                return;
            }

            case Dtor_Void_StringRef:
            {
                auto* thisPtr = GetPtr(qInst.args[0]);
                EmitRuntimeCall(LRuntimeFuncKind::StringMoveCtor, {thisPtr});
                return;
            }

            case CopyAssign_Void_StringRef_StringInRef:
            {
                auto* thisPtr = GetPtr(qInst.args[0]);
                auto* otherPtr = GetPtr(qInst.args[1]);
                EmitRuntimeCall(LRuntimeFuncKind::StringCopyAssign, {thisPtr, otherPtr});
                return;
            }

            case MoveAssign_Void_StringRef_StringInRef:                
            {
                auto* thisPtr = GetPtr(qInst.args[0]);
                auto* otherPtr = GetPtr(qInst.args[1]);
                EmitRuntimeCall(LRuntimeFuncKind::StringMoveAssign, {thisPtr, otherPtr});
                return;
            }
        }

        unreachable();
    }

    struct Emitter
    {
        QFuncBodyToLFuncTranslator& self;

        void operator()(auto& inst) { Emit(inst); }

        void Emit(QInst_Ctor_String& qInst)
        {
            auto* textPtr = self.builder.CreateGlobalString(qInst.text);
            self.EmitRuntimeCall(LRuntimeFuncKind::StringCtor, {self.slotValues[qInst.thisSlot.index], textPtr});
        }
        
        void Emit(QInst_Load& qInst)
        {
            auto* lPtrValueType = self.lContextImpl.GetPtrType();
            auto* lPtrValue = self.builder.CreateLoad(self.lContextImpl.GetPtrType(), self.slotValues[qInst.src.index]);
            auto* lValueType = self.lContextImpl.GetType(qInst.type);
            auto* lValue = self.builder.CreateLoad(lValueType, lPtrValue);

            self.builder.CreateStore(lValue, self.slotValues[qInst.dest.index]);
        }

        void Emit(QInst_Store& qInst)
        {
            auto* lValue = self.GetValue(qInst.src, qInst.type);
            auto* lPtrValue = self.builder.CreateLoad(self.lContextImpl.GetPtrType(), self.slotValues[qInst.dest.index]);
            self.builder.CreateStore(lValue, lPtrValue);
        }

        void Emit(QInst_AddrOf& qInst)
        {
            auto* lPtrValue = self.builder.CreateLoad(self.lContextImpl.GetPtrType(), self.slotValues[qInst.dest.index]);
            self.builder.CreateStore(self.slotValues[qInst.slot.index], lPtrValue);
        }

        void Emit(QInst_FieldOf& qInst)
        {
            throw NotImplementedException{};
        }

        void Emit(QInst_Assign& qInst)
        {
            auto* lValue = self.GetValue(qInst.src, qInst.type);
            self.builder.CreateStore(lValue, self.slotValues[qInst.dest.index]);
        }

        void Emit(QInst_Call& qInst)
        {
            throw NotImplementedException{};
        }

        void Emit(QInst_Return& qInst)
        {
            if (!qInst.o_value)
            {
                self.builder.CreateRetVoid();
            }
            else
            {
                auto* v = self.GetValue(qInst.o_value->value, qInst.o_value->type);
                self.builder.CreateRet(v);
            }
        }

        void Emit(QInst_Intrinsic& qInst)
        {
            self.EmitIntrinsic(qInst);
        }

        void Emit(QInst_CondJump& qInst)
        {
            auto* lCond = self.builder.CreateLoad(self.lContextImpl.GetBoolType(), self.slotValues[qInst.cond.index]);
            self.builder.CreateCondBr(lCond, self.lBlocksByQBlock[qInst.trueBlock], self.lBlocksByQBlock[qInst.falseBlock]);
        }

        void Emit(QInst_Jump& qInst)
        {
            self.builder.CreateBr(self.lBlocksByQBlock[qInst.block]);
        }
    };

    void Emit(QInst& qInst)
    {
        Emitter emitter{*this};
        visit(emitter, qInst);
    }

public:
    void Translate()
    {
        auto rFuncReturn = qFuncBody.nFuncDecl->GetRFuncDecl()->GetUnboundFuncReturn();
        auto* lRetType = visit([this](auto& ret) -> llvm::Type* {
            using T = remove_cvref_t<decltype(ret)>;
            if constexpr (same_as<T, RFuncReturn_ForCtor>)
            {
                // void
                return lContextImpl.GetVoidType();
            }
            else if constexpr (same_as<T, RFuncReturn_Set>)
            {
                return lContextImpl.GetType(ret.type);
            }
            else if constexpr (same_as<T, RFuncReturn_NotSet>)
            {
                assert(false);
                return nullptr;
            }
            else static_assert(false);

        }, rFuncReturn);

        vector<llvm::Type*> lParamTypes;
        for (auto& rFuncParam : qFuncBody.nFuncDecl->GetRFuncDecl()->GetUnboundFuncParams())
        {
            auto* lParamType = lContextImpl.GetType(rFuncParam.type);
            lParamTypes.push_back(lParamType);
        }

        std::string name = RNameToString(qFuncBody.nFuncDecl->GetNDecl()->GetRDecl()->GetIdentifier().name);

        llvm::FunctionType* lFuncType = llvm::FunctionType::get(lRetType, lParamTypes, /*isVarArg*/false);
        auto* lFunc = llvm::Function::Create(lFuncType, llvm::GlobalValue::LinkageTypes::ExternalLinkage, name, _module);

        auto* preludeBlock = llvm::BasicBlock::Create(lContextImpl.context, "prelude", lFunc);
        builder.SetInsertPoint(preludeBlock);
        
        slotValues.reserve(qFuncBody.slotInfos.size());

        // 함수 인자 이름 붙이기.
        for (auto& qSlotInfo : qFuncBody.slotInfos)
        {
            // 함수 초기에 할당

            if (qSlotInfo.o_argIndex) // 아규먼트에 매핑 되어있으면,
            {
                auto* lArg = lFunc->getArg(*qSlotInfo.o_argIndex);
                lArg->setName(qSlotInfo.name.substr(1) + "_arg");
                auto* lType = lContextImpl.GetType(qSlotInfo.type);
                auto* allocaInst = builder.CreateAlloca(lType, nullptr, qSlotInfo.name.substr(1));

                builder.CreateStore(lArg, allocaInst);
                slotValues.push_back(allocaInst);
            }
            else
            {
                // 함수 초기에 할당
                auto* lType = lContextImpl.GetType(qSlotInfo.type);
                auto* allocaInst = builder.CreateAlloca(lType, nullptr, qSlotInfo.name.substr(1));
                slotValues.push_back(allocaInst);
            }
        }

        // basic block을 미리 다 만들어 두는게 나을거 같다
        for (auto* qBlock : qFuncBody.blocks)
        {
            auto* lBlock = llvm::BasicBlock::Create(lContextImpl.context, qBlock->GetName(), lFunc);
            lBlocksByQBlock.try_emplace(qBlock, lBlock);
        }

        builder.CreateBr(lBlocksByQBlock[qFuncBody.blocks.front()]);

        for (size_t i = 0, count = qFuncBody.blocks.size(); i < count; i++)
        {
            auto* qBlock = qFuncBody.blocks[i];
            auto* lBlock = lBlocksByQBlock[qBlock];

            builder.SetInsertPoint(lBlock);
            for (auto& qInst : qBlock->GetInsts())
            {
                Emit(qInst);
            }
        }
    }
};

// 파일 하나에 대해서만 하는거
LData TranslateQDataToLData(QData* qData, LContext& lContext)
{
    auto& lContextImpl = *lContext.impl;
    auto _module = make_unique<llvm::Module>("MyModule", lContextImpl.context);

    LModuleContext lModuleContext;
    for (auto& qFuncBody : qData->GetAllBodies())
    {   
        llvm::IRBuilder<> builder{lContextImpl.context};
        QFuncBodyToLFuncTranslator translator{qFuncBody, *_module, builder, lContextImpl, lModuleContext};
        translator.Translate();
    }

    string output;
    llvm::raw_string_ostream stream{output};
    _module->print(stream, nullptr);

    auto lDataImpl = make_unique<LDataImpl>(move(_module));

    return LData{move(lDataImpl), move(output)};
}



} // namespace Citron