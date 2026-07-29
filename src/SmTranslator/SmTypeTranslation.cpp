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
#include "RSymbol/RTypeParam.h"
#include "RSymbol/RFactory.h"
#include "SmTypeRes.h"
#include "SmFuncContext.h"
#include "SmDeclContext.h"

using namespace std;

namespace Citron {

namespace {

RType* HandleReservedType(std::string_view name, span<STypeExp*> typeArgs, RFactory* rFactory)
{
    if (name == "void" && typeArgs.empty())
        return rFactory->MakeVoidType();
    else if (name == "int" && typeArgs.empty())
        return rFactory->MakeIntType();
    else if (name == "string" && typeArgs.empty())
        return rFactory->MakeStringType();
    else if (name == "bool" && typeArgs.empty())
        return rFactory->MakeBoolType();
    return nullptr;
}

}

optional<SmTypeRes> SmTypeResolveScope::ResolveTypeIdentifier(InRef<RName> name, RFactory* rFactory)
{
    return visit([&name, rFactory](auto& scope) -> optional<SmTypeRes> {
        using T = remove_cvref_t<decltype(scope)>;
        if constexpr (same_as<T, SmTypeResolveScope_DeclHeader>)
        {   
            for (auto* typeParam : scope.typeParams)
                if (typeParam->GetName() == *name)
                    return SmTypeRes_TypeVar{typeParam};

            return scope.outerDeclContext->ResolveTypeIdentifier(name);
        }
        else if constexpr (same_as<T, SmTypeResolveScope_FuncContext>)
        {
            return scope.funcContext->ResolveTypeIdentifier(name);
        }
        else if constexpr (same_as<T, SmTypeResolveScope_DeclContext>)
        {
            return scope.declContext->ResolveTypeIdentifier(name);
        }
        else
            static_assert(false);
    }, v);
}

expected<RAppliedDecl<RTraitDecl>, DiagPtr> TranslateSTypeExpToRTrait(STypeExp* sTypeExp, SmTypeResolveScope scope, RFactory* rFactory)
{
    // TODO: [66] 2026-07-09, Trait, Extend 구현
    throw NotImplementedException{};
}

expected<RType*, DiagPtr> TranslateSTypeExpToRType(STypeExp* sTypeExp, SmTypeResolveScope scope, RFactory* rFactory)
{
    // TODO: BuildNonTypeSymbolContext::MakeType 에도 같은 코드가 있다
    struct Visitor
    {
        using ResultType = expected<RType*, DiagPtr>;

        SmTypeResolveScope scope;
        RFactory* rFactory;

        expected<RType*, DiagPtr> Visit(STypeExp_Id* idExp)
        {
            // 예약어 처리
            auto* type = HandleReservedType(idExp->name, idExp->typeArgs, rFactory);
            if (type) return type;

            auto o_rTypeRes = scope.ResolveTypeIdentifier(RName::Normal(idExp->name), rFactory);
            if (!o_rTypeRes) return nullptr;
           
            return MakeType(*o_rTypeRes, idExp->typeArgs, scope, rFactory);
        }

        expected<RType*, DiagPtr> Visit(STypeExp* e)
        {
            throw NotImplementedException{};
        }

    } visitor{move(scope), rFactory};

    return Accept(visitor, sTypeExp);
}


expected<RType*, DiagPtr> MakeType(SmTypeRes& typeRes, span<STypeExp*> sMemberTypeArgs, SmTypeResolveScope scope, RFactory* rFactory)
{
    return typeRes.Visit([sMemberTypeArgs, scope, rFactory](auto& typeRes) -> expected<RType*, DiagPtr> {
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

            auto e_memberTypeArgs = MakeRTypeArgs(sMemberTypeArgs, scope, rFactory);
            RETURN_ON_ERROR(e_memberTypeArgs);

            auto* typeArgs = rFactory->MergeTypeArguments(typeRes.outerAppliedDecl.outerTypeArgs, *e_memberTypeArgs);
            return rFactory->MakeClassType(typeRes.outerAppliedDecl.decl, typeArgs);
        }
        else if constexpr (same_as<T, SmTypeRes_Struct>)
        {
            // 타입 인자 검사
            if (typeRes.outerAppliedDecl.decl->GetTypeParamCount() != sMemberTypeArgs.size())
                return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();

            auto e_memberTypeArgs = MakeRTypeArgs(sMemberTypeArgs, scope, rFactory);
            RETURN_ON_ERROR(e_memberTypeArgs);

            auto* typeArgs = rFactory->MergeTypeArguments(typeRes.outerAppliedDecl.outerTypeArgs, *e_memberTypeArgs);
            return rFactory->MakeStructType(typeRes.outerAppliedDecl.decl, typeArgs);
        }
        else if constexpr (same_as<T, SmTypeRes_Enum>)
        {
            // 타입 인자 검사
            if (typeRes.outerAppliedDecl.decl->GetTypeParamCount() != sMemberTypeArgs.size())
                return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();

            auto e_memberTypeArgs = MakeRTypeArgs(sMemberTypeArgs, scope, rFactory);
            RETURN_ON_ERROR(e_memberTypeArgs);

            auto* typeArgs = rFactory->MergeTypeArguments(typeRes.outerAppliedDecl.outerTypeArgs, *e_memberTypeArgs);
            return rFactory->MakeEnumType(typeRes.outerAppliedDecl.decl, typeArgs);
        }
        else if constexpr (same_as<T, SmTypeRes_EnumElem>)
        {
            // 타입 인자 검사
            if (typeRes.outerAppliedDecl.decl->GetTypeParamCount() != sMemberTypeArgs.size())
                return Error<Error_ResolveIdentifier_TypeParamCountMismatch>();

            auto e_memberTypeArgs = MakeRTypeArgs(sMemberTypeArgs, scope, rFactory);
            RETURN_ON_ERROR(e_memberTypeArgs);

            auto* typeArgs = rFactory->MergeTypeArguments(typeRes.outerAppliedDecl.outerTypeArgs, *e_memberTypeArgs);
            return rFactory->MakeEnumElemType(typeRes.outerAppliedDecl.decl, typeArgs);
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
            return rFactory->MakeTypeVarType(typeRes.decl);
        }
        else if constexpr (same_as<T, SmTypeRes_Trait>)
        {
            return Error<Error_ResolveIdentifier_CantUseTraitAsType>();
        }
        else static_assert(false);
    });
}

expected<RTypeArguments*, DiagPtr> MakeRTypeArgs(std::span<STypeExp*> typeArgs, SmTypeResolveScope scope, RFactory* rFactory)
{
    std::vector<RType*> items;
    items.reserve(typeArgs.size());

    for (auto* typeArg : typeArgs)
    {
        auto e_type = TranslateSTypeExpToRType(typeArg, scope, rFactory);
        RETURN_ON_ERROR(e_type);

        items.push_back(*e_type);
    }

    return rFactory->MakeTypeArguments(items);
}

} // namespace Citron