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

            auto* classType = contexts.rFactory->MakeClassType(RAppliedDecl<RClassDecl>{rTypeDecl, *e_typeArgs});
            return ImTypeExp_Type{classType};
        }

        ResultType Visit(RStructDecl* rTypeDecl)
        {
            auto e_typeArgs = MakeRTypeArguments(outerTypeArgs, sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_typeArgs);

            auto* structType = contexts.rFactory->MakeStructType(RAppliedDecl<RStructDecl>{rTypeDecl, *e_typeArgs});
            return ImTypeExp_Type{structType};
        }

        ResultType Visit(REnumDecl* rTypeDecl) 
        {
            auto e_typeArgs = MakeRTypeArguments(outerTypeArgs, sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_typeArgs);

            auto* enumType = contexts.rFactory->MakeEnumType(RAppliedDecl<REnumDecl>{rTypeDecl, *e_typeArgs});
            return ImTypeExp_Type{enumType};
        }
        ResultType Visit(REnumElemDecl* rTypeDecl) 
        {
            auto e_typeArgs = MakeRTypeArguments(outerTypeArgs, sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_typeArgs);

            auto* enumElemType = contexts.rFactory->MakeEnumElemType(RAppliedDecl<REnumElemDecl>{rTypeDecl, *e_typeArgs});
            return ImTypeExp_Type{enumElemType};
        }

        ResultType Visit(RInterfaceDecl* rTypeDecl) 
        {
            // TODO: [71] 2026-07-18, interface 구현
            throw NotImplementedException{};

            /*auto e_typeArgs = MakeRTypeArguments(outerTypeArgs, sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_typeArgs);

            auto* enumType = contexts.rFactory->MakeInterfaceType(rTypeDecl, *e_typeArgs);
            return ImTypeExp_Type{enumType};*/
        }

        ResultType Visit(RLambdaDecl* rTypeDecl) 
        {
            // TODO: [65] 2026-07-06, RLambdaDecl제거, RStructDecl을 쓰도록 변경
            throw NotImplementedException{};
        }

        ResultType Visit(RTraitDecl* rTypeDecl) 
        {
            auto e_typeArgs = MakeRTypeArguments(outerTypeArgs, sMemberTypeArgs, contexts);
            RETURN_ON_ERROR(e_typeArgs);

            return ImTypeExp_Trait{rTypeDecl, *e_typeArgs};
        }
    };

    return Accept(Translator{outerTypeArgs, sMemberTypeArgs, contexts}, typeDecl);
}

expected<ImTypeExp, DiagPtr> TranslateRTypeAndMemberToImTypeExp(RType* type, string_view name, span<STypeExp*> sMemberTypeArgs, SmTypeTranslationContexts& contexts)
{
    struct Translator
    {
        using ResultType = expected<ImTypeExp, DiagPtr>;
        string_view name;
        span<STypeExp*> sMemberTypeArgs;
        SmTypeTranslationContexts& contexts;

        ResultType Visit(RType_Nullable* rType) { return Error<Error_ResolveIdentifier_TypeCantHaveTypeMember>(); }
        ResultType Visit(RType_NullableInplace* rType) { return Error<Error_ResolveIdentifier_TypeCantHaveTypeMember>(); }
        ResultType Visit(RType_TypeVar* rType) 
        { 
            // typevar에 trait가 있으면, 멤버 타입을 가질 수 있다
            // TODO: [66] 2026-07-09, Trait, Extend 구현
            throw NotImplementedException{}; 
        } 
        ResultType Visit(RType_Void* rType) { return Error<Error_ResolveIdentifier_TypeCantHaveTypeMember>(); }
        ResultType Visit(RType_Primitive* rType) { return Error<Error_ResolveIdentifier_TypeCantHaveTypeMember>(); }
        ResultType Visit(RType_Tuple* rType) { return Error<Error_ResolveIdentifier_TypeCantHaveTypeMember>(); }
        ResultType Visit(RType_Func* rType) { return Error<Error_ResolveIdentifier_TypeCantHaveTypeMember>(); }
        ResultType Visit(RType_Ptr* rType) { return Error<Error_ResolveIdentifier_TypeCantHaveTypeMember>(); }
        ResultType Visit(RType_Shared* rType) { return Error<Error_ResolveIdentifier_TypeCantHaveTypeMember>(); }
        ResultType Visit(RType_Box* rType) { return Error<Error_ResolveIdentifier_TypeCantHaveTypeMember>(); }
        ResultType Visit(RType_Class* rType) 
        {
            auto* rTypeDecl = rType->appliedDecl.decl->GetTypeMember(RName::Normal(string{name}));
            if (!rTypeDecl) return Error<Error_ResolveIdentifier_NotFound>();
            return TranslateRTypeDeclAndSTypeArgsToImTypeExp(rTypeDecl, rType->appliedDecl.typeArgs, sMemberTypeArgs, contexts);
        }
        ResultType Visit(RType_Struct* rType) 
        {
            auto* rTypeDecl = rType->appliedDecl.decl->GetTypeMember(RName::Normal(string{name}));
            if (!rTypeDecl) return Error<Error_ResolveIdentifier_NotFound>();
            return TranslateRTypeDeclAndSTypeArgsToImTypeExp(rTypeDecl, rType->appliedDecl.typeArgs, sMemberTypeArgs, contexts);
        }
        ResultType Visit(RType_Enum* rType) { return Error<Error_ResolveIdentifier_TypeCantHaveTypeMember>(); }
        ResultType Visit(RType_EnumElem* rType) { return Error<Error_ResolveIdentifier_TypeCantHaveTypeMember>(); }
        ResultType Visit(RType_Interface* rType) { return Error<Error_ResolveIdentifier_TypeCantHaveTypeMember>(); }
        ResultType Visit(RType_Lambda* rType) { return Error<Error_ResolveIdentifier_TypeCantHaveTypeMember>(); }
        ResultType Visit(RType_Opaque* rType) { return Error<Error_ResolveIdentifier_TypeCantHaveTypeMember>(); }
    };

    return Accept(Translator{name, sMemberTypeArgs, contexts}, type);
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
        else if constexpr (same_as<T, ImTypeExp_Trait>)
        {
            // trait는 nested type을 지원하지 않는다
            return Error<Error_ResolveIdentifier_TraitCantHaveMember>();
        }
        else if constexpr (same_as<T, ImTypeExp_Type>)
        {
            return TranslateRTypeAndMemberToImTypeExp(imTypeExp.type, name, sTypeArgs, contexts);
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