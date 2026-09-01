#include "SmTypeTranslation.h"
#include "Infra/Expected.h"
#include "Infra/Exceptions.h"
#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RTypeArguments.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RClassDecl.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/REnumDecl.h"
#include "RSymbol/REnumElemDecl.h"
#include "RSymbol/RFactory.h"
#include "SmTypeRes.h"
#include "STypeExpToImTypeExp.h"
#include "ImTypeExp.h"
#include "SmTypeTranslationContexts.h"

using namespace std;

namespace Citron {

expected<RType*, DiagPtr> TranslateImTypeExpToRType(ImTypeExp&& imTypeExp, SmTypeTranslationContexts& contexts)
{
    return move(imTypeExp).Visit([&contexts](auto&& imTypeExp) -> expected<RType*, DiagPtr> {
        using T = remove_cvref_t<decltype(imTypeExp)>;
        if constexpr (same_as<T, ImTypeExp_Namespaces>)
        {
            return Error<Error_ResolveIdentifier_CantUseNamespaceAsType>();
        }
        else if constexpr (same_as<T, ImTypeExp_Trait>)
        {
            return Error<Error_ResolveIdentifier_CantUseTraitAsType>();
        }
        else if constexpr (same_as<T, ImTypeExp_Type>)
        {
            return imTypeExp.type;
        }
        else static_assert(false);
    });
}

expected<RAppliedDecl<RTraitDecl>, DiagPtr> TranslateImTypeExpToRTrait(ImTypeExp&& imTypeExp, SmTypeTranslationContexts& contexts)
{
    return move(imTypeExp).Visit([](auto&& imTypeExp) -> expected<RAppliedDecl<RTraitDecl>, DiagPtr> {
        using T = remove_cvref_t<decltype(imTypeExp)>;
        if constexpr (same_as<T, ImTypeExp_Trait>)
        {
            return imTypeExp.appliedDecl;
        }
        else
        {
            return Error<Error_ResolveIdentifier_CantUseTypeAsTrait>();
        }
    });
}

expected<RType*, DiagPtr> TranslateSTypeExpToRType(STypeExp* sTypeExp, SmTypeTranslationContexts& contexts)
{
    auto e_imTypeExp = TranslateSTypeExpToImTypeExp(sTypeExp, contexts);
    RETURN_ON_ERROR(e_imTypeExp);

    return TranslateImTypeExpToRType(move(*e_imTypeExp), contexts);
}

expected<RAppliedDecl<RTraitDecl>, DiagPtr> TranslateSTypeExpToRTrait(STypeExp* sTypeExp, SmTypeTranslationContexts& contexts)
{
    auto e_imTypeExp = TranslateSTypeExpToImTypeExp(sTypeExp, contexts);
    RETURN_ON_ERROR(e_imTypeExp);

    return TranslateImTypeExpToRTrait(move(*e_imTypeExp), contexts);
}

expected<RType*, DiagPtr> MakeType(SmTypeRes& typeRes, span<STypeExp*> sMemberTypeArgs, SmTypeTranslationContexts& contexts)
{
    return typeRes.Visit([sMemberTypeArgs, &contexts](auto& typeRes) -> expected<RType*, DiagPtr> {
        using T = remove_cvref_t<decltype(typeRes)>;

        if constexpr (same_as<T, SmTypeRes_Namespaces>)
        {
            return Error<Error_ResolveIdentifier_CantUseNamespaceAsType>();
        }
        else if constexpr (same_as<T, SmTypeRes_Class>)
        {
            // 타입 인자 검사
            if (typeRes.outerAppliedDecl.decl->GetTypeParamCount() != sMemberTypeArgs.size())
                return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();

            auto e_memberTypeArgs = MakeRTypeArguments(sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_memberTypeArgs);

            auto* typeArgs = contexts.rFactory->MergeTypeArguments(typeRes.outerAppliedDecl.outerTypeArgs, *e_memberTypeArgs);
            return contexts.rFactory->MakeClassType(RAppliedDecl<RClassDecl>{typeRes.outerAppliedDecl.decl, typeArgs});
        }
        else if constexpr (same_as<T, SmTypeRes_Struct>)
        {
            // 타입 인자 검사
            if (typeRes.outerAppliedDecl.decl->GetTypeParamCount() != sMemberTypeArgs.size())
                return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();

            auto e_memberTypeArgs = MakeRTypeArguments(sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_memberTypeArgs);

            auto* typeArgs = contexts.rFactory->MergeTypeArguments(typeRes.outerAppliedDecl.outerTypeArgs, *e_memberTypeArgs);
            return contexts.rFactory->MakeStructType(RAppliedDecl<RStructDecl>{typeRes.outerAppliedDecl.decl, typeArgs});
        }
        else if constexpr (same_as<T, SmTypeRes_Enum>)
        {
            // 타입 인자 검사
            if (typeRes.outerAppliedDecl.decl->GetTypeParamCount() != sMemberTypeArgs.size())
                return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();

            auto e_memberTypeArgs = MakeRTypeArguments(sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_memberTypeArgs);

            auto* typeArgs = contexts.rFactory->MergeTypeArguments(typeRes.outerAppliedDecl.outerTypeArgs, *e_memberTypeArgs);
            return contexts.rFactory->MakeEnumType(RAppliedDecl<REnumDecl>{typeRes.outerAppliedDecl.decl, typeArgs});
        }
        else if constexpr (same_as<T, SmTypeRes_EnumElem>)
        {
            // 타입 인자 검사
            if (typeRes.outerAppliedDecl.decl->GetTypeParamCount() != sMemberTypeArgs.size())
                return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();

            auto e_memberTypeArgs = MakeRTypeArguments(sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_memberTypeArgs);

            auto* typeArgs = contexts.rFactory->MergeTypeArguments(typeRes.outerAppliedDecl.outerTypeArgs, *e_memberTypeArgs);
            return contexts.rFactory->MakeEnumElemType(RAppliedDecl<REnumElemDecl>{typeRes.outerAppliedDecl.decl, typeArgs});
        }
        else if constexpr (same_as<T, SmTypeRes_Interface>)
        {
            // TODO: [71] 2026-07-18, interface 구현
            throw NotImplementedException{};
        }
        else if constexpr (same_as<T, SmTypeRes_Lambda>)
        {
            // TODO: [65] 2026-07-06, RLambdaDecl제거, RStructDecl을 쓰도록 변경
            throw NotImplementedException{};
        }
        else if constexpr (same_as<T, SmTypeRes_TypeVar>)
        {
            return contexts.rFactory->MakeTypeVarType(typeRes.decl);
        }
        else if constexpr (same_as<T, SmTypeRes_Type>)
        {
            if (!sMemberTypeArgs.empty())
                return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();

            return typeRes.type;
        }
        else if constexpr (same_as<T, SmTypeRes_Trait>)
        {
            return Error<Error_ResolveIdentifier_CantUseTraitAsType>();
        }
        else static_assert(false);
    });
}

expected<RTypeArguments*, DiagPtr> MakeRTypeArguments(span<STypeExp*> typeArgs, SmTypeTranslationContexts& contexts)
{
    vector<RType*> items;
    items.reserve(typeArgs.size());

    for (auto* typeArg : typeArgs)
    {
        auto e_type = TranslateSTypeExpToRType(typeArg, contexts);
        RETURN_ON_ERROR(e_type);

        items.push_back(*e_type);
    }

    return contexts.rFactory->MakeTypeArguments(items);
}

expected<RTypeArguments*, DiagPtr> MakeRTypeArguments(RTypeArguments* outerTypeArgs, span<STypeExp*> sMemberTypeArgs, SmTypeTranslationContexts& contexts)
{
    vector<RType*> items;
    items.reserve(sMemberTypeArgs.size());

    for (auto* sMemberTypeArg : sMemberTypeArgs)
    {
        auto e_rTypeArg = TranslateSTypeExpToRType(sMemberTypeArg, contexts);
        RETURN_ON_ERROR(e_rTypeArg);

        items.push_back(*e_rTypeArg);
    }

    return contexts.rFactory->AppendTypeArguments(outerTypeArgs, items);
}

} // namespace Citron
