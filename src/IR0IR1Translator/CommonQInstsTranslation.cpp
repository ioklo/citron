#include "CommonQInstsTranslation.h"
#include <optional>
#include <ranges>

#include "Infra/Variants.h"
#include "Infra/Expected.h"

#include "MIR/MExp.h"
#include "QBodyContext.h"
#include "MExpQInstsTranslation.h"

using namespace std;

namespace Citron::IR0IR1Translator {

expected<QArg, DiagPtr> TranslateMExp_StringToQInsts(MExp_String* exp, QBodyContext& bodyContext)
{
    // "abc $x" => "abc " + x
    optional<QArg> curArg;

    for (auto& elem : exp->elements)
    {
        auto eElemValue = visit(overloaded{
            [&bodyContext](MExp_StringElem_Text& textElem)
            {
                auto slot = bodyContext.NewStackSlot(bodyContext.MakeQStringType());
                bodyContext.AddInst(QInst_InitString{slot, textElem.text});
                return expected<QArg, DiagPtr>{slot};
            },
            [&bodyContext](MExp_StringElem_Exp& expElem) { return TranslateMExpToQInsts(expElem.mExp, bodyContext); }
        }, elem);
        RETURN_ON_ERROR(eElemValue);

        if (curArg)
        {
            auto newArg = bodyContext.AddIntrinsic(QInst_IntrinsicKind::Add_String_String, {*curArg, *eElemValue});
            curArg = newArg;
        }
        else
        {
            curArg = *eElemValue;
        }
    }
    
    assert(curArg);
    return *curArg;
}

} // namespace Citron::IR0IR1Translator