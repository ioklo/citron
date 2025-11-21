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

void* GetPtr(QArg& arg, Environment& env)
{
    return visit(overloaded{
        [&env](QArg_StackSlot& slot) { return env.stackSlots[slot.index]; },
        [&env](QArg_Register& reg) { return env.regValues[reg.index]; },
        [](auto&&) -> void* { throw NotImplementedException{}; }
    }, arg);
}

void* GetLoc(QArg& arg, Environment& env)
{
    return visit(overloaded{
        [&env](QArg_StackSlot& slot) { return env.stackSlots[slot.index]; },
        [&env](QArg_Register& reg) { return (void*)&env.regValues[reg.index]; },
        [&env](QArg_ConstBool& cb) { return (void*)&cb.value; },
        [&env](QArg_ConstInt32& ci) { return (void*)&ci.value; },
        [](auto&&) -> void* { throw NotImplementedException{}; }
    }, arg);
}

int GetInt(QArg arg, Environment& env)
{
    return visit(overloaded{
        [](QArg_ConstInt32& ci) { return ci.value; },
        [&env](QArg_Register& r) { return *(int*)&env.regValues[r.index]; },
        [&env](QArg_StackSlot& s) { return *(int*)env.stackSlots[s.index]; },
        [](auto&&) -> int { throw NotImplementedException{}; }
    }, arg);
}

bool GetBool(QArg arg, Environment& env)
{
    return visit(overloaded{
        [](QArg_ConstBool& cb) { return cb.value; },
        [&env](QArg_Register& r) { return *(bool*)&env.regValues[r.index]; },
        [&env](QArg_StackSlot& s) { return *(bool*)env.stackSlots[s.index]; },
        [](auto&&) -> bool { throw NotImplementedException{}; }
    }, arg);
}

//
void SetBool(QArg dest, bool v, Environment& env)
{
    visit(overloaded{
        [&env, v](QArg_Register& reg) { *(bool*)&env.regValues[reg.index] = v; },
        [&env, v](QArg_StackSlot& slot) { *(bool*)env.stackSlots[slot.index] = v; },
        [](auto&&) { throw NotImplementedException{}; }
    }, dest);
}

string* GetString(QArg arg, Environment& env)
{
    auto& slot = get<QArg_StackSlot>(arg);
    return (string*)env.stackSlots[slot.index];
}

void SetString(QArg dest, string&& s, Environment& env)
{   
    auto& slot = get<QArg_StackSlot>(dest); // string은 stack slot에만 들어갈 수 있다
    *(string*)env.stackSlots[slot.index] = move(s);
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

} // namespace 
expected<void, DiagPtr> EvaluateQData(span<RModule*> rModules, QData* qData, NGlobalFuncDecl* nEntry, IEvalQDataCommandHandlerPtr&& cmdHandler, const QFactoryPtr& qFactory)
{
    auto bodies = qData->GetAllBodies();
    auto i = ranges::find_if(bodies, [nEntry](QFuncBody& body) { return body.nFuncDecl == nEntry; });
    if (i == bodies.end()) return unexpected{nullptr};

    InstructionPointer ip{i->entry, 0};
    Environment env;

    env.stack.resize(1024 * 1024);
    env.stackPointer = env.stack.data() + env.stack.size();
    env.regValues.resize(i->registerCount);

    env.stackSlots.resize(i->stackSlots.size());
    for (size_t j = 0, count = i->stackSlots.size(); j < count; j++)
    {
        auto& slot = i->stackSlots[j];
        size_t size = GetSize(slot.qType, *qFactory);
        env.stackPointer -= size;
        env.stackSlots[j] = env.stackPointer;
    }

    while(true)
    {
        auto& inst = ip.block->GetInst(ip.index++);

        bool cont = visit(overloaded{
            [&env](QInst_InitString& inst) {
                auto* buf = env.stackSlots[inst.slot.index];
                new (buf) string{inst.text};
                return true;
            },
            [&env, qFactory](QInst_Store& inst)
            {
                // *dest = value;
                void* dest = GetPtr(inst.dest, env);

                // inst.value가 
                visit(overloaded{
                    [dest](QArg_ConstBool& cb) {
                        *((bool*)dest) = cb.value;
                    },
                    [dest](QArg_ConstInt32& ci) {
                        *((int*)dest) = ci.value;
                    },
                    [&env, dest, qFactory](QArg_Register& reg) {
                        // TODO: HARD CODED
                        if (reg.qType == qFactory->MakeBoolType())
                            *((bool*)dest) = *(bool*)&env.regValues[reg.index];
                        else if (reg.qType == qFactory->MakeIntType())
                            *((int*)dest) = *(int*)&env.regValues[reg.index];
                        else
                            throw NotImplementedException{};
                    },
                    [&env, dest, qFactory](QArg_StackSlot& slot) {
                        if (slot.qType == qFactory->MakeStringType())
                            *((string*)dest) = *(string*)env.stackSlots[slot.index];
                        else
                            throw NotImplementedException{};
                    },
                }, inst.value);

                return true;
            },
            [&env, qFactory](QInst_Load& inst)
            {
                // value <- *src;
                void* src = GetPtr(inst.src, env);

                // value쪽의 qType을 쓴다
                visit(overloaded{
                    [&env, src, qFactory](QArg_Register& reg) {
                        // TODO: HARD CODED
                        if (reg.qType == qFactory->MakeBoolType())
                            *(bool*)&env.regValues[reg.index] = *((bool*)src);
                        else if (reg.qType == qFactory->MakeIntType())
                            *(int*)&env.regValues[reg.index] = *((int*)src);
                        else
                            throw NotImplementedException{};
                    },

                    [&env, src, qFactory](QArg_StackSlot& slot) {
                        // TODO: HARD CODED
                        if (slot.qType == qFactory->MakeBoolType())
                            *(bool*)env.stackSlots[slot.index] = *((bool*)src);
                        else if (slot.qType == qFactory->MakeIntType())
                            *(int*)env.stackSlots[slot.index] = *((int*)src);
                        else if (slot.qType == qFactory->MakeStringType())
                            new (env.stackSlots[slot.index]) string{*((string*)src)};
                        else
                            throw NotImplementedException{};
                    },
                    [](auto&&) { throw NotImplementedException{}; }
                }, inst.value);
                
                return true;
            },
            [&env](QInst_Assign& inst)
            {   
                // %dest = %src
                // %r2 = %r1: memcpy(&regValues[r1.index], &regValues[r2.index], size) // void* 복사, size는 8보다 작을 것이다
                // %s2 = %s1: memcpy(stackSlots[s1.index], stackSlots[s2.index], size)

                void* src = GetLoc(inst.src, env);
                void* dest = GetLoc(inst.dest, env);
                memmove(dest, src, inst.size);
                return true;
            },
            [&cmdHandler, &env](QInst_Intrinsic& inst) -> bool
            {
                switch (inst.kind)
                {
                case QInst_IntrinsicKind::Command_Items:
                {
                    for (auto& arg : inst.args)
                    {
                        // string이라면, 크기가 8을 넘으므로
                        visit(overloaded{
                            [&env, &cmdHandler](QArg_StackSlot& slot) { 
                                auto* s = (string*)env.stackSlots[slot.index];
                                cmdHandler->Execute(*s); 
                            },
                            [](auto&&) { assert(false);  }
                        }, arg);
                    }
                    return true;
                }

                case QInst_IntrinsicKind::DebugPrint_Items:
                {
                    for (auto& arg : inst.args)
                    {
                        visit(overloaded{
                            [](QArg_ConstBool& b) { cout << b.value; },
                            [](QArg_ConstInt32& i) { cout << i.value; },
                            [](auto&&) {}
                        }, arg);
                    }
                    return true;
                }

                case QInst_IntrinsicKind::LessThan_Int_Int:
                {
                    // const integer가 있으면, 
                    auto i1 = GetInt(inst.args[0], env);
                    auto i2 = GetInt(inst.args[1], env);

                    SetBool(*inst.result, i1 < i2, env);
                    return true;
                }

                case QInst_IntrinsicKind::GreaterThan_Int_Int:
                {
                    // const integer가 있으면, 
                    auto i1 = GetInt(inst.args[0], env);
                    auto i2 = GetInt(inst.args[1], env);

                    SetBool(*inst.result, i1 > i2, env);
                    return true;
                }

                case QInst_IntrinsicKind::ToString_Int:
                {
                    auto i = GetInt(inst.args[0], env);
                    SetString(*inst.result, format("{}", i), env);
                    return true;
                }

                case QInst_IntrinsicKind::ToString_Bool:
                {
                    auto b = GetBool(inst.args[0], env);
                    SetString(*inst.result, format("{}", b), env);
                    return true;
                }

                case QInst_IntrinsicKind::Add_String_String:
                {
                    auto* s1 = GetString(inst.args[0], env);
                    auto* s2 = GetString(inst.args[1], env);
                    SetString(*inst.result, *s1 + *s2, env);
                    return true;
                }

                default:
                    throw NotImplementedException{};
                }
            },
            [](QInst_ReturnVoid& inst) { return false; },
            [&ip, &env](QInst_CondJump& condJump) 
            {
                if (GetBool(condJump.cond, env))
                    ip = InstructionPointer{condJump.trueBlock, 0};
                else
                    ip = InstructionPointer{condJump.falseBlock, 0};

                return true;
            },
            [&ip](QInst_Jump& jump)
            {
                ip = InstructionPointer{jump.block, 0};
                return true;
            },
            
            [](auto& inst) { throw NotImplementedException{}; return false; }
        }, inst);

        if (!cont) break;
    }

    return {};
}

} // namespace Citron