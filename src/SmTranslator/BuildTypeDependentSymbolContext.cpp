#include "BuildTypeDependentSymbolContext.h"
#include <variant>

#include "Syntax/Syntax.h"

#include "Infra/Ptr.h"
#include "Infra/Expected.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "NSymbol/NDecl.h"
#include "CommonTranslation.h"
#include "Misc.h"

using namespace std;

namespace Citron {

BuildTypeDependentSymbolContext::BuildTypeDependentSymbolContext(const RFactoryPtr& rFactory, const NFactoryPtr& nFactory)
    : rFactory{rFactory}, nFactory{nFactory}
{
}

RType* BuildTypeDependentSymbolContext::MakeType(STypeExp* sTypeExp, NDecl* decl)
{
    // TODO: ScopeContext::TranslateSTypeExpToRType 에도 같은 코드가 있다
    struct Visitor
    {
        using ResultType = RType*;

        RFactory* rFactory;

        RType* Visit(STypeExp_Id* idExp)
        {
            if (idExp->name == "void")
                return rFactory->MakeVoidType();
            else if (idExp->name == "int")
                return rFactory->MakeIntType();
            else if (idExp->name == "string")
                return rFactory->MakeStringType();
            else if (idExp->name == "bool")
                return rFactory->MakeBoolType();

            throw NotImplementedException{};
        }

        RType* Visit(STypeExp* e)
        {
            throw NotImplementedException{};
        }
    } visitor{rFactory.get()};

    return Accept(visitor, sTypeExp);
}

RFuncReturn BuildTypeDependentSymbolContext::MakeFuncReturn(SFuncReturn& funcRet, NDecl* decl)
{
    return visit([this, decl](auto& funcRet) {
        using T = remove_cvref_t<decltype(funcRet)>;

        if constexpr (same_as<T, SFuncReturn_Normal>)
        {
            auto* rType = MakeType(funcRet.type, decl);
            return RFuncReturn_Normal(rType);
        }
        else if constexpr (same_as<T, SFuncReturn_Opaque>)
        {
            // rType이 맞는걸까
            auto* rType = MakeType(funcRet.trait, decl);
            return RFuncReturn_Opaque{rType};
        }
        else static_assert(false);

    }, funcRet);
}

expected<tuple<vector<RFuncParameter>, bool>, DiagPtr> BuildTypeDependentSymbolContext::MakeParameters(NDecl* decl, vector<SFuncParam>& sParams)
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

        auto type = this->MakeType(sParam.type, decl);
        if (!type) throw NotImplementedException{}; // 에러 처리

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

        rParams.emplace_back(rParamKind, type, RName_Normal{sParam.name});
    }

    return make_tuple(move(rParams), bLastParamVariadic);
}

}