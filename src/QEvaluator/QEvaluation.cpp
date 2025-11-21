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
    std::vector<void*> regValues;  // 최소 void* 크기 만큼에서 동작하는
    std::vector<byte> stack;

    byte* stackPointer;
};

size_t GetSize(QType* type, QFactory& qFactory)
{   
    if (type == qFactory.MakeBoolType())
        return 1;

    if (type == qFactory.MakeIntType())
        return 4;

    if (type == qFactory.MakeStringType())
        return sizeof(string); // TODO: 임시
    
    throw NotImplementedException{};
}

int GetInt(QArg arg, Environment& env)
{
    return visit(overloaded{
        [](QArg_ConstInt32& ci) { return ci.value; },
        [&env](QArg_Register& r) { return *(int*)&env.regValues[r.index]; },
        [](auto&&) -> int { throw NotImplementedException{}; }
    }, arg);
}

bool GetBool(QArg arg, Environment& env)
{
    return visit(overloaded{
        [](QArg_ConstBool& cb) { return cb.value; },
        [&env](QArg_Register& r) { return (bool)env.regValues[r.index]; },
        [](auto&&) -> bool { throw NotImplementedException{}; }
    }, arg);
}

//
void SetBool(QArg_Register reg, bool v, Environment& env)
{
    *(bool*)&env.regValues[reg.index] = v;
}

string* GetString(QArg arg, Environment& env)
{
    auto& reg = get<QArg_Register>(arg);
    return (string*)env.regValues[reg.index];
}

void SetString(QArg_Register reg, string&& s, Environment& env)
{   
    new ((string*)env.regValues[reg.index]) string{move(s)};
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

    while(true)
    {
        auto& inst = ip.block->GetInst(ip.index++);

        bool cont = visit(overloaded{
            [&env](QInst_InitString& inst) {
                auto* buf = (string*)env.regValues[inst.buf.index];
                new (buf) string{inst.s};
                return true;
            },
            [&env, qFactory](QInst_Store& inst)
            {
                // loc <- value;
                void* ptr = &env.regValues[inst.loc.index];

                // inst.value가 
                visit(overloaded{
                    [ptr](QArg_ConstBool& cb) {
                        *((bool*)ptr) = cb.value;
                    },
                    [ptr](QArg_ConstInt32& ci) {
                        *((int*)ptr) = ci.value;
                    },
                    [&env, ptr, qFactory](QArg_Register& reg) {
                        if (reg.qType == qFactory->MakeBoolType())
                            *((bool*)ptr) = *(bool*)&env.regValues[reg.index];
                        else if (reg.qType == qFactory->MakeIntType())
                            *((int*)ptr) = *(int*)&env.regValues[reg.index];
                        else if (reg.qType == qFactory->MakeStringType())
                            *((string**)ptr) = (string*)env.regValues[reg.index];
                        else
                            throw NotImplementedException{};
                    }
                }, inst.value);

                return true;
            },
            [&env, qFactory](QInst_Load& inst)
            {
                // value <- *loc;
                void* ptr = &env.regValues[inst.loc.index];

                // value쪽의 qType을 쓴다
                if (inst.value.qType == qFactory->MakeBoolType())
                {
                    env.regValues[inst.value.index] = (void*)*(bool*)ptr;
                }
                else if (inst.value.qType == qFactory->MakeIntType())
                {
                    env.regValues[inst.value.index] = (void*)(int*)ptr;
                }
                else
                {
                    throw NotImplementedException{};
                    // env.regValues[inst.value.index] = RegValue_Ptr{*((void**)ptr)};
                }

                return true;
            },
            [&env, qFactory](QInst_Alloc& alloc)
            {   
                // stack이니 그냥 하나 올리면 되는데 일단 new
                env.stackPointer -= GetSize(alloc.loc.qType, *qFactory);
                env.regValues[alloc.loc.index] = env.stackPointer;
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
                            [&env, &cmdHandler](QArg_Register& r) { 
                                auto* s = (string*)env.regValues[r.index];
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
                if (GetBool(condJump.value, env))
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