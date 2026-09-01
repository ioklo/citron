#include "STypeExp_IdToImTypeExp.h"
#include <string_view>
#include <span>
#include "Infra/Expected.h"
#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RFactory.h"
#include "ImTypeExp.h"
#include "SmTypeTranslationContexts.h"
#include "SmTypeTranslation.h"
#include "SmTypeRes.h"

using namespace std;

namespace Citron {

namespace {

expected<optional<ImTypeExp>, DiagPtr> HandleReservedType(std::string_view name, span<STypeExp*> typeArgs, RFactory* rFactory)
{
    if (name == "void")
    {
        if (!typeArgs.empty()) return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();
        return ImTypeExp_Type{rFactory->MakeVoidType()};
    }
    else if (name == "int")
    {
        if (!typeArgs.empty()) return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();
        return ImTypeExp_Type{rFactory->MakeIntType()};
    }
    else if (name == "string")
    {
        if (!typeArgs.empty()) return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();

        return ImTypeExp_Type{rFactory->MakeStringType()};
    }
    else if (name == "bool")
    {
        if (!typeArgs.empty()) return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();
        return ImTypeExp_Type{rFactory->MakeBoolType()};
    }

    return nullopt;
}

}

expected<RTypeArguments*, DiagPtr> MakeTypeArguments(RTypeArguments* outerTypeArgs, std::span<STypeExp*> sMemberTypeArgs, SmTypeTranslationContexts& contexts)
{
    auto e_memberTypeArgs = MakeRTypeArguments(sMemberTypeArgs, contexts);
    RETURN_ON_ERROR(e_memberTypeArgs);

    return contexts.rFactory->MergeTypeArguments(outerTypeArgs, *e_memberTypeArgs);
}

// SmTypeRes <TypeArgs> => ImTypeExp
expected<ImTypeExp, DiagPtr> TranslateSmTypeResAndSTypeArgsToImTypeExp(SmTypeRes&& typeRes, span<STypeExp*> sMemberTypeArgs, SmTypeTranslationContexts& contexts)
{
    return move(typeRes).Visit([sMemberTypeArgs, &contexts](auto&& typeRes) -> expected<ImTypeExp, DiagPtr> {

        using T = remove_cvref_t<decltype(typeRes)>;

        if constexpr (same_as<T, SmTypeRes_Namespaces>)
        {
            return ImTypeExp_Namespaces{typeRes.namespaces};
        }
        else if constexpr (same_as<T, SmTypeRes_Class>)
        {
            auto e_typeArgs = MakeTypeArguments(typeRes.outerAppliedDecl.outerTypeArgs, sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_typeArgs);
            return ImTypeExp_Type{contexts.rFactory->MakeClassType(RAppliedDecl<RClassDecl>{typeRes.outerAppliedDecl.decl, *e_typeArgs})};
        }
        else if constexpr (same_as<T, SmTypeRes_Struct>)
        {
            auto e_typeArgs = MakeTypeArguments(typeRes.outerAppliedDecl.outerTypeArgs, sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_typeArgs);
            return ImTypeExp_Type{contexts.rFactory->MakeStructType(RAppliedDecl<RStructDecl>{typeRes.outerAppliedDecl.decl, *e_typeArgs})};
        }
        else if constexpr (same_as<T, SmTypeRes_Enum>)
        {
            auto e_typeArgs = MakeTypeArguments(typeRes.outerAppliedDecl.outerTypeArgs, sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_typeArgs);
            return ImTypeExp_Type{contexts.rFactory->MakeEnumType(RAppliedDecl<REnumDecl>{typeRes.outerAppliedDecl.decl, *e_typeArgs})};
        }
        else if constexpr (same_as<T, SmTypeRes_EnumElem>)
        {
            auto e_typeArgs = MakeTypeArguments(typeRes.outerAppliedDecl.outerTypeArgs, sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_typeArgs);
            return ImTypeExp_Type{contexts.rFactory->MakeEnumElemType(RAppliedDecl<REnumElemDecl>{typeRes.outerAppliedDecl.decl, *e_typeArgs})};
        }
        else if constexpr (same_as<T, SmTypeRes_Interface>)
        {
            // TODO: [71] 2026-07-18, interface 구현
            throw NotImplementedException{};

            //auto e_typeArgs = MakeTypeArguments(typeRes.outerAppliedDecl.outerTypeArgs, sMemberTypeArgs, contexts);
            //RETURN_ON_ERROR(e_typeArgs);
            //return ImTypeExp_Type{contexts.rFactory->MakeInterfaceType(typeRes.outerAppliedDecl.decl, *e_typeArgs)};
        }
        else if constexpr (same_as<T, SmTypeRes_Lambda>)
        {
            // TODO: [65] 2026-07-06, RLambdaDecl제거, RStructDecl을 쓰도록 변경
            throw NotImplementedException{};
        }
        else if constexpr (same_as<T, SmTypeRes_TypeVar>)
        {
            return ImTypeExp_Type{contexts.rFactory->MakeTypeVarType(typeRes.decl)};
        }
        else if constexpr (same_as<T, SmTypeRes_Trait>)
        {
            auto e_typeArgs = MakeTypeArguments(typeRes.outerAppliedDecl.outerTypeArgs, sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_typeArgs);
            return ImTypeExp_Trait{typeRes.outerAppliedDecl.decl, *e_typeArgs};
        }
        else if constexpr (same_as<T, SmTypeRes_Type>)
        {
            if (!sMemberTypeArgs.empty())
                return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();

            return ImTypeExp_Type{typeRes.type};
        }
        else static_assert(false);
    });
}

expected<ImTypeExp, DiagPtr> TranslateSTypeExp_IdToImTypeExp(STypeExp_Id* sTypeExp, SmTypeTranslationContexts& contexts)
{
    auto e_o_typeRes = HandleReservedType(sTypeExp->name, sTypeExp->typeArgs, contexts.rFactory);
    RETURN_ON_ERROR(e_o_typeRes);
    if (*e_o_typeRes) return **e_o_typeRes;

    auto o_typeRes = contexts.scope.ResolveTypeIdentifier(RName::Normal(sTypeExp->name), contexts.rFactory);
    if (!o_typeRes) return Error<Error_ResolveIdentifier_NotFound>();

    return TranslateSmTypeResAndSTypeArgsToImTypeExp(move(*o_typeRes), sTypeExp->typeArgs, contexts);
}

} // namespace Citron
