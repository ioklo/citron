#include "QEvaluation.h"

#include <ranges>
#include <iostream>
#include <unordered_map>
#include <variant>
#include <cassert>

#include "Infra/Variants.h"
#include "Infra/Exceptions.h"

#include "Logging/Diag.h"

#include "RSymbol/RModule.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RGlobalFuncDecl.h"
#include "RSymbol/RClassCtorDecl.h"
#include "RSymbol/RClassFuncDecl.h"
#include "RSymbol/RStructCtorDecl.h"
#include "RSymbol/RStructDtorDecl.h"
#include "RSymbol/RStructFuncDecl.h"
#include "RSymbol/RLambdaDecl.h"

#include "QIR/QData.h"
#include "QIR/QFuncBody.h"
#include "QIR/QBlock.h"

using namespace std;

namespace Citron {

namespace {
struct InstructionPointer
{
    QBlock* block;
    int index;

    InstructionPointer() : block{nullptr}, index{0} 
    {
    }

    InstructionPointer(QBlock* block, int index) : block{block}, index{index}
    {
    }
};

struct Slot
{
    void* ptr;

    template<typename T>
    T& Get() { return *(T*)ptr; }

    template<typename T>
    void Set(T& t) { *(T*)ptr = t; }

    template<typename T>
    void Set(T&& t) { *(T*)ptr = std::forward<T>(t); }

    void* GetAddr() { return ptr; }
};

struct StackFrame
{
    QFuncBody* qFuncBody;
    InstructionPointer ip;
    std::vector<Slot> slots; // 스택에 저장하는 공간, stackPointer위의 어디를 가리킨다
    byte* stackPointer;
    std::optional<Slot> o_retSlot; // 리턴용 저장공간

    StackFrame()
        : qFuncBody{nullptr}, ip{}, stackPointer{nullptr}, o_retSlot{nullopt}
    { }
    StackFrame(StackFrame&& other) = default;
};

struct Environment
{   
    QData* qData; // 임시
    IEvalQDataCommandHandlerPtr cmdHandler;
    std::vector<byte> stack;
    StackFrame* curFrame;

    // stack frame, 굳이 frame을 스택에 넣지 않는다
    vector<StackFrame> frames;
};

size_t GetSize(RType* type, RFactory& rFactory)
{   
    // TODO: HARD CODED
    if (dynamic_cast<RType_Ptr*>(type))
        return sizeof(void*);

    if (type == rFactory.MakeBoolType())
        return 4;

    if (type == rFactory.MakeIntType())
        return 4;

    if (type == rFactory.MakeStringType())
        return sizeof(string); // TODO: 임시
    
    throw NotImplementedException{};
}

// GetLoc은 value자체의 메모리 상의 위치를 
// GetPtr은 value가 갖고 있는 값이 ptr인 경우에 그 값을 반환

// QArg_Addr
// QArg_Value
// QArg_CallArg
// QArg_Dest
// size_t 

template<typename T> requires std::is_pointer_v<T>
T Get(QArg_Addr& arg, Environment& env)
{
    return visit([&env](auto& arg) -> T {
        using U = remove_cvref_t<decltype(arg)>;
        if constexpr (same_as<U, QArg_Addr_OfSlot>) return (T)env.curFrame->slots[arg.index].GetAddr();
        else if constexpr (same_as<U, QArg_Addr_PtrSlot>) return env.curFrame->slots[arg.index].Get<T>();
        else static_assert(false);
    }, arg);
}

template<typename T>
T Get(QArg_Value& arg, Environment& env)
{
    return visit([&env](auto& arg) -> T {
        using U = remove_cvref_t<decltype(arg)>;
        if constexpr (same_as<U, QArg_Value_ConstInt32>)
        {
            if constexpr (same_as<T, int>) return arg.value;
            else throw RuntimeFatalException{};
        }
        else if constexpr (same_as<U, QArg_Value_ConstBool>)
        {
            if constexpr (same_as<T, bool>) return arg.value;
            else throw RuntimeFatalException{};
        }
        else if constexpr (same_as<U, QArg_Value_Slot>) return env.curFrame->slots[arg.index].Get<T>();
        else throw NotImplementedException{};
    }, arg);
}

template<typename T>
T Get(QArg_CallArg& arg, Environment& env)
{
    return visit([&env](auto& arg) -> T {
        using U = remove_cvref_t<decltype(arg)>;
        if constexpr (same_as<U, QArg_CallArg_ConstInt32>)
        {
            if constexpr (same_as<T, int>) return arg.value;
            else throw RuntimeFatalException{};
        }
        else if constexpr (same_as<U, QArg_CallArg_ConstBool>)
        {
            if constexpr (same_as<T, bool>) return arg.value;
            else throw RuntimeFatalException{};
        }
        else if constexpr (same_as<U, QArg_CallArg_Slot>)
            return env.curFrame->slots[arg.index].Get<T>();
        else if constexpr (same_as<U, QArg_CallArg_DerefPtrSlot>)
            return *env.curFrame->slots[arg.index].Get<T*>();
        else if constexpr (same_as<U, QArg_CallArg_AddrOfSlot>)
        {
            if constexpr (is_pointer_v<T>)
                return (T)env.curFrame->slots[arg.index].GetAddr();
            else
                throw RuntimeFatalException{};
        }
        else static_assert(false);
    }, arg);
}

template<typename T>
T Get(size_t index, Environment& env)
{
    return env.curFrame->slots[index].Get<T>();
}

template<typename T, typename U>
void Set(QArg_Dest& dest, U&& value, Environment& env)
{
    visit([&env, &value](auto& dest) {
        using T = remove_cvref_t<decltype(dest)>;
        if constexpr (same_as<T, QArg_Dest_Slot>) env.curFrame->slots[dest.index].Set(std::forward<U>(value));
        else if constexpr (same_as<T, QArg_Dest_DirectReturn>) env.curFrame->o_retSlot->Set(std::forward<U>(value));
        else static_assert(false);
    }, dest);
}

Slot GetSlot(QArg_Dest& dest, Environment& env)
{
    return visit([&env](auto& dest) -> Slot {
        using T = remove_cvref_t<decltype(dest)>;
        if constexpr (same_as<T, QArg_Dest_Slot>) return env.curFrame->slots[dest.index];
        else if constexpr (same_as<T, QArg_Dest_DirectReturn>)
        {
            assert(env.curFrame->o_retSlot);
            return *env.curFrame->o_retSlot;
        }
        else static_assert(false);
    }, dest);
}

void EvalIntrinsic(QInst_Intrinsic& inst, Environment& env)
{
    using enum QInst_IntrinsicKind;

    switch (inst.kind)
    {
    case Command_Item:
    {   
        auto* s = Get<string*>(inst.args[0], env);
        env.cmdHandler->Execute(*s);
        return;
    }

    case Alloc_Int: throw NotImplementedException{};

    // 여기가 GetPtr인지
    case Memcpy_Void_Ptr_Ptr_Int:
    {
        auto* dest = Get<void*>(inst.args[0], env);
        auto* src = Get<void*>(inst.args[1], env);
        auto size = Get<int>(inst.args[2], env);
        memcpy(dest, src, size);
        return;
    }

    case NewList_Items: throw NotImplementedException{};
    case GetIterator_ListPtr_ListIterator: throw NotImplementedException{};
    case LogicalNot_Bool_Bool:
    {
        auto b = Get<bool>(inst.args[0], env);
        Set<bool>(*inst.o_dest, !b, env);
        return;
    }

    case UnaryMinus_Int_Int:
    {
        auto i = Get<int>(inst.args[0], env);
        Set<int>(*inst.o_dest, -i, env);
        return;
    }

    case ToString_String_Bool:
    {
        string* ret = Get<string*>(inst.args[0], env);
        auto b = Get<bool>(inst.args[1], env);
        *ret = format("{}", b);
        return;
    }

    case ToString_String_Int:
    {
        string* ret = Get<string*>(inst.args[0], env);
        auto i = Get<int>(inst.args[1], env);
        *ret = format("{}", i);
        return;
    }

    case PrefixInc_Int_IntRef:
    {
        // ++i

        // 인자는 location
        int* ptr = Get<int*>(inst.args[0], env);
        Set<int>(*inst.o_dest, ++(*ptr), env);
        return;
    }

    case PrefixDec_Int_IntRef:
    {
        // --i

        // 인자는 location
        int* ptr = Get<int*>(inst.args[0], env);
        Set<int>(*inst.o_dest, --(*ptr), env);
        return;
    }

    case PostfixInc_Int_IntRef:
    {
        // i++

        // 인자는 location
        int* ptr = Get<int*>(inst.args[0], env);
        Set<int>(*inst.o_dest, (*ptr)++, env);
        return;
    }
    case PostfixDec_Int_IntRef:
    {
        // i--
        // 인자는 location
        int* ptr = Get<int*>(inst.args[0], env);
        Set<int>(*inst.o_dest, (*ptr)--, env);
        return;
    }

    case Multiply_Int_Int_Int:
    {
        auto i1 = Get<int>(inst.args[0], env);
        auto i2 = Get<int>(inst.args[1], env);
        Set<int>(*inst.o_dest, i1 * i2, env);
        return;
    }
    case Divide_Int_Int_Int:
    {
        auto i1 = Get<int>(inst.args[0], env);
        auto i2 = Get<int>(inst.args[1], env);
        Set<int>(*inst.o_dest, i1 / i2, env);
        return;
    }

    case Modulo_Int_Int_Int:
    {
        auto i1 = Get<int>(inst.args[0], env);
        auto i2 = Get<int>(inst.args[1], env);
        Set<int>(*inst.o_dest, i1 % i2, env);
        return;
    }

    case Add_Int_Int_Int:
    {
        auto i1 = Get<int>(inst.args[0], env);
        auto i2 = Get<int>(inst.args[1], env);
        Set<int>(*inst.o_dest, i1 + i2, env);
        return;
    }

    case Add_String_StringInRef_StringInRef:
    {
        auto* ret = Get<string*>(inst.args[0], env);
        auto* s1 = Get<string*>(inst.args[1], env);
        auto* s2 = Get<string*>(inst.args[2], env);
        *ret = *s1 + *s2;
        return;
    }

    case Subtract_Int_Int_Int:
    {
        auto i1 = Get<int>(inst.args[0], env);
        auto i2 = Get<int>(inst.args[1], env);
        Set<int>(*inst.o_dest, i1 - i2, env);
        return;
    }

    case LessThan_Bool_Int_Int:
    {
        // const integer가 있으면,
        auto i1 = Get<int>(inst.args[0], env);
        auto i2 = Get<int>(inst.args[1], env);

        Set<bool>(*inst.o_dest, i1 < i2, env);
        return;
    }

    case LessThan_Bool_StringInRef_StringInRef:
    {
        auto* s1 = Get<string*>(inst.args[0], env);
        auto* s2 = Get<string*>(inst.args[1], env);

        Set<bool>(*inst.o_dest, *s1 < *s2, env);
        return;
    }

    case GreaterThan_Bool_Int_Int:
    {
        // const integer가 있으면,
        auto i1 = Get<int>(inst.args[0], env);
        auto i2 = Get<int>(inst.args[1], env);
        Set<bool>(*inst.o_dest, i1 > i2, env);
        return;
    }

    case GreaterThan_Bool_StringInRef_StringInRef:
    {
        auto* s1 = Get<string*>(inst.args[0], env);
        auto* s2 = Get<string*>(inst.args[1], env);

        Set<bool>(*inst.o_dest, *s1 > *s2, env);
        return;
    }

    case LessThanOrEqual_Bool_Int_Int:
    {
        auto i1 = Get<int>(inst.args[0], env);
        auto i2 = Get<int>(inst.args[1], env);
        Set<bool>(*inst.o_dest, i1 <= i2, env);
        return;
    }
    case LessThanOrEqual_Bool_StringInRef_StringInRef:
    {
        auto* s1 = Get<string*>(inst.args[0], env);
        auto* s2 = Get<string*>(inst.args[1], env);

        Set<bool>(*inst.o_dest, *s1 <= *s2, env);
        return;
    }

    case GreaterThanOrEqual_Bool_Int_Int:
    {
        auto i1 = Get<int>(inst.args[0], env);
        auto i2 = Get<int>(inst.args[1], env);
        Set<bool>(*inst.o_dest, i1 >= i2, env);
        return;
    }

    case GreaterThanOrEqual_Bool_StringInRef_StringInRef:
    {
        auto* s1 = Get<string*>(inst.args[0], env);
        auto* s2 = Get<string*>(inst.args[1], env);

        Set<bool>(*inst.o_dest, *s1 >= *s2, env);
        return;
    }

    case Equal_Bool_Int_Int:
    {
        // const integer가 있으면,
        auto i1 = Get<int>(inst.args[0], env);
        auto i2 = Get<int>(inst.args[1], env);

        Set<bool>(*inst.o_dest, i1 == i2, env);
        return;
    }

    case Equal_Bool_Bool_Bool:
    {
        auto b1 = Get<bool>(inst.args[0], env);
        auto b2 = Get<bool>(inst.args[1], env);

        Set<bool>(*inst.o_dest, b1 == b2, env);
        return;
    }

    case Equal_Bool_StringInRef_StringInRef:
    {
        auto* s1 = Get<string*>(inst.args[0], env);
        auto* s2 = Get<string*>(inst.args[1], env);
        Set<bool>(*inst.o_dest, *s1 == *s2, env);
        return;
    }

    case QInst_IntrinsicKind::CopyCtor_Void_StringRef_StringInRef:
    {
        auto* thisPtr = Get<string*>(inst.args[0], env);
        string* otherPtr = Get<string*>(inst.args[1], env);

        new (thisPtr) string{*otherPtr};
        return;
    }

    case QInst_IntrinsicKind::MoveCtor_Void_StringRef_StringMoveRef:
    {
        auto* thisPtr = Get<string*>(inst.args[0], env);
        string* otherPtr = Get<string*>(inst.args[1], env);

        new (thisPtr) string{std::move(*otherPtr)};
        return;
    }
    
    case QInst_IntrinsicKind::Dtor_Void_StringRef:
    {
        string* thisPtr = Get<string*>(inst.args[0], env);
        (*thisPtr).~string();
        return;
    }

    case QInst_IntrinsicKind::CopyAssign_Void_StringRef_StringInRef:
    {
        string* destPtr = Get<string*>(inst.args[0], env);
        string* srcPtr = Get<string*>(inst.args[1], env);
        *destPtr = *srcPtr;
        return;
    }

    case QInst_IntrinsicKind::MoveAssign_Void_StringRef_StringMoveRef:
    {
        string* destPtr = Get<string*>(inst.args[0], env);
        string* srcPtr = Get<string*>(inst.args[1], env);
        *destPtr = std::move(*srcPtr);
        return;
    }

    case QInst_IntrinsicKind::Max: unreachable();

    }

    unreachable();
}

StackFrame MakeStackFrame(QFuncBody* qFuncBody, StackFrame& curFrame, optional<QArg_Dest> o_dest, span<QArg_CallArg> args, RFactory& rFactory)
{
    StackFrame frame;

    frame.qFuncBody = qFuncBody;
    frame.ip = InstructionPointer{qFuncBody->blocks.front(), 0},
    frame.stackPointer = curFrame.stackPointer;
    frame.slots.resize(qFuncBody->slotInfos.size());

    if (o_dest)
    {
        visit([&curFrame, &frame](auto& dest) {
            using T = remove_cvref_t<decltype(dest)>;
            if constexpr (same_as<T, QArg_Dest_Slot>)
            {
                frame.o_retSlot = curFrame.slots[dest.index];
            }
            else if constexpr (same_as<T, QArg_Dest_DirectReturn>)
            {
                frame.o_retSlot = curFrame.o_retSlot;
            }
        }, *o_dest);
    }
    else
    {
        frame.o_retSlot = nullopt;
    }
    
    // 그냥 0번부터 쭉 argument 복사하면 된다
    size_t argCount = args.size();
    for (size_t i = 0; i < argCount; i++)
    {
        visit([i, qFuncBody, &curFrame, &frame, &rFactory](auto& arg) {
            using T = remove_cvref_t<decltype(arg)>;
            if constexpr (same_as<T, QArg_CallArg_Slot>)
            {
                // 타입정보는 qFunc로부터
                size_t size = GetSize(qFuncBody->slotInfos[i].type, rFactory);

                frame.stackPointer -= size;
                frame.slots[i] = Slot{frame.stackPointer};

                // 복사
                memcpy(frame.slots[i].GetAddr(), curFrame.slots[arg.index].GetAddr(), size);
            }
            else if constexpr (same_as<T, QArg_CallArg_DerefPtrSlot>)
            {
                size_t size = GetSize(qFuncBody->slotInfos[i].type, rFactory);
                frame.stackPointer -= size;
                frame.slots[i] = Slot{frame.stackPointer};

                void* pSrc = curFrame.slots[arg.index].Get<void*>();
                memcpy(frame.slots[i].GetAddr(), pSrc, size);
            }
            else if constexpr (same_as<T, QArg_CallArg_ConstBool>)
            {
                size_t size = GetSize(rFactory.MakeBoolType(), rFactory);
                frame.stackPointer -= size;
                frame.slots[i] = Slot{frame.stackPointer};
    
                frame.slots[i].Set<bool>(arg.value);
            }
            else if constexpr (same_as<T, QArg_CallArg_ConstInt32>)
            {
                size_t size = GetSize(rFactory.MakeIntType(), rFactory);
                frame.stackPointer -= size;
                frame.slots[i] = Slot{frame.stackPointer};
    
                frame.slots[i].Set<int>(arg.value);
            }
            else if constexpr (same_as<T, QArg_CallArg_AddrOfSlot>)
            {
                size_t size = GetSize(rFactory.MakePtrType(rFactory.MakeVoidType()), rFactory);
                frame.stackPointer -= size;
                frame.slots[i] = Slot{frame.stackPointer};
    
                frame.slots[i].Set<void*>(curFrame.slots[arg.index].GetAddr());
            }
            else static_assert(false);
        }, args[i]);
    }
    
    for (size_t i = argCount, slotCount = qFuncBody->slotInfos.size(); i < slotCount; i++)
    {
        auto& slot = qFuncBody->slotInfos[i];

        size_t size = GetSize(slot.type, rFactory);
        frame.stackPointer -= size;
        frame.slots[i].ptr = frame.stackPointer;

        // TODO: [61] 일반적인 struct ctor, dtor, copy/move ctor, copy/move assign 구현
        if (slot.type == rFactory.MakeStringType())
        {
            new (frame.slots[i].ptr) string{};
        }
    }

    return frame;
}

struct Evaluator
{
    Environment& env;
    RFactoryPtr rFactory;

    bool operator()(auto& inst) { return Eval(inst); }

    bool Eval(QInst_Ctor_String& inst)
    {
        auto* buf = Get<string*>(inst._this, env);
        new (buf) string{inst.text};
        return true;
    }

    bool Eval(QInst_Load& inst)
    {
        // dest <- *src;

        // src는 포인터 값을 갖고 있다
               
        auto destSlot = GetSlot(inst.dest, env);
        void* src = Get<void*>(inst.src.index, env);
        size_t size = GetSize(inst.type, *rFactory);

        memcpy(destSlot.GetAddr(), src, size);
        return true;
    }

    bool Eval(QInst_Store& inst)
    {
        // *dest = value;
        void* dest = Get<void*>(inst.dest.index, env);

        visit([this, dest, type = inst.type](auto& src)
        {
            using T = remove_cvref_t<decltype(src)>;
            if constexpr (same_as<T, QArg_Value_ConstBool>)
            {
                *(bool*)dest = src.value;
            }
            else if constexpr (same_as <T, QArg_Value_ConstInt32>)
            {
                *(int*)dest = src.value;
            }
            else if constexpr (same_as<T, QArg_Value_Slot>)
            {
                void* pSrc = env.curFrame->slots[src.index].GetAddr();
                size_t size = GetSize(type, *rFactory);
                memcpy(dest, pSrc, size);
            }
            else static_assert(false);
        }, inst.src);

        return true;
    }

    bool Eval(QInst_AddrOf& inst)
    {
        void* addr = env.curFrame->slots[inst.slot].GetAddr();
        Set<void*>(inst.dest, addr, env);
        return true;
    }

    bool Eval(QInst_FieldOf& inst)
    {
        throw NotImplementedException{};
    }

    bool Eval(QInst_Assign& inst)
    {
        // %dest = %src
        // %r2 = %r1: memcpy(&regValues[r1.index], &regValues[r2.index], size) // void* 복사, size는 8보다 작을 것이다
        // %s2 = %s1: memcpy(slots[s1.index], slots[s2.index], size)

        Slot& destSlot = visit([this](auto& dest) -> Slot& {
            using T = remove_cvref_t<decltype(dest)>;
            if constexpr (same_as<T, QArg_Dest_Slot>)
            {
                return env.curFrame->slots[dest.index];
            }
            else if constexpr (same_as<T, QArg_Dest_DirectReturn>)
            {
                return *env.curFrame->o_retSlot;
            }
            else static_assert(false);
        }, inst.dest);

        visit([this, &destSlot, type = inst.type](auto& src) {
            using T = remove_cvref_t<decltype(src)>;
            if constexpr (same_as<T, QArg_Value_ConstBool>)
            {
                destSlot.Set<bool>(src.value);
            }
            else if constexpr (same_as < T, QArg_Value_ConstInt32>)
            {
                destSlot.Set<int>(src.value);
            }
            else if constexpr (same_as<T, QArg_Value_Slot>)
            {   
                size_t size = GetSize(type, *rFactory);
                memcpy(destSlot.GetAddr(), env.curFrame->slots[src.index].GetAddr(), size);
            }
            else static_assert(false);
        }, inst.src);

        return true;
    }

    bool Eval(QInst_Call& inst)
    {
        // TODO: linker가 미리 어떻게 할지 알렸어야 한다

        // 여기서는 RFuncDecl이 NFuncDecl이고
        // RFuncDecl -> RGlobalFuncDecl -> NGlobalFuncDecl
        auto bodies = env.qData->GetAllBodies();
        auto i = ranges::find_if(bodies, [&inst](QFuncBody& body) { return body.rFuncDecl == inst.rFuncDecl; });
        if (i == bodies.end()) throw NotImplementedException{};

        auto frame = MakeStackFrame(&*i, *env.curFrame, inst.o_dest, inst.args, *rFactory);
        env.frames.push_back(move(frame));
        env.curFrame = &env.frames.back();
        return true;
    }

    bool Eval(QInst_Intrinsic& inst)
    { 
        EvalIntrinsic(inst, env);
        return true; 
    }
     
    bool Eval(QInst_Return& inst) 
    {   
        env.frames.pop_back();
        if (env.frames.empty()) return false;
        env.curFrame = &env.frames.back();
        return true;
    }

    bool Eval(QInst_CondJump& condJump)
    {
        bool cond = Get<bool>(condJump.cond.index, env);

        if (cond)
            env.curFrame->ip = InstructionPointer{condJump.trueBlock, 0};
        else
            env.curFrame->ip = InstructionPointer{condJump.falseBlock, 0};

        return true;
    }

    bool Eval(QInst_Jump& jump)
    {
        env.curFrame->ip = InstructionPointer{jump.block, 0};
        return true;
    }
};

bool Evaluate(QInst& inst, Environment& env, TakeRef<RFactoryPtr> rFactory)
{
    return visit(Evaluator{env, rFactory.Take()}, inst);
}

} // namespace 
expected<void, DiagPtr> EvaluateQData(span<RModule*> rModules, QData* qData, RGlobalFuncDecl* nEntry, IEvalQDataCommandHandlerPtr&& cmdHandler, InRef<RFactoryPtr> rFactory)
{
    auto bodies = qData->GetAllBodies();
    auto i = ranges::find_if(bodies, [nEntry](QFuncBody& body) { return body.rFuncDecl == nEntry; });
    if (i == bodies.end())
    {
        throw NotImplementedException{}; // 에러 처리
        return unexpected{nullptr};
    }

    Environment env;
    env.qData = qData;
    env.cmdHandler = move(cmdHandler);
    env.stack.resize(1024 * 1024);

    env.frames.push_back(StackFrame{});
    env.curFrame = &env.frames.back();


    env.curFrame->qFuncBody = &*i;
    env.curFrame->ip = InstructionPointer{i->blocks.front(), 0},
    env.curFrame->stackPointer = env.stack.data() + env.stack.size();
    env.curFrame->o_retSlot = nullopt;
    env.curFrame->slots.resize(i->slotInfos.size());
    for (size_t j = 0, count = i->slotInfos.size(); j < count; j++)
    {
        auto& slot = i->slotInfos[j];
        size_t size = GetSize(slot.type, **rFactory);
        env.curFrame->stackPointer -= size;
        env.curFrame->slots[j].ptr = env.curFrame->stackPointer;
    }

    while(true)
    {
        auto& inst = env.curFrame->ip.block->GetInst(env.curFrame->ip.index++);

        bool cont = Evaluate(inst, env, *rFactory);

        if (!cont) break;
    }

    return {};
}

} // namespace Citron