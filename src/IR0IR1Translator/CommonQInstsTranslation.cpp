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

expected<void, DiagPtr> TranslateMExp_StringElemToQInsts(MExp_StringElem& elem, optional<size_t> oDestSlotIndex, QBodyContext& bodyContext)
{
    return visit([&bodyContext, &oDestSlotIndex](auto& elem) -> expected<void, DiagPtr> {
        using T = remove_cvref_t<decltype(elem)>;
        if constexpr (same_as<T, MExp_StringElem_Text>)
        {
            if (oDestSlotIndex)
                return bodyContext.EmitInst(QInst_Ctor_String{QArg_Slot{*oDestSlotIndex}, elem.text});

            return {};
        }
        else if constexpr (same_as<T, MExp_StringElem_Exp>)
        {
            return TranslateMExpToQInsts(elem.mExp, oDestSlotIndex, bodyContext);
        }
        else static_assert(false);
    }, elem);
}
} // namespace 

expected<void, DiagPtr> TranslateMExp_StringToQInsts(MExp_String* exp, optional<size_t> destSlotIndex, QBodyContext& bodyContext)
{
    assert(!exp->elements.empty());

    // 원소가 한개라면, dest에 직접 넣는다
    if (exp->elements.size() == 1)
    {
        auto e_result = TranslateMExp_StringElemToQInsts(exp->elements.front(), destSlotIndex, bodyContext);
        RETURN_ON_ERROR(e_result);
    }
    else
    {
        auto* qStringType = bodyContext.GetStringQType();

        // "abc $x" => "abc " + x
        auto curSlotIndex = bodyContext.NewSlot(qStringType);
        auto e_resultFront = TranslateMExp_StringElemToQInsts(exp->elements.front(), curSlotIndex, bodyContext);
        RETURN_ON_ERROR(e_resultFront);

        auto elemSlotIndex = bodyContext.NewSlot(qStringType);
        auto newSlotIndex = bodyContext.NewSlot(qStringType);
        for (size_t i = 1, end = exp->elements.size() - 1; i < end; i++)
        {   
            auto e_result = TranslateMExp_StringElemToQInsts(exp->elements[i], elemSlotIndex, bodyContext);
            RETURN_ON_ERROR(e_result);
            
            auto e_emitResult = bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Add_String_String, QArg_Slot{newSlotIndex}, {QArg_Slot{curSlotIndex}, QArg_Slot{elemSlotIndex}});
            RETURN_ON_ERROR(e_emitResult);

            swap(curSlotIndex, newSlotIndex);
        }
        
        auto e_resultBack = TranslateMExp_StringElemToQInsts(exp->elements.back(), elemSlotIndex, bodyContext);
        RETURN_ON_ERROR(e_resultBack);

        if (destSlotIndex)
        {
            auto e_emitResult = bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Add_String_String, QArg_Slot{*destSlotIndex}, {QArg_Slot{curSlotIndex}, QArg_Slot{elemSlotIndex}});
            RETURN_ON_ERROR(e_emitResult);
        }
    }

    return {};
}

std::expected<void, DiagPtr> TranslateMExp_StringToQInstsWithNewScope(MExp_String* exp, std::optional<size_t> destSlot, QBodyContext& bodyContext)
{
    ScopeGuard guard{bodyContext};
    return TranslateMExp_StringToQInsts(exp, destSlot, bodyContext);
}

} // namespace Citron