#include "QEvaluation.h"

#include <ranges>
#include <iostream>
#include <unordered_map>

#include "Infra/Variants.h"
#include "Infra/Exceptions.h"

#include "RSymbol/RModule.h"
#include "NSymbol/NGlobalFuncDecl.h"
#include "QIR/QData.h"
#include "QIR/QFuncBody.h"
#include "QIR/QBlock.h"
#include "Logging/Diag.h"

using namespace std;

namespace Citron {

namespace {

struct InstructionPointer
{
    QBlock* block;
    int index;
};

// 실제 value
struct RawValue_Ptr { void* ptr; };
struct RawValue_Int { int value; };
struct RawValue_Bool { bool value; };
struct RawValue_String { string value; };

using RawValue = variant<RawValue_Ptr, RawValue_Int, RawValue_Bool, RawValue_String>;

struct Environment
{   
    using Value = variant<int>;
    unordered_map<string, RawValue> namedValues;
    std::unordered_map<size_t, RawValue> locals;
};

int GetInt(QValue value, Environment& env)
{
    return visit(overloaded{
        [](QValue_ConstInteger& ci) { return ci.value; },
        [&env](QValue_Named& n) { return get<RawValue_Int>(env.namedValues[n.name]).value; },
        [](auto&&) -> int { throw NotImplementedException{}; }
    }, value);
}

bool GetBool(QValue value, Environment& env)
{
    return visit(overloaded{
        [](QValue_ConstBool& cb) { return cb.value; },
        [&env](QValue_Named& n) { return get<RawValue_Bool>(env.namedValues[n.name]).value; },
        [](auto&&) -> bool { throw NotImplementedException{}; }
    }, value);
}

void SetBool(QValue_Named namedValue, bool v, Environment& env)
{
    env.namedValues[namedValue.name] = RawValue_Bool{v};
}

void SetString(QValue_Named namedValue, string s, Environment& env)
{
    env.namedValues[namedValue.name] = RawValue_String{s};
}

RawValue GetRawValue(QValue& value, Environment& env)
{
    return visit<RawValue>(overloaded{
        [&env](QValue_Local& local) { return env.locals[local.index]; },
        [&env](QValue_Named& named) { return env.namedValues[named.name]; },
        [&env](QValue_ConstBool& cb) { return RawValue_Bool{cb.value}; },
        [&env](QValue_ConstInteger& ci) { return RawValue_Int{ci.value}; },
        [&env](QValue_String& s) { throw NotImplementedException{}; return RawValue_Int{0}; }
    }, value);
}

void SetRawValue(QValue& value, RawValue& rawValue, Environment& env)
{
    return visit(overloaded{
        [&env, &rawValue](QValue_Local& local) { env.locals[local.index] = rawValue; },
        [&env, &rawValue](QValue_Named& named) { env.namedValues[named.name] = rawValue; },
        [](auto&) { throw NotImplementedException{}; }
    }, value);
}

} // namespace 
expected<void, DiagPtr> EvaluateQData(span<RModule*> rModules, QData* qData, NGlobalFuncDecl* nEntry, IEvalQDataCommandHandlerPtr&& cmdHandler)
{
    auto bodies = qData->GetAllBodies();
    auto i = ranges::find_if(bodies, [nEntry](QFuncBody& body) { return body.nFuncDecl == nEntry; });
    if (i == bodies.end()) return unexpected{nullptr};

    InstructionPointer ip{i->entry, 0};
    Environment env;

    while(true)
    {
        auto& inst = ip.block->GetInst(ip.index++);

        bool cont = visit(overloaded{
            [&env](QInst_Store& inst)
            {
                auto rawLoc = GetRawValue(inst.loc, env);
                auto rawValue = GetRawValue(inst.value, env);

                // ptr에 value를 저장합니다.
                void* ptr = get<RawValue_Ptr>(rawLoc).ptr;

                visit(overloaded{
                    [&ptr](RawValue_String& rb) {*((string*)ptr) = rb.value; },
                    [&ptr](RawValue_Bool& rb) {*((bool*)ptr) = rb.value; },
                    [&ptr](RawValue_Int& ri) {*((int*)ptr) = ri.value; },
                    [&ptr](RawValue_Ptr& p) {*((void**)ptr) = p.ptr; }
                }, rawValue);
                return true;
            },
            [&env](QInst_Load& inst)
            {
                auto rawValue = GetRawValue(inst.loc, env);

                // TODO: rawValue는 포인터고, 무슨 타입인지 알수가 없으니, 어딘가에 적어놓고 *rawValue를 저장하는 방식으로 해야한다
                throw NotImplementedException{};

                SetRawValue(inst.value, rawValue, env);

                return true;
            },
            [&env](QInst_Alloc& alloc)
            {
                // stack이니 그냥 하나 올리면 되는데 일단 new
                // TODO: 
                env.locals[alloc.loc.index] = RawValue_Ptr{new byte[alloc.size]};
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
                        visit(overloaded{
                            [&cmdHandler](QValue_String& s) { cmdHandler->Execute(s.value); },
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
                            [](QValue_ConstBool& b) { cout << b.value; },
                            [](QValue_ConstInteger& i) { cout << i.value; },
                            [](QValue_String& s) { cout << s.value; },
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