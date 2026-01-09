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
#include "NSymbol/NGlobalFuncDecl.h"
#include "QIR/QData.h"
#include "QIR/QFuncBody.h"
#include "QIR/QBlock.h"
#include "QIR/QFactory.h"


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

struct StackFrame
{
    QFuncBody* qFuncBody;
    InstructionPointer ip;
    std::vector<void*> slots; // 스택에 저장하는 공간
    byte* stackPointer;
    void* retSlot; // 리턴용 저장공간

    StackFrame()
        : qFuncBody{nullptr}, ip{}, stackPointer{nullptr}, retSlot{nullptr}
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

size_t GetSize(QType* type, QFactory& qFactory)
{   
    // TODO: HARD CODED
    if (type == qFactory.MakePtrType())
        return sizeof(void*);

    if (type == qFactory.MakeBoolType())
        return 4;

    if (type == qFactory.MakeIntType())
        return 4;

    if (type == qFactory.MakeStringType())
        return sizeof(string); // TODO: 임시
    
    throw NotImplementedException{};
}

// GetLoc은 value자체의 메모리 상의 위치를 
// GetPtr은 value가 갖고 있는 값이 ptr인 경우에 그 값을 반환

// Ptr타입의 값
void* GetPtr(QArg_Input& src, Environment& env)
{
    return visit([&env](auto& src) -> void* {
        using T = remove_cvref_t<decltype(src)>;
        if constexpr (same_as<T, QArg_Slot>) return *(void**)env.curFrame->slots[src.index];
        else throw NotImplementedException{};
    }, src);
}

void* GetPtr(QArg_Slot& slot, Environment& env)
{
    return *(void**)env.curFrame->slots[slot.index];
}

// slot이 가리키고 있는 값의 위치
void* GetLoc(QArg_Slot& slot, Environment& env)
{
    return env.curFrame->slots[slot.index];
}

// inplace 값의 위치를 나타내는
//void* GetLoc(QArg& arg, Environment& env)
//{
//    return visit(overloaded{
//        [&env](QArg_StackSlot& slot) { return env.slots[slot.index]; },
//        [&env](QArg_ConstBool& cb) { return (void*)&cb.value; },
//        [&env](QArg_ConstInt32& ci) { return (void*)&ci.value; },
//        [](auto&&) -> void* { throw NotImplementedException{}; }
//    }, arg);
//}

int GetInt(QArg_Input& arg, Environment& env)
{
    return visit([&env](auto& arg) -> int {
        using T = remove_cvref_t<decltype(arg)>;
        if constexpr (same_as<T, QArg_ConstInt32>) return arg.value;
        else if constexpr (same_as<T, QArg_Slot>) return *(int*)env.curFrame->slots[arg.index];
        else throw NotImplementedException{};
    }, arg);
}

void SetInt(QArg_Slot& slot, int v, Environment& env)
{
    *(int*)env.curFrame->slots[slot.index] = v;
}

bool GetBool(QArg_Input& arg, Environment& env)
{
    return visit([&env](auto& arg) -> bool {
        using T = remove_cvref_t<decltype(arg)>;
        if constexpr (same_as<T, QArg_ConstBool>) return arg.value;
        else if constexpr (same_as<T, QArg_Slot>) return *(bool*)env.curFrame->slots[arg.index];
        else throw NotImplementedException{};
    }, arg);
}

bool GetBool(QArg_Slot& slot, Environment& env)
{
    return *(bool*)env.curFrame->slots[slot.index];
}

//
void SetBool(QArg_Slot& slot, bool v, Environment& env)
{
    *(bool*)env.curFrame->slots[slot.index] = v;
}

string& GetStringRef(QArg_Slot& slot, Environment& env)
{
    return *(string*)env.curFrame->slots[slot.index];
}

string& GetStringRef(QArg_Input& arg, Environment& env)
{
    return visit([&env](auto& arg) -> string& {        
        using T = remove_cvref_t<decltype(arg)>;
        if constexpr (same_as<T, QArg_Slot>) return *(string*)env.curFrame->slots[arg.index];
        else unreachable();
    }, arg);
}

void SetString(QArg_Slot& slot, string&& s, Environment& env)
{   
    *(string*)env.curFrame->slots[slot.index] = move(s);
}

//
//RawValue GetRawValue(QValue& value, Environment& env)
//{
//    return visit<RawValue>(overloaded{
//        [&env](QValue_Named& named) { return env.namedValues[named.name]; },
//        [&env](QValue_ConstBool& cb) { return RawValue_Bool{cb.value}; },
//        [&env](QValue_ConstInteger& ci) { return RawValue_Int{ci.value}; },
//        [&env](QValue_String& s) { throw NotImplementedException{}; return RawValue_Int{0}; }
//    }, value);
//}
//
//void SetRawValue(QValue& value, RawValue& rawValue, Environment& env)
//{
//    return visit(overloaded{
//        [&env, &rawValue](QValue_Named& named) { env.namedValues[named.name] = rawValue; },
//        [](auto&) { throw NotImplementedException{}; }
//    }, value);
//}

void EvalIntrinsic(QInst_Intrinsic& inst, Environment& env)
{
    using enum QInst_IntrinsicKind;

    switch (inst.kind)
    {
    case Command_Items:
    {
        for (auto& arg : inst.args)
        {
            // string이라면, 크기가 8을 넘으므로
            visit([&env](auto& arg) {

                using T = remove_cvref_t<decltype(arg)>;

                if constexpr (same_as<T, QArg_Slot>)
                {
                    auto* s = (string*)env.curFrame->slots[arg.index];
                    env.cmdHandler->Execute(*s);
                }
                else throw NotImplementedException{};
            }, arg);
        }
        return;
    }

    case Alloc_Int: throw NotImplementedException{};
    case Memcpy_Ptr_Ptr_Int:
    {
        auto* dest = GetPtr(inst.args[0], env);
        auto* src = GetPtr(inst.args[1], env);
        auto size = GetInt(inst.args[2], env);
        memcpy(dest, src, size);
        return;
    }

    case NewList_Items: throw NotImplementedException{};
    case GetListIterator_List: throw NotImplementedException{};
    case LogicalNot_Bool:
    {
        auto b = GetBool(inst.args[0], env);
        SetBool(*inst.o_dest, !b, env);
        return;
    }

    case UnaryMinus_Int:
    {
        auto i = GetInt(inst.args[0], env);
        SetInt(*inst.o_dest, -i, env);
        return;
    }

    case ToString_Bool:
    {
        auto b = GetBool(inst.args[0], env);
        SetString(*inst.o_dest, format("{}", b), env);
        return;
    }


    case ToString_Int:
    {
        auto i = GetInt(inst.args[0], env);
        SetString(*inst.o_dest, format("{}", i), env);
        return;
    }

    case PrefixInc_Int:
    {
        // ++i

        // 인자는 location
        int* ptr = (int*)GetPtr(inst.args[0], env);
        SetInt(*inst.o_dest, ++(*ptr), env);
        return;
    }

    case PrefixDec_Int:
    {
        // --i

        // 인자는 location
        int* ptr = (int*)GetPtr(inst.args[0], env);
        SetInt(*inst.o_dest, --(*ptr), env);
        return;
    }
    case PostfixInc_Int:
    {
        // i++

        // 인자는 location
        int* ptr = (int*)GetPtr(inst.args[0], env);
        SetInt(*inst.o_dest, (*ptr)++, env);
        return;
    }
    case PostfixDec_Int:
    {
        // i--
        // 인자는 location
        int* ptr = (int*)GetPtr(inst.args[0], env);
        SetInt(*inst.o_dest, (*ptr)--, env);
        return;
    }

    case Multiply_Int_Int:
    {
        auto i1 = GetInt(inst.args[0], env);
        auto i2 = GetInt(inst.args[1], env);
        SetInt(*inst.o_dest, i1 * i2, env);
        return;
    }
    case Divide_Int_Int:
    {
        auto i1 = GetInt(inst.args[0], env);
        auto i2 = GetInt(inst.args[1], env);
        SetInt(*inst.o_dest, i1 / i2, env);
        return;
    }

    case Modulo_Int_Int:
    {
        auto i1 = GetInt(inst.args[0], env);
        auto i2 = GetInt(inst.args[1], env);
        SetInt(*inst.o_dest, i1 % i2, env);
        return;
    }
    case Add_Int_Int:
    {
        auto i1 = GetInt(inst.args[0], env);
        auto i2 = GetInt(inst.args[1], env);
        SetInt(*inst.o_dest, i1 + i2, env);
        return;
    }

    case Add_String_String:
    {
        auto& s1 = GetStringRef(inst.args[0], env);
        auto& s2 = GetStringRef(inst.args[1], env);
        SetString(*inst.o_dest, s1 + s2, env);
        return;
    }

    case Subtract_Int_Int:
    {
        auto i1 = GetInt(inst.args[0], env);
        auto i2 = GetInt(inst.args[1], env);
        SetInt(*inst.o_dest, i1 - i2, env);
        return;
    }

    case LessThan_Int_Int:
    {
        // const integer가 있으면,
        auto i1 = GetInt(inst.args[0], env);
        auto i2 = GetInt(inst.args[1], env);

        SetBool(*inst.o_dest, i1 < i2, env);
        return;
    }

    case LessThan_String_String:
    {
        auto& s1 = GetStringRef(inst.args[0], env);
        auto& s2 = GetStringRef(inst.args[1], env);

        SetBool(*inst.o_dest, s1 < s2, env);
        return;
    }

    case GreaterThan_Int_Int:
    {
        // const integer가 있으면,
        auto i1 = GetInt(inst.args[0], env);
        auto i2 = GetInt(inst.args[1], env);

        SetBool(*inst.o_dest, i1 > i2, env);
        return;
    }

    case GreaterThan_String_String:
    {
        auto& s1 = GetStringRef(inst.args[0], env);
        auto& s2 = GetStringRef(inst.args[1], env);

        SetBool(*inst.o_dest, s1 > s2, env);
        return;
    }

    case LessThanOrEqual_Int_Int:
    {
        auto i1 = GetInt(inst.args[0], env);
        auto i2 = GetInt(inst.args[1], env);
        SetBool(*inst.o_dest, i1 <= i2, env);
        return;
    }
    case LessThanOrEqual_String_String:
    {
        auto& s1 = GetStringRef(inst.args[0], env);
        auto& s2 = GetStringRef(inst.args[1], env);

        SetBool(*inst.o_dest, s1 <= s2, env);
        return;
    }

    case GreaterThanOrEqual_Int_Int:
    {
        auto i1 = GetInt(inst.args[0], env);
        auto i2 = GetInt(inst.args[1], env);
        SetBool(*inst.o_dest, i1 >= i2, env);
        return;
    }

    case GreaterThanOrEqual_String_String:
    {
        auto& s1 = GetStringRef(inst.args[0], env);
        auto& s2 = GetStringRef(inst.args[1], env);

        SetBool(*inst.o_dest, s1 >= s2, env);
        return;
    }

    case Equal_Int_Int:
    {
        // const integer가 있으면,
        auto i1 = GetInt(inst.args[0], env);
        auto i2 = GetInt(inst.args[1], env);

        SetBool(*inst.o_dest, i1 == i2, env);
        return;
    }

    case Equal_Bool_Bool:
    {
        auto b1 = GetBool(inst.args[0], env);
        auto b2 = GetBool(inst.args[1], env);

        SetBool(*inst.o_dest, b1 == b2, env);
        return;
    }

    case Equal_String_String:
    {
        auto& s1 = GetStringRef(inst.args[0], env);
        auto& s2 = GetStringRef(inst.args[1], env);
        SetBool(*inst.o_dest, s1 == s2, env);
        return;
    }

    default: throw NotImplementedException{};
    }
}

StackFrame MakeStackFrame(QFuncBody* qFuncBody, StackFrame& curFrame, optional<QArg_Slot> o_dest, span<QArg_Input> args, QFactory& qFactory)
{
    StackFrame frame;

    frame.qFuncBody = qFuncBody;
    frame.ip = InstructionPointer{qFuncBody->blocks.front(), 0},
    frame.stackPointer = curFrame.stackPointer;
    frame.slots.resize(qFuncBody->slotInfos.size());

    // oRetSlotIndex 위치 담기
    if (o_dest)
        frame.retSlot = curFrame.slots[o_dest->index];
    
    for (size_t i = 0, count = qFuncBody->slotInfos.size(); i < count; i++)
    {
        auto& slot = qFuncBody->slotInfos[i];

        if (slot.o_argIndex) // argument로부터 복사
        {
            visit([i, &frame, &curFrame](auto& arg) {
                using T = remove_cvref_t<decltype(arg)>;
                if constexpr (same_as<T, QArg_Slot>)
                {
                    frame.slots[i] = curFrame.slots[arg.index];
                }
                else if constexpr (same_as<T, QArg_ConstBool>)
                {
                    *(bool*)frame.slots[i] = arg.value;
                }
                else if constexpr (same_as<T, QArg_ConstInt32>)
                {
                    *(int*)frame.slots[i] = arg.value;
                }
                else static_assert(false);
            }, args[*slot.o_argIndex]);

        }
        else
        {
            size_t size = GetSize(slot.qType, qFactory);
            frame.stackPointer -= size;
            frame.slots[i] = frame.stackPointer;

            if (slot.qType == qFactory.MakeStringType())
            {
                new (frame.slots[i]) string{};
            }
        }
    }

    return frame;
}

struct Evaluator
{
    Environment& env;
    QFactoryPtr qFactory;

    bool operator()(auto& inst) { return Eval(inst); }

    bool Eval(QInst_Ctor_String& inst)
    {
        auto* buf = env.curFrame->slots[inst.slot.index];
        new (buf) string{inst.text};
        return true;
    }

    bool Eval(QInst_CopyCtor_String& inst)
    {
        auto* buf = env.curFrame->slots[inst.slot.index];
        auto& srcStr = GetStringRef(inst.src, env);
        new (buf) string{srcStr};
        return true;
    }

    bool Eval(QInst_MoveCtor_String& inst)
    {
        auto* buf = env.curFrame->slots[inst.slot.index];
        auto& srcStr = GetStringRef(inst.src, env);
        new (buf) string{std::move(srcStr)};
        return true;
    }

    bool Eval(QInst_CopyAssign_String& inst)
    {
        auto& destStr = GetStringRef(inst.dest, env);
        auto& srcStr = GetStringRef(inst.src, env);
        destStr = srcStr;
        return true;
    }

    bool Eval(QInst_MoveAssign_String& inst)
    {
        auto& destStr = GetStringRef(inst.dest, env);
        auto& srcStr = GetStringRef(inst.src, env);
        destStr = move(srcStr);
        return true;
    }

    bool Eval(QInst_Dtor_String& inst)
    {
        auto* buf = env.curFrame->slots[inst.slot.index];
        auto* str = (string*)buf;
        str->~string();
        return true;
    }

    bool Eval(QInst_Load& inst)
    {
        // dest <- *src;

        // src는 포인터 값을 갖고 있다
        void* dest = GetLoc(inst.dest, env);
        void* src = GetPtr(inst.src, env);
        size_t size = GetSize(inst.type, *qFactory);

        memcpy(dest, src, size);
        return true;
    }

    bool Eval(QInst_Store& inst)
    {
        // *dest = value;
        void* dest = GetPtr(inst.dest, env);

        visit([this, dest, qType = inst.type](auto& src)
        {
            using T = remove_cvref_t<decltype(src)>;
            if constexpr (same_as<T, QArg_ConstBool>)
            {
                *(bool*)dest = src.value;
            }
            else if constexpr (same_as < T, QArg_ConstInt32>)
            {
                *(int*)dest = src.value;
            }
            else if constexpr (same_as<T, QArg_Slot>)
            {
                void* pSrc = GetLoc(src, env);
                size_t size = GetSize(qType, *qFactory);
                memcpy(dest, pSrc, size);
            }
            else static_assert(false);
        }, inst.src);

        return true;
    }

    bool Eval(QInst_AddrOf& inst)
    {   
        *(void**)env.curFrame->slots[inst.dest.index] = env.curFrame->slots[inst.slot.index];
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

        void* dest = GetLoc(inst.dest, env);

        visit([this, dest, qType = inst.type](auto& src)
        {
            using T = remove_cvref_t<decltype(src)>;
            if constexpr (same_as<T, QArg_ConstBool>)
            {
                *(bool*)dest = src.value;
            }
            else if constexpr (same_as < T, QArg_ConstInt32>)
            {
                *(int*)dest = src.value;
            }
            else if constexpr (same_as<T, QArg_Slot>)
            {
                void* pSrc = GetLoc(src, env);
                size_t size = GetSize(qType, *qFactory);
                memcpy(dest, pSrc, size);
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
        auto nFuncDecl = dynamic_cast<NFuncDecl*>(inst.rFuncDecl);
        auto bodies = env.qData->GetAllBodies();
        auto i = ranges::find_if(bodies, [nFuncDecl](QFuncBody& body) { return body.nFuncDecl == nFuncDecl; });
        if (i == bodies.end()) throw NotImplementedException{};

        auto frame = MakeStackFrame(&*i, *env.curFrame, inst.o_dest, inst.args, *qFactory);
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
        if (inst.o_value)
        {
            visit([this, &retValue = *inst.o_value](auto& value) 
            {
                using T = remove_cvref_t<decltype(value)>;

                if constexpr (same_as<T, QArg_ConstInt32>)
                {   
                    *(int*)env.curFrame->retSlot = value.value;
                }
                else if constexpr (same_as<T, QArg_ConstBool>)
                {
                    *(bool*)env.curFrame->retSlot = value.value;
                }
                else if constexpr (same_as<T, QArg_Slot>)
                {
                    void* valueLoc = GetLoc(value, env);
                    size_t size = GetSize(retValue.qType, *qFactory);
                    memcpy(env.curFrame->retSlot, valueLoc, size);
                }
            }, inst.o_value->value);
        }

        env.frames.pop_back();
        if (env.frames.empty()) return false;
        env.curFrame = &env.frames.back();
        return true;
    }

    bool Eval(QInst_CondJump& condJump)
    {
        bool cond = GetBool(condJump.cond, env);

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

bool Evaluate(QInst& inst, Environment& env, const QFactoryPtr& qFactory)
{
    return visit(Evaluator{env, qFactory}, inst);
}

} // namespace 
expected<void, DiagPtr> EvaluateQData(span<RModule*> rModules, QData* qData, NGlobalFuncDecl* nEntry, IEvalQDataCommandHandlerPtr&& cmdHandler, const QFactoryPtr& qFactory)
{
    auto bodies = qData->GetAllBodies();
    auto i = ranges::find_if(bodies, [nEntry](QFuncBody& body) { return body.nFuncDecl == nEntry; });
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
    env.curFrame->slots.resize(i->slotInfos.size());
    for (size_t j = 0, count = i->slotInfos.size(); j < count; j++)
    {
        auto& slot = i->slotInfos[j];
        size_t size = GetSize(slot.qType, *qFactory);
        env.curFrame->stackPointer -= size;
        env.curFrame->slots[j] = env.curFrame->stackPointer;
    }

    while(true)
    {
        auto& inst = env.curFrame->ip.block->GetInst(env.curFrame->ip.index++);

        bool cont = Evaluate(inst, env, qFactory);

        if (!cont) break;
    }

    return {};
}

} // namespace Citron