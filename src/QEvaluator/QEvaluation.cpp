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
struct RawValue_Int { int value; };
struct RawValue_Bool { bool value; };

using RawValue = variant<RawValue_Int, RawValue_Bool>;

struct Environment
{   
    using Value = variant<int>;
    unordered_map<string, RawValue> namedValues;
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