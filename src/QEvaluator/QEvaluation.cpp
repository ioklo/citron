#include "QEvaluation.h"

#include <ranges>
#include <iostream>

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

} // namespace 
expected<void, DiagPtr> Evaluate(span<RModule> rModules, QData& qData, NGlobalFuncDecl* nEntry)
{
    auto bodies = qData.GetAllBodies();
    auto i = ranges::find_if(bodies, [nEntry](QFuncBody& body) { return body.funcDecl == nEntry; });
    if (i == bodies.end()) return unexpected{nullptr};

    InstructionPointer ip{i->entry, 0};

    while(true)
    {
        auto& inst = ip.block->GetInst(ip.index++);

        bool cont = visit(overloaded{
            [](QInst_Intrinsic& inst) -> bool
            {
                switch (inst.kind) 
                {
                case QInst_IntrinsicKind::DebugPrint_Items: 
                {
                    for(auto& arg : inst.args)
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
                default:
                    throw NotImplementedException{};
                }

            },
            [](QInst_Return& inst) { return false; },
            [](auto&) { throw NotImplementedException{}; return false; }
        }, inst);

        if (!cont) break;
    }

    return {};
}

} // namespace Citron