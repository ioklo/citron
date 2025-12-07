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
#include "RSymbol/RFactory.h"
#include "RSymbol/RDecl.h"
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
    StringInit,
    StringDestroy,
    StringConcat,
    StringLessThan,
    StringGreaterThan,
    StringLessThanOrEqual,
    StringGreaterThanOrEqual,
    StringEquals,
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

    llvm::Type* GetType(QType* qType)
    {
        // TODO: HARD CODED
        if (qType == qFactory->MakeVoidType())
            return GetVoidType();

        else if (qType == qFactory->MakeBoolType())
            return GetBoolType();

        else if (qType == qFactory->MakeIntType())
            return GetInt32Type();

        // string 을 무슨 타입으로 해야 하는지
        // 1) 현재 VC++ 컴파일러 옵션으로 sizeof(string)를 갖고 있는 타입으로
        else if (qType == qFactory->MakeStringType())
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

// TODO: 공통 라이브러리로 빼기
string RNameToString2(const RName& name)
{
    return visit(overloaded{
        [](const RName_Normal& n) { return n.text; },
        [](const RName_Reserved& n) { return format("${}", n.text); },
        [](const RName_Lambda& n) { return format("$$lambdaVar{}>", n.index); },
        [](const RName_CtorParam& n) { return format("$$ctor_{}", n.paramText); }
    }, name);
}

class LModuleContext
{   
    struct RuntimeFuncEntry
    {
        llvm::Function* func;
        std::function<llvm::Function* (llvm::Module& _module, LContextImpl& lContextImpl)> ctor;

        RuntimeFuncEntry(std::function<llvm::Function* (llvm::Module& _module, LContextImpl& lContextImpl)> ctor)
            : func{nullptr}, ctor{move(ctor)}
        {
        }
    };

    unordered_map<LRuntimeFuncKind, RuntimeFuncEntry> runtimeFuncs;

public:
    void InitRuntimeFuncs()
    {   
        {
            // void citron_command(string*);
            auto ctor = [](llvm::Module& _module, LContextImpl& lContextImpl) {
                auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetVoidType(), {lContextImpl.GetPtrType()}, /*isVarArg*/false);
                return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_command", _module);
            };
            runtimeFuncs.try_emplace(LRuntimeFuncKind::Command, move(ctor));
        }

        {
            // citron_bool_to_string(void* dest, bool value);
            auto ctor = [](llvm::Module& _module, LContextImpl& lContextImpl) {
                auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetVoidType(), {lContextImpl.GetPtrType(), lContextImpl.GetBoolType()}, /*isVarArg*/false);
                return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_bool_to_string", _module);
            };
            runtimeFuncs.try_emplace(LRuntimeFuncKind::BoolToString, move(ctor));
        }

        {
            // void citron_int_to_string(void* dest, int value);
            auto ctor = [](llvm::Module& _module, LContextImpl& lContextImpl) {
                auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetVoidType(), {lContextImpl.GetPtrType(), lContextImpl.GetInt32Type()}, /*isVarArg*/false);
                return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_int_to_string", _module);
            };
            runtimeFuncs.try_emplace(LRuntimeFuncKind::IntToString, move(ctor));
        }

        {
            // void citron_string_init(string* dest, const char* text);
            auto ctor = [](llvm::Module& _module, LContextImpl& lContextImpl) {
                auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetVoidType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
                return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_init", _module);
            };
            runtimeFuncs.try_emplace(LRuntimeFuncKind::StringInit, move(ctor));
        }

        {
            // void citron_string_destroy(string* dest);
            auto ctor = [](llvm::Module& _module, LContextImpl& lContextImpl) {
                auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetVoidType(), {lContextImpl.GetPtrType()}, /*isVarArg*/false);
                return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_destroy", _module);
            };
            runtimeFuncs.try_emplace(LRuntimeFuncKind::StringDestroy, move(ctor));
        }

        {
            // void citron_string_concat(string* dest, string* x, string* y);
            auto ctor = [](llvm::Module& _module, LContextImpl& lContextImpl) {
                auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetVoidType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
                return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_concat", _module);
            };
            runtimeFuncs.try_emplace(LRuntimeFuncKind::StringConcat, move(ctor));
        }

        {
            // bool citron_string_less_than(string* x, string* y);
            auto ctor = [](llvm::Module& _module, LContextImpl& lContextImpl) {
                auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetBoolType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
                return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_less_than", _module);
            };
            runtimeFuncs.try_emplace(LRuntimeFuncKind::StringLessThan, move(ctor));
        }

        {
            // bool citron_string_greater_than(string* x, string* y);
            auto ctor = [](llvm::Module& _module, LContextImpl& lContextImpl) {
                auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetBoolType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
                return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_greater_than", _module);
            };
            runtimeFuncs.try_emplace(LRuntimeFuncKind::StringGreaterThan, move(ctor));
        }

        {
            // bool citron_string_less_than_eq(string* x, string* y);
            auto ctor = [](llvm::Module& _module, LContextImpl& lContextImpl) {
                auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetBoolType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
                return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_less_than_eq", _module);
            };
            runtimeFuncs.try_emplace(LRuntimeFuncKind::StringLessThanOrEqual, move(ctor));
        }

        {
            // bool citron_string_greater_than_eq(string* x, string* y);
            auto ctor = [](llvm::Module& _module, LContextImpl& lContextImpl) {
                auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetBoolType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
                return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_greater_than_eq", _module);
            };
            runtimeFuncs.try_emplace(LRuntimeFuncKind::StringGreaterThanOrEqual, move(ctor));
        }

        {
            // bool citron_string_eq(string* x, string* y);
            auto ctor = [](llvm::Module& _module, LContextImpl& lContextImpl) {
                auto* rtFuncTy = llvm::FunctionType::get(lContextImpl.GetBoolType(), {lContextImpl.GetPtrType(), lContextImpl.GetPtrType()}, /*isVarArg*/false);
                return llvm::Function::Create(rtFuncTy, llvm::Function::ExternalLinkage, "citron_string_eq", _module);
            };
            runtimeFuncs.try_emplace(LRuntimeFuncKind::StringEquals, move(ctor));
        }
    }
    llvm::Function* GetRuntimeFunc(LRuntimeFuncKind kind, llvm::Module& _module, LContextImpl& lContextImpl)
    {
        auto i = runtimeFuncs.find(kind);
        assert(i != runtimeFuncs.end());

        if (i->second.func) 
            return i->second.func;

        i->second.func = i->second.ctor(_module, lContextImpl);
        return i->second.func;
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

    llvm::Value* GetValue(QArg_Input& qInput, QType* qType)
    {
        return visit([this, qType](auto& qInput) -> llvm::Value*
        {
            using T = remove_cvref_t<decltype(qInput)>;
            if constexpr (same_as<T, QArg_ConstBool>)
            {
                return llvm::ConstantInt::getBool(lContextImpl.context, qInput.value);
            }
            else if constexpr (same_as<T, QArg_ConstInt32>)
            {   
                return llvm::ConstantInt::get(lContextImpl.GetInt32Type(), qInput.value);
            }
            else if constexpr (same_as<T, QArg_Slot>)
            {
                auto* lValueType = lContextImpl.GetType(qType);
                return builder.CreateLoad(lValueType, slotValues[qInput.index]);
            }
            else static_assert(false);
        }, qInput);
    }

    llvm::Value* GetBool(QArg_Input& input)
    {
        return visit([this](auto& input) -> llvm::Value*
        {
            using T = remove_cvref_t<decltype(input)>;
            if constexpr (same_as<T, QArg_ConstBool>)
            {
                return llvm::ConstantInt::getBool(lContextImpl.context, input.value);
            }
            else if constexpr (same_as<T, QArg_Slot>)
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

    llvm::Value* GetInt(QArg_Input& input)
    {
        return visit([this](auto& input) -> llvm::Value*
        {
            using T = remove_cvref_t<decltype(input)>;
            if constexpr (same_as<T, QArg_ConstInt32>)
            {
                return llvm::ConstantInt::get(lContextImpl.GetInt32Type(), input.value);
            }
            else if constexpr (same_as<T, QArg_Slot>)
            {
                return builder.CreateLoad(lContextImpl.GetInt32Type(), slotValues[input.index]);
            }
            else if constexpr (same_as<T, QArg_ConstBool>)
            {
                assert(false);
                return nullptr;
            }
            else static_assert(false);
        }, input);
    }

    llvm::Value* GetPtr(QArg_Input& input)
    {
        return visit([this](auto& input) -> llvm::Value*
        {
            using T = remove_cvref_t<decltype(input)>;

            if constexpr (same_as<T, QArg_Slot>)
            {
                return builder.CreateLoad(lContextImpl.GetPtrType(), slotValues[input.index]);
            }
            else if constexpr (same_as<T, QArg_ConstBool>)
            {
                assert(false);
                return nullptr;
            }
            else if constexpr (same_as<T, QArg_ConstInt32>)
            {
                assert(false);
                return nullptr;
            }
            else static_assert(false);

        }, input);
    }

    llvm::Value* GetStringRef(QArg_Input& input)
    {
        return visit([this](auto& input) -> llvm::Value*
        {
            using T = remove_cvref_t<decltype(input)>;
            if constexpr (same_as<T, QArg_Slot>)
            {
                return slotValues[input.index];
            }
            else if constexpr (same_as<T, QArg_ConstBool>)
            {
                assert(false);
                return nullptr;
            }
            else if constexpr (same_as<T, QArg_ConstInt32>)
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

            case Command_Items: 
            {   
                for (size_t i = 0, count = qInst.args.size(); i < count; i++)
                {
                    // command에는 slot만 오게 된다
                    size_t slotIndex = get<QArg_Slot>(qInst.args[i]).index;
                    EmitRuntimeCall(LRuntimeFuncKind::Command, {slotValues[slotIndex]});
                }

                return;
            }

            case Alloc_Int: { throw NotImplementedException{}; }
            case Memcpy_Ptr_Ptr_Int: { throw NotImplementedException{}; }
            case NewList_Items: { throw NotImplementedException{}; }
            case GetListIterator_List: { throw NotImplementedException{}; }

            case LogicalNot_Bool: 
            { 
                auto* boolValue = GetBool(qInst.args[0]);
                auto* notValue = builder.CreateNot(boolValue);
                SetSlot(*qInst.oDest, notValue);
                return;
            }

            case UnaryMinus_Int:
            {
                auto* intValue = GetInt(qInst.args[0]);
                auto* negValue = builder.CreateNeg(intValue);
                SetSlot(*qInst.oDest, negValue);
                return;
            }

            case ToString_Bool: 
            { 
                auto* boolValue = GetBool(qInst.args[0]);
                EmitRuntimeCall(LRuntimeFuncKind::BoolToString, {slotValues[qInst.oDest->index], boolValue});
                return;
            }

            case ToString_Int:
            {
                auto* intValue = GetInt(qInst.args[0]);
                EmitRuntimeCall(LRuntimeFuncKind::IntToString, {slotValues[qInst.oDest->index], intValue});
                return;
            }

            case PrefixInc_Int: 
            { 
                // int* p;
                // p에서 값을 읽어들인다 => r
                // r을 하나 올린다
                // r을 p에 저장한다
                auto* ptrValue = GetPtr(qInst.args[0]);
                auto* loadedValue = builder.CreateLoad(lContextImpl.GetInt32Type(), ptrValue);
                auto* increasedValue = builder.CreateAdd(loadedValue, llvm::ConstantInt::get(lContextImpl.GetInt32Type(), 1));
                builder.CreateStore(increasedValue, ptrValue);
                SetSlot(*qInst.oDest, increasedValue);
                return;
            }

            case PrefixDec_Int:
            {
                auto* ptrValue = GetPtr(qInst.args[0]);
                auto* loadedValue = builder.CreateLoad(lContextImpl.GetInt32Type(), ptrValue);
                auto* decreasedValue = builder.CreateSub(loadedValue, llvm::ConstantInt::get(lContextImpl.GetInt32Type(), 1));
                builder.CreateStore(decreasedValue, ptrValue);
                SetSlot(*qInst.oDest, decreasedValue);
                return;
            }

            case PostfixInc_Int: 
            { 
                auto* ptrValue = GetPtr(qInst.args[0]);
                auto* loadedValue = builder.CreateLoad(lContextImpl.GetInt32Type(), ptrValue);
                auto* increasedValue = builder.CreateAdd(loadedValue, llvm::ConstantInt::get(lContextImpl.GetInt32Type(), 1));
                builder.CreateStore(increasedValue, ptrValue);
                SetSlot(*qInst.oDest, loadedValue);
                return;
            }

            case PostfixDec_Int: 
            {
                auto* ptrValue = GetPtr(qInst.args[0]);
                auto* loadedValue = builder.CreateLoad(lContextImpl.GetInt32Type(), ptrValue);
                auto* decreasedValue = builder.CreateSub(loadedValue, llvm::ConstantInt::get(lContextImpl.GetInt32Type(), 1));
                builder.CreateStore(decreasedValue, ptrValue);
                SetSlot(*qInst.oDest, loadedValue);
                return;
            }

            case Multiply_Int_Int: 
            { 
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateMul(operand0, operand1);
                SetSlot(*qInst.oDest, result);
                return;
            }

            case Divide_Int_Int:
            {
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateSDiv(operand0, operand1);
                SetSlot(*qInst.oDest, result);
                return;
            }

            case Modulo_Int_Int: 
            { 
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateSRem(operand0, operand1);
                SetSlot(*qInst.oDest, result);
                return;
            }

            case Add_Int_Int: 
            { 
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateAdd(operand0, operand1);
                SetSlot(*qInst.oDest, result);
                return;
            }

            case Add_String_String: 
            { 
                auto* str0 = GetStringRef(qInst.args[0]);
                auto* str1 = GetStringRef(qInst.args[1]);
                auto* result = slotValues[qInst.oDest->index];
                EmitRuntimeCall(LRuntimeFuncKind::StringConcat, {result, str0, str1});
                return;
            }
            
            case Subtract_Int_Int:
            {
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateSub(operand0, operand1);
                SetSlot(*qInst.oDest, result);
                return;
            }

            case LessThan_Int_Int: 
            {
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateICmpSLT(operand0, operand1);
                SetSlot(*qInst.oDest, result);
                return;
            }

            case LessThan_String_String: 
            { 
                auto* str0 = GetStringRef(qInst.args[0]);
                auto* str1 = GetStringRef(qInst.args[1]);                
                auto* result = EmitRuntimeCall(LRuntimeFuncKind::StringLessThan, {str0, str1});
                SetSlot(*qInst.oDest, result);
                return;
            }

            case GreaterThan_Int_Int: 
            { 
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateICmpSGT(operand0, operand1);
                SetSlot(*qInst.oDest, result);
                return;
            }

            case GreaterThan_String_String: 
            { 
                auto* str0 = GetStringRef(qInst.args[0]);
                auto* str1 = GetStringRef(qInst.args[1]);                
                auto* result = EmitRuntimeCall(LRuntimeFuncKind::StringGreaterThan, {str0, str1});
                SetSlot(*qInst.oDest, result);
                return;
            }

            case LessThanOrEqual_Int_Int: 
            { 
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateICmpSLE(operand0, operand1);
                SetSlot(*qInst.oDest, result);
                return;
            }

            case LessThanOrEqual_String_String:
            {
                auto* str0 = GetStringRef(qInst.args[0]);
                auto* str1 = GetStringRef(qInst.args[1]);                
                auto* result = EmitRuntimeCall(LRuntimeFuncKind::StringLessThanOrEqual, {str0, str1});
                SetSlot(*qInst.oDest, result);
                return;
            }

            case GreaterThanOrEqual_Int_Int: 
            { 
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateICmpSGE(operand0, operand1);
                SetSlot(*qInst.oDest, result);
                return;
            }

            case GreaterThanOrEqual_String_String: 
            { 
                auto* str0 = GetStringRef(qInst.args[0]);
                auto* str1 = GetStringRef(qInst.args[1]);                
                auto* result = EmitRuntimeCall(LRuntimeFuncKind::StringGreaterThanOrEqual, {str0, str1});
                SetSlot(*qInst.oDest, result);
                return;
            }

            case Equal_Int_Int: 
            {
                auto* operand0 = GetInt(qInst.args[0]);
                auto* operand1 = GetInt(qInst.args[1]);
                auto* result = builder.CreateICmpEQ(operand0, operand1);
                SetSlot(*qInst.oDest, result);
                return;
            }

            case Equal_Bool_Bool: 
            { 
                auto* operand0 = GetBool(qInst.args[0]);
                auto* operand1 = GetBool(qInst.args[1]);
                auto* result = builder.CreateICmpEQ(operand0, operand1);
                SetSlot(*qInst.oDest, result);
                return;
            }

            case Equal_String_String:
            {
                auto* str0 = GetStringRef(qInst.args[0]);
                auto* str1 = GetStringRef(qInst.args[1]);                
                auto* result = EmitRuntimeCall(LRuntimeFuncKind::StringEquals, {str0, str1});
                SetSlot(*qInst.oDest, result);
                return;
            }

            default: assert(false);
        }
    }

    void Emit(QInst& qInst)
    {
        visit([this](auto& qInst)
        {
            using T = remove_cvref_t<decltype(qInst)>;
            if constexpr (same_as<T, QInst_InitString>) // InitString{string& dest, std::string text}
            {
                auto* textPtr = builder.CreateGlobalString(qInst.text);
                EmitRuntimeCall(LRuntimeFuncKind::StringInit, {slotValues[qInst.slot.index], textPtr});
            }
            else if constexpr (same_as<T, QInst_DestroyString>)
            {
                EmitRuntimeCall(LRuntimeFuncKind::StringDestroy, {slotValues[qInst.slot.index]});
            }
            else if constexpr (same_as<T, QInst_Load>)
            {
                auto* lPtrValueType = lContextImpl.GetPtrType();
                auto* lPtrValue = builder.CreateLoad(lContextImpl.GetPtrType(), slotValues[qInst.src.index]);
                auto* lValueType = lContextImpl.GetType(qInst.type);
                auto* lValue = builder.CreateLoad(lValueType, lPtrValue);

                // 1:1변환이므로, 다시 slot에 저장한다
                builder.CreateStore(lValue, slotValues[qInst.dest.index]);
            }
            else if constexpr (same_as<T, QInst_Store>)
            {
                // source가 뭐냐에따라 달라진다
                auto* lValue = GetValue(qInst.src, qInst.type);
                auto* lPtrValue = builder.CreateLoad(lContextImpl.GetPtrType(), slotValues[qInst.dest.index]);
                builder.CreateStore(lValue, lPtrValue);
            }
            else if constexpr (same_as<T, QInst_AddrOf>)
            {   
                auto* lPtrValue = builder.CreateLoad(lContextImpl.GetPtrType(), slotValues[qInst.dest.index]);
                builder.CreateStore(slotValues[qInst.slot.index], lPtrValue);
            }
            else if constexpr (same_as<T, QInst_Assign>)
            {
                // source가 뭐냐에따라 달라진다
                auto* lValue = GetValue(qInst.src, qInst.type);
                builder.CreateStore(lValue, slotValues[qInst.dest.index]);
            }
            else if constexpr (same_as<T, QInst_Call>)
            {
                throw NotImplementedException{};
            }
            else if constexpr (same_as<T, QInst_Return>) 
            {
                if (!qInst.oValue)
                {
                    builder.CreateRetVoid();
                }
                else
                {
                    auto* v = GetValue(qInst.oValue->value, qInst.oValue->qType);
                    builder.CreateRet(v);
                }
            }
            else if constexpr (same_as<T, QInst_Intrinsic>)
            {
                EmitIntrinsic(qInst);
            }
            else if constexpr (same_as<T, QInst_CondJump>)
            {
                auto* lCond = builder.CreateLoad(lContextImpl.GetBoolType(), slotValues[qInst.cond.index]);
                builder.CreateCondBr(lCond, lBlocksByQBlock[qInst.trueBlock], lBlocksByQBlock[qInst.falseBlock]);
            }
            else if constexpr (same_as<T, QInst_Jump>)
            {
                builder.CreateBr(lBlocksByQBlock[qInst.block]);
            }
            else static_assert(false);
        }, qInst);
    }

public:
    void Translate()
    {
        auto rFuncReturn = qFuncBody.nFuncDecl->GetUnboundFuncReturn();
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
        for (auto& rFuncParam : qFuncBody.nFuncDecl->GetUnboundFuncParams())
        {
            auto* lParamType = lContextImpl.GetType(rFuncParam.type);
            lParamTypes.push_back(lParamType);
        }

        std::string name = RNameToString2(qFuncBody.nFuncDecl->GetNDecl()->GetRDecl()->GetIdentifier().name);

        llvm::FunctionType* lFuncType = llvm::FunctionType::get(lRetType, lParamTypes, /*isVarArg*/false);
        auto* lFunc = llvm::Function::Create(lFuncType, llvm::GlobalValue::LinkageTypes::ExternalLinkage, name, _module);

        auto* preludeBlock = llvm::BasicBlock::Create(lContextImpl.context, "prelude", lFunc);
        builder.SetInsertPoint(preludeBlock);
        
        slotValues.reserve(qFuncBody.slotInfos.size());

        // 함수 인자 이름 붙이기.
        for (auto& qSlotInfo : qFuncBody.slotInfos)
        {
            // 함수 초기에 할당

            if (qSlotInfo.oArgIndex) // 아규먼트에 매핑 되어있으면,
            {
                auto* lArg = lFunc->getArg(*qSlotInfo.oArgIndex);
                lArg->setName(qSlotInfo.name.substr(1) + "_arg");
                auto* lType = lContextImpl.GetType(qSlotInfo.qType);
                auto* allocaInst = builder.CreateAlloca(lType, nullptr, qSlotInfo.name.substr(1));

                builder.CreateStore(lArg, allocaInst);
                slotValues.push_back(allocaInst);
            }
            else
            {
                // 함수 초기에 할당
                auto* lType = lContextImpl.GetType(qSlotInfo.qType);
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
    lModuleContext.InitRuntimeFuncs();

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