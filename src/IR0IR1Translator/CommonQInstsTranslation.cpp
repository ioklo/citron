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

namespace {
expected<void, DiagPtr> TranslateMExp_StringElemToQInsts(MExp_StringElem& elem, optional<QArg_Slot> oDestSlot, QBodyContext& bodyContext)
{
    return visit<expected<void, DiagPtr>>(overloaded{
        [&bodyContext, oDestSlot](MExp_StringElem_Text& textElem) -> expected<void, DiagPtr>
        {
            if (oDestSlot)
                bodyContext.EmitInst(QInst_InitString{*oDestSlot, textElem.text});

            return {};
        },
        [&bodyContext, oDestSlot](MExp_StringElem_Exp& expElem) { return TranslateMExpToQInsts(expElem.mExp, oDestSlot, bodyContext); }
    }, elem);
}
} // namespace 

expected<void, DiagPtr> TranslateMExp_StringToQInsts(MExp_String* exp, std::optional<QArg_Slot> destSlot, QBodyContext& bodyContext)
{
    assert(!exp->elements.empty());

    // 원소가 한개라면, dest에 직접 넣는다
    if (exp->elements.size() == 1)
    {
        auto eResult = TranslateMExp_StringElemToQInsts(exp->elements.front(), destSlot, bodyContext);
        RETURN_ON_ERROR(eResult);
    }
    else
    {
        auto* qStringType = bodyContext.GetStringQType();

        // "abc $x" => "abc " + x
        auto curSlot = bodyContext.NewSlot(qStringType);
        auto eResultFront = TranslateMExp_StringElemToQInsts(exp->elements.front(), curSlot, bodyContext);
        RETURN_ON_ERROR(eResultFront);

        auto elemSlot = bodyContext.NewSlot(qStringType);
        auto newSlot = bodyContext.NewSlot(qStringType);
        for (size_t i = 1, end = exp->elements.size() - 1; i < end; i++)
        {   
            auto eResult = TranslateMExp_StringElemToQInsts(exp->elements[i], elemSlot, bodyContext);
            RETURN_ON_ERROR(eResult);
            
            bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Add_String_String, newSlot, {curSlot, elemSlot});
            swap(curSlot, newSlot);
        }
        
        auto eResultBack = TranslateMExp_StringElemToQInsts(exp->elements.back(), elemSlot, bodyContext);
        RETURN_ON_ERROR(eResultBack);

        if (destSlot)
            bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Add_String_String, *destSlot, {curSlot, elemSlot});
    }

    return {};
}

} // namespace Citron::IR0IR1Translator