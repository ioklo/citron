#include "QEvaluation.h"

#include <ranges>
#include <iostream>
#include <unordered_map>
#include <variant>

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
};

struct Environment
{   
    InstructionPointer ip;
    IEvalQDataCommandHandlerPtr cmdHandler;

    // stack frame    
    QFuncBody* qFuncBody;
    // register index -> values
    std::vector<void*> regValues;  // 최소 void* 크기 만큼에서 동작하는, bool, int, T*
    std::vector<void*> stackSlots; // 스택 포인터
    std::vector<byte> stack;

    byte* stackPointer;
};

size_t GetSize(QType* type, QFactory& qFactory)
{   
    // TODO: HARD CODED
    if (type == qFactory.MakeBoolType())
        return 1;

    if (type == qFactory.MakeIntType())
        return 4;

    if (type == qFactory.MakeStringType())
        return sizeof(string); // TODO: 임시
    
    throw NotImplementedException{};
}

// 실제 값의 위치 포인터x

// Ptr타입의 값
void* GetPtr(QArg_Input& src, Environment& env)
{
    return visit(overloaded{
        [&env](QArg_Register& reg) { return env.regValues[reg.index]; },
        [&env](QArg_StackSlot& slot) { return env.stackSlots[slot.index]; },
        [](auto&) -> void* { throw NotImplementedException{}; }
    }, src);
}

void* GetPtr(QArg_Register& src, Environment& env)
{
    return env.regValues[src.index];
}

void* GetPtr(QArg_Loc& loc, Environment& env)
{
    return visit(overloaded{
        [&env](QArg_Register& reg) { return env.regValues[reg.index]; },
        [&env](QArg_StackSlot& slot) { return env.stackSlots[slot.index]; }
    }, loc);
}

void SetPtr(QArg_Register& dest, void* v, Environment& env)
{
    env.regValues[dest.index] = v;
}

// inplace 값의 위치를 나타내는
//void* GetLoc(QArg& arg, Environment& env)
//{
//    return visit(overloaded{
//        [&env](QArg_StackSlot& slot) { return env.stackSlots[slot.index]; },
//        [&env](QArg_Register& reg) { return (void*)&env.regValues[reg.index]; },
//        [&env](QArg_ConstBool& cb) { return (void*)&cb.value; },
//        [&env](QArg_ConstInt32& ci) { return (void*)&ci.value; },
//        [](auto&&) -> void* { throw NotImplementedException{}; }
//    }, arg);
//}

int GetInt(QArg_Input& arg, Environment& env)
{
    return visit(overloaded{
        [](QArg_ConstInt32& ci) { return ci.value; },
        [&env](QArg_Register& r) { return *(int*)&env.regValues[r.index]; },
        [&env](QArg_StackSlot& s) { return *(int*)env.stackSlots[s.index]; },
        [](auto&&) -> int { throw NotImplementedException{}; }
    }, arg);
}

void SetInt(QArg_Register& dest, int v, Environment& env)
{
    *(int*)&env.regValues[dest.index] = v;
}

bool GetBool(QArg_Input& arg, Environment& env)
{
    return visit(overloaded{
        [](QArg_ConstBool& cb) { return cb.value; },
        [&env](QArg_Register& r) { return *(bool*)&env.regValues[r.index]; },
        [&env](QArg_StackSlot& s) { return *(bool*)env.stackSlots[s.index]; },
        [](auto&&) -> bool { throw NotImplementedException{}; }
    }, arg);
}

bool GetBool(QArg_Register& reg, Environment& env)
{
    return *(bool*)&env.regValues[reg.index];
}

//
void SetBool(QArg_Register& dest, bool v, Environment& env)
{
    *(bool*)&env.regValues[dest.index] = v;
}

string* GetStringPtr(QArg_Register& arg, Environment& env)
{
    return (string*)env.regValues[arg.index];
}

string* GetStringPtr(QArg_Input& arg, Environment& env)
{
    return visit(overloaded{        
        [&env](QArg_Register& r) { return (string*)env.regValues[r.index]; },
        [&env](QArg_StackSlot& s) { return (string*)env.stackSlots[s.index]; },
        [](auto&&) -> string* { unreachable(); }
    }, arg);
}

void SetString(QArg_Input& arg, string&& s, Environment& env)
{   
    return visit(overloaded{
        [&env, &s](QArg_Register& reg) { *(string*)env.regValues[reg.index] = move(s); },
        [&env, &s](QArg_StackSlot& slot) { *(string*)env.stackSlots[slot.index] = move(s); },
        [](auto&&) { unreachable(); }
    }, arg);
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
    case DebugPrint_Items:
    {
        for (auto& arg : inst.args)
        {
            visit(overloaded{
                [](QArg_ConstBool& b) { cout << b.value; },
                [](QArg_ConstInt32& i) { cout << i.value; },
                [](auto&&) {}
            }, arg);
        }
        return;
    }

    case Command_Items:
    {
        for (auto& arg : inst.args)
        {
            // string이라면, 크기가 8을 넘으므로
            visit(overloaded{
                [&env](QArg_StackSlot& slot) {
                    auto* s = (string*)env.stackSlots[slot.index];
                    env.cmdHandler->Execute(*s);
                },
                [](auto&&) { assert(false);  }
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
            SetBool(*inst.result, !b, env);
            return;
        }

    case UnaryMinus_Int:
        {
            auto i = GetInt(inst.args[0], env);
            SetInt(*inst.result, -i, env);
            return;
        }

    case ToString_Bool:
        {
            auto b = GetBool(inst.args[1], env);
            SetString(inst.args[0], format("{}", b), env);
            return;
        }


    case ToString_Int:
        {
            auto i = GetInt(inst.args[1], env);
            SetString(inst.args[0], format("{}", i), env);
            return;
        }

    case PrefixInc_Int:
        {
            // ++i

            // 인자는 location
            int* ptr = (int*)GetPtr(inst.args[0], env);
            SetInt(*inst.result, ++(*ptr), env);
            return;
        }

    case PrefixDec_Int:
        {
            // --i

            // 인자는 location
            int* ptr = (int*)GetPtr(inst.args[0], env);
            SetInt(*inst.result, --(*ptr), env);
            return;
        }
    case PostfixInc_Int:
        {
            // i++

            // 인자는 location
            int* ptr = (int*)GetPtr(inst.args[0], env);
            SetInt(*inst.result, (*ptr)++, env);
            return;
        }
    case PostfixDec_Int:
        {
            // i--
            // 인자는 location
            int* ptr = (int*)GetPtr(inst.args[0], env);
            SetInt(*inst.result, (*ptr)--, env);
            return;
        }

    case Multiply_Int_Int:
        {
            auto i1 = GetInt(inst.args[0], env);
            auto i2 = GetInt(inst.args[1], env);
            SetInt(*inst.result, i1 * i2, env);
            return;
        }
    case Divide_Int_Int:
        {
            auto i1 = GetInt(inst.args[0], env);
            auto i2 = GetInt(inst.args[1], env);
            SetInt(*inst.result, i1 / i2, env);
            return;
        }

    case Modulo_Int_Int:
        {
            auto i1 = GetInt(inst.args[0], env);
            auto i2 = GetInt(inst.args[1], env);
            SetInt(*inst.result, i1 % i2, env);
            return;
        }
    case Add_Int_Int:
        {
            auto i1 = GetInt(inst.args[0], env);
            auto i2 = GetInt(inst.args[1], env);
            SetInt(*inst.result, i1 + i2, env);
            return;
        }

    case Add_String_String:
        {
            auto* s1 = GetStringPtr(inst.args[1], env);
            auto* s2 = GetStringPtr(inst.args[2], env);
            SetString(inst.args[0], *s1 + *s2, env);
            return;
        }

    case Subtract_Int_Int:
        {
            auto i1 = GetInt(inst.args[0], env);
            auto i2 = GetInt(inst.args[1], env);
            SetInt(*inst.result, i1 - i2, env);
            return;
        }

    case LessThan_Int_Int:
        {
            // const integer가 있으면,
            auto i1 = GetInt(inst.args[0], env);
            auto i2 = GetInt(inst.args[1], env);

            SetBool(*inst.result, i1 < i2, env);
            return;
        }

    case LessThan_String_String:
        {
            auto* s1 = GetStringPtr(inst.args[0], env);
            auto* s2 = GetStringPtr(inst.args[1], env);

            SetBool(*inst.result, *s1 < *s2, env);
            return;
        }

    case GreaterThan_Int_Int:
        {
            // const integer가 있으면,
            auto i1 = GetInt(inst.args[0], env);
            auto i2 = GetInt(inst.args[1], env);

            SetBool(*inst.result, i1 > i2, env);
            return;
        }

    case GreaterThan_String_String:
        {
            auto* s1 = GetStringPtr(inst.args[0], env);
            auto* s2 = GetStringPtr(inst.args[1], env);

            SetBool(*inst.result, *s1 > *s2, env);
            return;
        }

    case LessThanOrEqual_Int_Int:
        {
            auto i1 = GetInt(inst.args[0], env);
            auto i2 = GetInt(inst.args[1], env);
            SetBool(*inst.result, i1 <= i2, env);
            return;
        }
    case LessThanOrEqual_String_String:
        {
            auto* s1 = GetStringPtr(inst.args[0], env);
            auto* s2 = GetStringPtr(inst.args[1], env);

            SetBool(*inst.result, *s1 <= *s2, env);
            return;
        }

    case GreaterThanOrEqual_Int_Int:
        {
            auto i1 = GetInt(inst.args[0], env);
            auto i2 = GetInt(inst.args[1], env);
            SetBool(*inst.result, i1 >= i2, env);
            return;
        }

    case GreaterThanOrEqual_String_String:
        {
            auto* s1 = GetStringPtr(inst.args[0], env);
            auto* s2 = GetStringPtr(inst.args[1], env);

            SetBool(*inst.result, *s1 >= *s2, env);
            return;
        }

    case Equal_Int_Int:
        {
            // const integer가 있으면,
            auto i1 = GetInt(inst.args[0], env);
            auto i2 = GetInt(inst.args[1], env);

            SetBool(*inst.result, i1 == i2, env);
            return;
        }

    case Equal_Bool_Bool:
        {
            auto b1 = GetBool(inst.args[0], env);
            auto b2 = GetBool(inst.args[1], env);

            SetBool(*inst.result, b1 == b2, env);
            return;
        }

    case Equal_String_String:
        {
            auto* s1 = GetStringPtr(inst.args[0], env);
            auto* s2 = GetStringPtr(inst.args[1], env);
            SetBool(*inst.result, *s1 == *s2, env);
            return;
        }

    default: throw NotImplementedException{};
    }
}

struct Evaluator
{
    Environment& env;
    QFactoryPtr qFactory;

    bool operator()(QInst_InitString& inst)
    {
        auto* buf = env.stackSlots[inst.dest.index];
        new (buf) string{inst.text};
        return true;
    }

    bool operator()(QInst_Load& inst)
    {
        // value <- *src;
        void* src = GetPtr(inst.src, env);

        switch (inst.type)
        {
        case QRegisterType::Ptr: SetPtr(inst.dest, *(void**)src, env); break;
        case QRegisterType::Int1: SetBool(inst.dest, *(bool*)src, env); break;
        case QRegisterType::Int32: SetInt(inst.dest, *(int*)src, env); break;
        default: unreachable();
        }

        return true;
    }

    bool operator()(QInst_Store& inst)
    {
        // *dest = value;
        void* dest = GetPtr(inst.dest, env);

        switch (inst.type)
        {
        case QRegisterType::Int1: *(bool*)dest = GetBool(inst.src, env); break;
        case QRegisterType::Int32: *(int*)dest = GetInt(inst.src, env); break;
        case QRegisterType::Ptr: *(void**)dest = GetPtr(inst.src, env); break;
        default: unreachable();
        }
        return true;
    }

    

    bool operator()(QInst_Assign& inst)
    {
        // %dest = %src
        // %r2 = %r1: memcpy(&regValues[r1.index], &regValues[r2.index], size) // void* 복사, size는 8보다 작을 것이다
        // %s2 = %s1: memcpy(stackSlots[s1.index], stackSlots[s2.index], size)

        switch (inst.type)
        {
        case QRegisterType::Ptr: SetPtr(inst.dest, GetPtr(inst.src, env), env); break;
        case QRegisterType::Int1: SetBool(inst.dest, GetBool(inst.src, env), env); break;
        case QRegisterType::Int32: SetInt(inst.dest, GetInt(inst.src, env), env); break;
        default: unreachable();
        }
        return true;
    }

    bool operator()(QInst_Intrinsic& inst)
    { 
        EvalIntrinsic(inst, env);
        return true; 
    }
     
    bool operator()(QInst_Return& inst) 
    {
        // throw NotImplementedException{};
        return false; 
    }

    bool operator()(QInst_CondJump& condJump)
    {
        bool cond = GetBool(condJump.cond, env);

        if (cond)
            env.ip = InstructionPointer{condJump.trueBlock, 0};
        else
            env.ip = InstructionPointer{condJump.falseBlock, 0};

        return true;
    }

    bool operator()(QInst_Jump& jump)
    {
        env.ip = InstructionPointer{jump.block, 0};
        return true;
    }

    bool operator()(auto& inst) 
    { 
        throw NotImplementedException{};
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
    if (i == bodies.end()) return unexpected{nullptr};

    Environment env;
    env.ip = InstructionPointer{i->entry, 0};
    env.cmdHandler = move(cmdHandler);

    env.stack.resize(1024 * 1024);
    env.stackPointer = env.stack.data() + env.stack.size();
    env.regValues.resize(i->regInfos.size());
    env.stackSlots.resize(i->slotInfos.size());
    for (size_t j = 0, count = i->slotInfos.size(); j < count; j++)
    {
        auto& slot = i->slotInfos[j];
        size_t size = GetSize(slot.qType, *qFactory);
        env.stackPointer -= size;
        env.stackSlots[j] = env.stackPointer;
    }

    while(true)
    {
        auto& inst = env.ip.block->GetInst(env.ip.index++);

        bool cont = Evaluate(inst, env, qFactory);

        if (!cont) break;
    }

    return {};
}

} // namespace Citron