#include "CommonQInstsTranslation.h"
#include <optional>
#include <ranges>
#include <cassert>

#include "Infra/Variants.h"
#include "Infra/Expected.h"

#include "MIR/MExp.h"
#include "QBodyContext.h"
#include "ScopeGuard.h"
#include "MExpQInstsTranslation.h"

using namespace std;

namespace Citron {

namespace {

expected<void, DiagPtr> TranslateMExp_StringElemToQInsts(MExp_StringElem& elem, optional<QArg_Slot> oDestSlot, QBodyContext& bodyContext)
{
    return visit([&bodyContext, &oDestSlot](auto& elem) -> expected<void, DiagPtr> {
        using T = remove_cvref_t<decltype(elem)>;
        if constexpr (same_as<T, MExp_StringElem_Text>)
        {
            if (oDestSlot)
                return bodyContext.EmitInst(QInst_Ctor_String{*oDestSlot, elem.text});

            return {};
        }
        else if constexpr (same_as<T, MExp_StringElem_Exp>)
        {
            return TranslateMExpToQInsts(elem.mExp, oDestSlot, bodyContext);
        }
        else static_assert(false);
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
            
            auto eEmitResult = bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Add_String_String, newSlot, {curSlot, elemSlot});
            RETURN_ON_ERROR(eEmitResult);

            swap(curSlot, newSlot);
        }
        
        auto eResultBack = TranslateMExp_StringElemToQInsts(exp->elements.back(), elemSlot, bodyContext);
        RETURN_ON_ERROR(eResultBack);

        if (destSlot)
        {
            auto eEmitResult = bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Add_String_String, *destSlot, {curSlot, elemSlot});
            RETURN_ON_ERROR(eEmitResult);
        }
    }

    return {};
}

std::expected<void, DiagPtr> TranslateMExp_StringToQInstsWithNewScope(MExp_String* exp, std::optional<QArg_Slot> destSlot, QBodyContext& bodyContext)
{
    ScopeGuard guard{bodyContext};
    return TranslateMExp_StringToQInsts(exp, move(destSlot), bodyContext);
}

} // namespace Citron