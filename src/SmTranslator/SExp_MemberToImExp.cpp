#include "SExp_MemberToImExp.h"
#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RNamespace.h"
#include "RSymbol/RClassDecl.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/REnumDecl.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTypeArguments.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"
#include "MIR/MCreate.h"
#include "SmFactory.h"
#include "ImExp.h"
#include "SExpToImExp.h"
#include "ImExpToReExp.h"
#include "SmTranslationContexts.h"
#include "SmFuncContext.h"
#include "Misc.h"
#include "ImExpTranslations.h"
#include "SmTypeUtil.h"
#include "SmDeclRes.h"

using namespace std;

namespace Citron {

namespace {

struct StaticBaseTranslator
{
    using ResultType = expected<ImExp*, DiagPtr>;

    RTypeArguments* memberTypeArgs; // rClass 제외
    SmTranslationContexts& contexts;

    ResultType operator()(auto&& declRes) { return Visit(declRes); }

    template<typename TImExp, typename... TArgs> requires derived_from<TImExp, ImExp>
    TImExp* MakeImExp(TArgs&&... args)
    {
        return contexts.smFactory->MakeImExp<TImExp>(std::forward<TArgs>(args)...);
    }

    // 기본 표현
    ResultType Visit(auto&& declRes)
    {
        throw RuntimeFatalException{};
    }

    // NS.'NS'
    ResultType Visit(SmDeclRes_Namespaces&& declRes)
    {
        return MakeImExp<ImExp_Namespaces>(move(declRes.namespaces));
    }

    // NS.F
    ResultType Visit(SmDeclRes_GlobalFuncs&& declRes)
    {
        return MakeImExp<ImExp_GlobalFuncs>(declRes.outerAppliedFuncDecls, memberTypeArgs);
    }

    // T.C
    ResultType Visit(SmDeclRes_Class&& declRes)
    {   
        // TODO: [43] ResolveIdentifier에서 AccessCheck를 할지, Verify패스를 따로 둘지 결정
        if (!contexts.funcContext->CanAccess(declRes.outerAppliedDecl.decl))
        {
            return Error<Error_ResolveIdentifier_TryAccessingPrivateMember>();
        }

        auto* typeArgs = contexts.rFactory->MergeTypeArguments(declRes.outerAppliedDecl.outerTypeArgs, memberTypeArgs);
        return MakeImExp<ImExp_Class>(declRes.outerAppliedDecl.decl, typeArgs);
    }

    // C.F
    ResultType Visit(SmDeclRes_ClassFuncs&& declRes)
    {
        return MakeImExp<ImExp_ClassFuncs>(declRes.outerAppliedFuncDecls, memberTypeArgs, ImExpInstanceKind_ExplicitStatic{});
    }

    // C.x
    ResultType Visit(SmDeclRes_ClassVar&& declRes)
    {
        if (!declRes.appliedDecl.decl->IsStatic())
        {
            return Error<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>();
        }

        if (!contexts.funcContext->CanAccess(declRes.appliedDecl.decl))
        {
            return Error<Error_ResolveIdentifier_TryAccessingPrivateMember>();
        }

        // variable은 typeArgs가 없다
        assert(memberTypeArgs->GetCount() == 0);

        return MakeImExp<ImExp_ClassVar>(declRes.appliedDecl.decl, declRes.appliedDecl.typeArgs, ImExpInstanceKind_ExplicitStatic{});
    }

    // T.S
    ResultType Visit(SmDeclRes_Struct&& declRes)
    {
        // check access, TODO: ? 여기서 Access체크를 왜 하나? 이미 decl찾을때 access 체크를 했을텐데
        if (!contexts.funcContext->CanAccess(declRes.outerAppliedDecl.decl))
        {
            return Error<Error_ResolveIdentifier_TryAccessingPrivateMember>();
        }

        auto typeArgs = contexts.rFactory->MergeTypeArguments(declRes.outerAppliedDecl.outerTypeArgs, memberTypeArgs);

        return MakeImExp<ImExp_Struct>(declRes.outerAppliedDecl.decl, typeArgs);
    }

    // S.F
    ResultType Visit(SmDeclRes_StructFuncs&& declRes)
    {
        return MakeImExp<ImExp_StructFuncs>(declRes.outerAppliedFuncDecls, memberTypeArgs, ImExpInstanceKind_ExplicitStatic{});
    }

    // S.x
    ResultType Visit(SmDeclRes_StructVar&& declRes)
    {
        if (!declRes.appliedDecl.decl->IsStatic())
        {
            return Error<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>();
        }

        if (!contexts.funcContext->CanAccess(declRes.appliedDecl.decl))
        {
            return Error<Error_ResolveIdentifier_TryAccessingPrivateMember>();
        }

        // variable은 typeArgs가 없다
        assert(memberTypeArgs->GetCount() == 0);
        return MakeImExp<ImExp_StructVar>(declRes.appliedDecl.decl, declRes.appliedDecl.typeArgs, ImExpInstanceKind_ExplicitStatic{});
    }

    // T.E
    ResultType Visit(SmDeclRes_Enum&& declRes)
    {
        // check access
        if (!contexts.funcContext->CanAccess(declRes.outerAppliedDecl.decl))
        {
            return Error<Error_ResolveIdentifier_TryAccessingPrivateMember>();
        }

        auto typeArgs = contexts.rFactory->MergeTypeArguments(declRes.outerAppliedDecl.outerTypeArgs, memberTypeArgs);
        return MakeImExp<ImExp_Enum>(declRes.outerAppliedDecl.decl, typeArgs);
    }

    // E.First
    ResultType Visit(SmDeclRes_EnumElem&& declRes)
    {
        // EnumElem은 TypeArgs를 가질 수 없다
        assert(memberTypeArgs->GetCount() == 0);
        return MakeImExp<ImExp_EnumElem>(declRes.outerAppliedDecl.decl, declRes.outerAppliedDecl.outerTypeArgs);
    }

    // ResultType Visit(SmDeclRes_EnumElemVar&& declRes); // S.x 표현 불가능
    // ResultType Visit(SmDeclRes_LambdaVar&& declRes); // S.x 표현 불가능
    // ResultType Visit(SmDeclRes_TupleVar&& declRes) // S.x 표현 불가능

    ResultType Visit(SmDeclRes_TypeVar&& declRes)
    {
        // TODO: [52] TypeVar정리
        throw NotImplementedException{};
    }
};

struct InstanceParentTranslator
{
    MLoc* mInstLoc;
    RTypeArguments* memberTypeArgs;
    SmTranslationContexts& contexts;

    using ResultType = expected<ImExp*, DiagPtr>;

    template<typename TImExp, typename... TArgs> requires derived_from<TImExp, ImExp>
    TImExp* MakeImExp(TArgs&&... args)
    {
        return contexts.smFactory->MakeImExp<TImExp>(std::forward<TArgs>(args)...);
    }

    template<typename TMLoc, typename... TArgs> requires derived_from<TMLoc, MLoc>
    TMLoc* MakeMLoc(TArgs&&... args)
    {
        return contexts.mFactory->MakeMLoc<TMLoc>(std::forward<TArgs>(args)...);
    }

    ResultType operator()(auto&& declRes) { return Visit(declRes); }

    // 기본, 표현 불가
    ResultType Visit(auto&& declRes)
    {
        throw RuntimeFatalException{};
    }

    // ResultType Visit(SmDeclRes_Namespace&& declRes); // x.NS 표현 불가
    // ResultType Visit(SmDeclRes_GlobalFuncs&& declRes); // x.F() 표현 불가

    // exp.C
    ResultType Visit(SmDeclRes_Class&& declRes)
    {
        return Error<Error_ResolveIdentifier_CantGetTypeMemberThroughInstance>();
    }

    // exp.F
    ResultType Visit(SmDeclRes_ClassFuncs&& declRes)
    {
        return MakeImExp<ImExp_ClassFuncs>(declRes.outerAppliedFuncDecls, memberTypeArgs, ImExpInstanceKind_ExplicitInstance{mInstLoc});
    }

    // exp.x
    ResultType Visit(SmDeclRes_ClassVar&& declRes)
    {
        // static인지 검사
        if (declRes.appliedDecl.decl->IsStatic())
        {
            return Error<Error_ResolveIdentifier_CantGetStaticMemberThroughInstance>();
        }

        // access modifier 검사?
        if (!contexts.funcContext->CanAccess(declRes.appliedDecl.decl))
        {
            return Error<Error_ResolveIdentifier_TryAccessingPrivateMember>();
        }

        return MakeImExp<ImExp_ClassVar>(declRes.appliedDecl.decl, declRes.appliedDecl.typeArgs, ImExpInstanceKind_ExplicitInstance{mInstLoc});
    }

    // exp.S
    ResultType Visit(SmDeclRes_Struct&& declRes)
    {
        return Error<Error_ResolveIdentifier_CantGetTypeMemberThroughInstance>();
    }

    // exp.F
    ResultType Visit(SmDeclRes_StructFuncs&& declRes)
    {
        return MakeImExp<ImExp_StructFuncs>(declRes.outerAppliedFuncDecls, memberTypeArgs, ImExpInstanceKind_ExplicitInstance{mInstLoc});
    }

    // exp.x
    ResultType Visit(SmDeclRes_StructVar&& declRes)
    {
        // static인지 검사
        if (declRes.appliedDecl.decl->IsStatic())
        {
            return Error<Error_ResolveIdentifier_CantGetStaticMemberThroughInstance>();
        }

        // access modifier 검사                            
        if (!contexts.funcContext->CanAccess(declRes.appliedDecl.decl))
        {
            return Error<Error_ResolveIdentifier_TryAccessingPrivateMember>();
        }

        return MakeImExp<ImExp_StructVar>(declRes.appliedDecl.decl, declRes.appliedDecl.typeArgs, ImExpInstanceKind_ExplicitInstance{mInstLoc});
    }

    // exp.E
    ResultType Visit(SmDeclRes_Enum&& declRes)
    {
        return Error<Error_ResolveIdentifier_CantGetTypeMemberThroughInstance>();
    }

    // exp.First
    ResultType Visit(SmDeclRes_EnumElem&& declRes)
    {
        return Error<Error_ResolveIdentifier_CantGetTypeMemberThroughInstance>();
    }

    // exp.firstX
    ResultType Visit(SmDeclRes_EnumElemVar&& declRes)
    {
        auto* loc = MakeMLoc<MLoc_EnumElemVar>(mInstLoc, declRes.outerAppliedDecl.decl, declRes.outerAppliedDecl.outerTypeArgs);
        return MakeImExp<ImExp_ReExp>(ReExp_Loc{loc});
    }

    // ResultType Visit(SmDeclRes_LambdaVar&& declRes); exp.lambdaVar // 표현 불가

    // exp.x
    ResultType Visit(SmDeclRes_TupleVar&& declRes) 
    {
        // TODO: [44] Tuple 구현
        throw NotImplementedException{};
    }

    // exp.T
    ResultType Visit(SmDeclRes_TypeVar&& declRes)
    {
        return Error<Error_ResolveIdentifier_CantGetTypeMemberThroughInstance>();
    }
};

struct MemberTranslator
{
    using ResultType = expected<ImExp*, DiagPtr>;
    RName memberName;
    RTypeArguments* memberTypeArgs;
    SmTranslationContexts& contexts;

    ResultType TranslateStaticParent(RDecl* decl, RTypeArguments* typeArgs)
    {
        auto o_member = decl->GetMember(memberName);
        if (!o_member)
            return Error<Error_ResolveIdentifier_NotFound>();

        // member의 typeArgs개수가 
        auto declRes = ToSmDeclRes(typeArgs, *o_member);

        StaticBaseTranslator binder{memberTypeArgs, contexts};
        return move(declRes).Visit(binder);
    }

    ResultType TranslateInstanceParent(ReExp& reExp)
    {
        // materialize
        auto e_mLoc = visit([this](auto& reExp) -> expected<MLoc*, DiagPtr> {
            using T = remove_cvref_t<decltype(reExp)>;

            if constexpr (same_as<T, ReExp_Loc>)
                return reExp.mLoc;
            else if constexpr (same_as<T, ReExp_Exp>)
                return contexts.mFactory->MakeMLoc<MLoc_Materialize>(MCreate_BC(reExp.mExp));
            else if constexpr (same_as<T, ReExp_InitExp>)
                return contexts.mFactory->MakeMLoc<MLoc_Materialize>(MCreate_NBC(reExp.mInitExp));
            else if constexpr (same_as<T, ReExp_StmtCall>)
                return Error<Error_ResolveIdentifier_MemberBaseCantBeLocation>();
            else if constexpr (same_as<T, ReExp_StmtAssign>)
                return Error<Error_ResolveIdentifier_MemberBaseCantBeLocation>();
            else static_assert(false);
        }, reExp);
        RETURN_ON_ERROR(e_mLoc);

        auto* type = GetType(*e_mLoc, &*contexts.rFactory);

        auto o_declRes = GetMember(type, memberName);
        if (!o_declRes)
            return Error<Error_ResolveIdentifier_NotFound>();

        InstanceParentTranslator binder{*e_mLoc, memberTypeArgs, contexts};
        return o_declRes->Visit(binder);
    }

    ResultType Visit(ImExp_Namespaces* imExp) 
    { 
        // TODO: [74] 2026-07-28, SmDeclRes, ImExp, IrExp의 RNamespaceGroup 구현
        throw NotImplementedException{};
        // return TranslateStaticParent(imExp->namespaces, contexts.rFactory->MakeEmptyTypeArguments());
    }

    ResultType Visit(ImExp_GlobalFuncs* imExp)
    {
        return Error<Error_ResolveIdentifier_FuncCantHaveMember>();
    }

    ResultType Visit(ImExp_TypeVar* imExp)
    {
        throw NotImplementedException{};
    }

    ResultType Visit(ImExp_Class* imExp)
    {
        return TranslateStaticParent(imExp->classDecl, imExp->typeArgs);
    }

    ResultType Visit(ImExp_ClassFuncs* imExp)
    {
        return Error<Error_ResolveIdentifier_FuncCantHaveMember>();
    }

    ResultType Visit(ImExp_Struct* imExp)
    {
        return TranslateStaticParent(imExp->structDecl, imExp->typeArgs);
    }

    ResultType Visit(ImExp_StructFuncs* imExp)
    {
        return Error<Error_ResolveIdentifier_FuncCantHaveMember>();
    }

    // (E).F
    ResultType Visit(ImExp_Enum* imExp)
    {
        return TranslateStaticParent(imExp->decl, imExp->typeArgs);
    }

    ResultType Visit(ImExp_EnumElem* imExp)
    {
        return Error<Error_ResolveIdentifier_EnumElemCantHaveMember>();
    }

    ResultType Visit(ImExp_ClassVar* imExp)
    {
        ReExp reClassVar = ReExp_Loc{TranslateImExp_ClassVarToMLoc_ClassVar(imExp, contexts)};
        return TranslateInstanceParent(reClassVar);
    }

    ResultType Visit(ImExp_StructVar* imExp)
    {
        ReExp reStructVar = ReExp_Loc{TranslateImExp_StructVarToMLoc_StructVar(imExp, contexts)};
        return TranslateInstanceParent(reStructVar);
    }

    ResultType Visit(ImExp_ReExp* imExp)
    {
        return TranslateInstanceParent(imExp->reExp);
    }
};

} // 

expected<ImExp*, DiagPtr> TranslateSExp_MemberToImExp(SExp_Member* sExp, SmTranslationContexts& contexts)
{
    auto e_imBase = TranslateSExpToImExp(sExp->base, /*hintType*/nullptr, contexts);
    RETURN_ON_ERROR(e_imBase);

    auto e_rMemberTypeArgs = MakeRTypeArgs(sExp->memberTypeArgs, contexts);
    RETURN_ON_ERROR(e_rMemberTypeArgs);

    return Accept(MemberTranslator{RName_Normal{sExp->memberName}, *e_rMemberTypeArgs, contexts}, *e_imBase);
}

} // namespace Citron