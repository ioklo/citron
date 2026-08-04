#include "CommonTranslation.h"

#include <ranges>

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "RSymbol/RAccessor.h"
#include "RSymbol/RDecl.h"
#include "RSymbol/RTypeDeclOuter.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTypeParam.h"
#include "RSymbol/RFuncReturn.h"
#include "SmTypeTranslation.h"
#include "SmTypeTranslationContexts.h"
#include "Misc.h"

using namespace std;

namespace Citron {

RNamespaceMemberAccessor MakeNamespaceMemberAccessor(optional<SAccessModifier> modifier)
{
    if (!modifier) return RNamespaceMemberAccessor::Private;

    switch (*modifier)
    {
    case SAccessModifier::Public: return RNamespaceMemberAccessor::Public;
    case SAccessModifier::Private: throw NotImplementedException{};
    case SAccessModifier::Protected: throw NotImplementedException{};
    }

    unreachable();
}

RStructMemberAccessor MakeStructMemberAccessor(optional<SAccessModifier> accessModifier) // throws FatalException
{
    if (!accessModifier) return RStructMemberAccessor::Public;

    switch (*accessModifier)
    {
    case SAccessModifier::Private: return RStructMemberAccessor::Private;
    case SAccessModifier::Protected: throw NotImplementedException{};
    case SAccessModifier::Public: throw NotImplementedException{};
    }

    unreachable();
}

// rDecl이 아직 tree에 매달려있지 않아도 되고, 대신 baseIndex를 따로 계산할것을 요구한다
vector<RTypeParam*> MakeTypeParams(size_t baseIndex, RDecl* rDecl, span<STypeParam> sTypeParams, InRef<RFactoryPtr> rFactory)
{
    assert(rDecl);

    vector<RTypeParam*> nTypeParams;
    size_t count = sTypeParams.size();
    nTypeParams.reserve(count);
    for (size_t i = 0; i < count; i++)
    {
        auto& sTypeParam = sTypeParams[i];
        auto* nTypeParam = (*rFactory)->MakeTypeParam(rDecl, RName_Normal{sTypeParam.name}, baseIndex + i, *rFactory);
        nTypeParams.push_back(nTypeParam);
    }

    return nTypeParams;
}

expected<RFuncReturn, DiagPtr> MakeFuncReturn(SFuncReturn& funcRet, RDecl* decl, std::span<RTypeParam*> typeParams, SmTypeTranslationContexts& contexts)
{
    return visit([decl, typeParams, &contexts](auto& funcRet) -> expected<RFuncReturn, DiagPtr> {
        using T = remove_cvref_t<decltype(funcRet)>;

        if constexpr (same_as<T, SFuncReturn_Normal>)
        {
            auto e_rType = TranslateSTypeExpToRType(funcRet.type, contexts);
            RETURN_ON_ERROR(e_rType);
            return RFuncReturn_Normal{*e_rType};
        }
        else if constexpr (same_as<T, SFuncReturn_Opaque>)
        {
            auto e_rTrait = TranslateSTypeExpToRTrait(funcRet.type, contexts);
            RETURN_ON_ERROR(e_rTrait);

            // rDecl과 typeParams로 open typeArguments를 만들어야 한다
            auto* outerTypeArgs = decl->GetOuter()->MakeOpenTypeArgs(*contexts.rFactory);

            vector<RType*> typeArgsVector;
            typeArgsVector.reserve(typeParams.size());
            for (auto* typeParam : typeParams)
                typeArgsVector.push_back(contexts.rFactory->MakeTypeVarType(typeParam));

            auto* typeArgs = contexts.rFactory->AppendTypeArguments(outerTypeArgs, typeArgsVector);
            RType_Opaque* rOpaqueType = contexts.rFactory->MakeOpaqueType({e_rTrait->decl, e_rTrait->typeArgs}, {decl, typeArgs});
            return RFuncReturn_Normal{rOpaqueType};
        }
        else static_assert(false);

    }, funcRet);
}

expected<tuple<vector<RFuncParameter>, bool>, DiagPtr> MakeFuncParameters(span<SFuncParam> sParams, SmTypeTranslationContexts& contexts)
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

        auto e_rType = TranslateSTypeExpToRType(sParam.type, contexts);
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
