#include "STypeExp_MemberToImTypeExp.h"
#include "Infra/Expected.h"
#include "Infra/Exceptions.h"
#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RClassDecl.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RFactory.h"
#include "ImTypeExp.h"
#include "STypeExpToImTypeExp.h"
#include "SmTypeTranslationContexts.h"
#include "SmTypeTranslation.h"

using namespace std;

namespace Citron {

expected<ImTypeExp, DiagPtr> TranslateRTypeDeclAndSTypeArgsToImTypeExp(RTypeDecl* typeDecl, RTypeArguments* outerTypeArgs, span<STypeExp*> sMemberTypeArgs, SmTypeTranslationContexts& contexts)
{
    struct Translator
    {
        using ResultType = expected<ImTypeExp, DiagPtr>;
        RTypeArguments* outerTypeArgs;
        span<STypeExp*> sMemberTypeArgs;
        SmTypeTranslationContexts& contexts;

        ResultType Visit(RClassDecl* rTypeDecl) 
        {
            auto e_typeArgs = MakeRTypeArguments(outerTypeArgs, sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_typeArgs);

            return ImTypeExp_Class{RAppliedDecl<RClassDecl>{rTypeDecl,* e_typeArgs}};
        }

        ResultType Visit(RStructDecl* rTypeDecl)
        {
            auto e_typeArgs = MakeRTypeArguments(outerTypeArgs, sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_typeArgs);

            return ImTypeExp_Struct{RAppliedDecl<RStructDecl>{rTypeDecl,* e_typeArgs}};
        }

        ResultType Visit(REnumDecl* rTypeDecl) 
        {
            auto e_typeArgs = MakeRTypeArguments(outerTypeArgs, sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_typeArgs);

            auto* type = contexts.rFactory->MakeEnumType(rTypeDecl, *e_typeArgs);
            return ImTypeExp_Type{type};
        }
        ResultType Visit(REnumElemDecl* rTypeDecl) 
        {
            auto e_typeArgs = MakeRTypeArguments(outerTypeArgs, sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_typeArgs);

            auto* type = contexts.rFactory->MakeEnumElemType(rTypeDecl, *e_typeArgs);
            return ImTypeExp_Type{type};
        }

        ResultType Visit(RInterfaceDecl* rTypeDecl) 
        {
            // TODO: [71] 2026-07-18, interface 구현
            throw NotImplementedException{};

            /*auto e_typeArgs = MakeRTypeArguments(outerTypeArgs, sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_typeArgs);

            auto* type = contexts.rFactory->MakeInterfaceType(rTypeDecl, *e_typeArgs);
            return ImTypeExp_Type{type};*/
        }

        ResultType Visit(RLambdaDecl* rTypeDecl) 
        {
            // TODO: [65] 2026-07-06, RLambdaDecl제거, RStructDecl을 쓰도록 변경
            throw NotImplementedException{};
        }

        ResultType Visit(RTraitDecl* rTypeDecl) 
        {
            return ImTypeExp_Trait{rTypeDecl, outerTypeArgs};
        }
    };

    return Accept(Translator{outerTypeArgs, sMemberTypeArgs, contexts}, typeDecl);
}

expected<ImTypeExp, DiagPtr> TranslateImTypExpAndMemberToImTypeExp(ImTypeExp&& imTypeExp, string_view name, span<STypeExp*> sTypeArgs, SmTypeTranslationContexts& contexts)
{
    return move(imTypeExp).Visit([name, sTypeArgs, &contexts](auto&& imTypeExp) -> expected<ImTypeExp, DiagPtr> {
        using T = remove_cvref_t<decltype(imTypeExp)>;

        if constexpr (same_as<T, ImTypeExp_Namespaces>)
        {
            // TODO: [74] 2026-07-28, SmDeclRes, ImExp, IrExp, SmTypeRes의 RNamespaceGroup 구현
            throw NotImplementedException{};
        }
        else if constexpr (same_as <T, ImTypeExp_Class>)
        {
            auto* typeDecl = imTypeExp.appliedDecl.decl->GetTypeMember(RName::Normal(string{name}));
            return TranslateRTypeDeclAndSTypeArgsToImTypeExp(typeDecl, imTypeExp.appliedDecl.typeArgs, sTypeArgs, contexts);
        }
        else if constexpr (same_as<T, ImTypeExp_Struct>)
        {
            auto* typeDecl = imTypeExp.appliedDecl.decl->GetTypeMember(RName::Normal(string{name}));
            return TranslateRTypeDeclAndSTypeArgsToImTypeExp(typeDecl, imTypeExp.appliedDecl.typeArgs, sTypeArgs, contexts);
        }
        else if constexpr (same_as<T, ImTypeExp_Trait>)
        {
            // trait는 nested type을 지원하지 않는다
            return Error<Error_ResolveIdentifier_TraitCantHaveMember>();
        }
        else if constexpr (same_as<T, ImTypeExp_Type>)
        {
            return Error<Error_ResolveIdentifier_TraitCantHaveMember>();
        }
        else
            static_assert(false);
    });
}

expected<ImTypeExp, DiagPtr> TranslateSTypeExp_MemberToImTypeExp(STypeExp_Member* sTypeExp, SmTypeTranslationContexts& contexts)
{
    auto e_baseImTypeExp = TranslateSTypeExpToImTypeExp(sTypeExp->parentType, contexts);
    RETURN_ON_ERROR(e_baseImTypeExp);

    return TranslateImTypExpAndMemberToImTypeExp(move(*e_baseImTypeExp), sTypeExp->name, sTypeExp->typeArgs, contexts);
}

} // namespace Citron