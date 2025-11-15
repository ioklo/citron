#include "CommonQInstsTranslation.h"
#include <optional>

#include "Infra/Variants.h"
#include "Infra/Expected.h"

#include "MIR/MExp.h"
#include "QBodyContext.h"
#include "MExpQInstsTranslation.h"

using namespace std;

namespace Citron::IR0IR1Translator {

expected<QValue, DiagPtr> TranslateMExp_StringToQInsts(MExp_String* exp, QBodyContext& bodyContext)
{
    // "abc $x" => "abc " + x
    optional<QValue> curValue;
    for (auto& elem : exp->elements)
    {
        auto eValueResult = visit(overloaded{
            [](MExp_StringElem_Text& textElem) { return expected<QValue, DiagPtr>{QValue_String{textElem.text}}; },
            [&bodyContext](MExp_StringElem_Exp& expElem) { return TranslateMExpToQInsts(expElem.mExp, bodyContext); }
        }, elem);
        RETURN_ON_ERROR(eValueResult);

        if (curValue)
        {
            QValue_Named newValue;
            bodyContext.AddIntrinsic(QInst_IntrinsicKind::Add_String_String, {newValue, *curValue, *eValueResult});
            curValue = newValue;
        }
        else
        {
            curValue = *eValueResult;
        }
    }

    assert(curValue);
    return *curValue;
}

} // namespace Citron::IR0IR1Translator