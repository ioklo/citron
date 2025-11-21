#include "CommonQInstsTranslation.h"
#include <optional>

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
        auto eValueResult = visit(overloaded{
            [&bodyContext](MExp_StringElem_Text& textElem) 
            { 
                auto reg = bodyContext.AddBuffer(bodyContext.MakeQStringType());
                bodyContext.AddInst(QInst_InitString{reg, textElem.text});
                return expected<QArg, DiagPtr>{reg};
            },
            [&bodyContext](MExp_StringElem_Exp& expElem) { return TranslateMExpToQInsts(expElem.mExp, bodyContext); }
        }, elem);
        RETURN_ON_ERROR(eValueResult);

        if (curArg)
        {   
            auto newReg = bodyContext.AddIntrinsic(QInst_IntrinsicKind::Add_String_String, {*curArg, *eValueResult});
            curArg = newReg;
        }
        else
        {
            curArg = *eValueResult;
        }
    }

    assert(curArg);
    return *curArg;
}

} // namespace Citron::IR0IR1Translator