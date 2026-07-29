#include "BuildNonTypeSymbolContext.h"
#include <variant>

#include "Syntax/Syntax.h"
#include "Logging/Diag.h"

#include "Infra/Ptr.h"
#include "Infra/Expected.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RDecl.h"
#include "RSymbol/RTypeParam.h"
#include "CommonTranslation.h"
#include "SmDeclContext.h"
#include "SmTypeRes.h"
#include "Misc.h"
#include "SmTypeTranslation.h"

using namespace std;

namespace Citron {

BuildNonTypeSymbolContext::BuildNonTypeSymbolContext(TakeRef<RFactoryPtr> rFactory)
    : rFactory{rFactory.Take()}
{
}

expected<RFuncReturn, DiagPtr> BuildNonTypeSymbolContext::MakeFuncReturn(SFuncReturn& funcRet, SmTypeResolveScope scope)
{
    return visit([this, scope](auto& funcRet) -> expected<RFuncReturn, DiagPtr> {
        using T = remove_cvref_t<decltype(funcRet)>;

        if constexpr (same_as<T, SFuncReturn_Normal>)
        {
            auto e_rType = TranslateSTypeExpToRType(funcRet.type, scope, rFactory.get());
            RETURN_ON_ERROR(e_rType);
            return RFuncReturn_Normal{*e_rType};
        }
        else if constexpr (same_as<T, SFuncReturn_Opaque>)
        {
            auto e_rTrait = TranslateSTypeExpToRTrait(funcRet.type, scope, rFactory.get());
            RETURN_ON_ERROR(e_rTrait);

            RType_Opaque* rOpaqueType = rFactory->MakeOpaqueType(e_rTrait->decl, e_rTrait->typeArgs);
            return RFuncReturn_Normal{rOpaqueType};
        }
        else static_assert(false);

    }, funcRet);
}

expected<tuple<vector<RFuncParameter>, bool>, DiagPtr> BuildNonTypeSymbolContext::MakeParameters(vector<SFuncParam>& sParams, SmTypeResolveScope scope)
{
    bool bLastParamVariadic = false;

    size_t paramCount = sParams.size();
    vector<RFuncParameter> rParams;
    rParams.reserve(paramCount);

    for (size_t i = 0; i < paramCount; i++)
    {
        auto& sParam = sParams[i];

        auto e_rParamKind = MakeParamKind(sParam.o_modifier, sParam.bRef);
        RETURN_ON_ERROR(e_rParamKind);
        auto& rParamKind = *e_rParamKind;

        auto e_rType = TranslateSTypeExpToRType(sParam.type, scope, rFactory.get());
        RETURN_ON_ERROR(e_rType);

        if (rParamKind == RFuncParameterKind::Params)
        {
            if (i == paramCount - 1)
            {
                bLastParamVariadic = true;
            }
            else
            {
                throw NotImplementedException{}; // 에러 처리, params는 마지막 파라미터에만 사용할 수 있습니다
            }

        }

        rParams.emplace_back(rParamKind, *e_rType, RName_Normal{sParam.name});
    }

    return make_tuple(move(rParams), bLastParamVariadic);
}

}