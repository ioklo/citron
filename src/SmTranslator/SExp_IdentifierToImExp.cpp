#include "SExp_IdentifierToImExp.h"
#include <cassert>
#include "Infra/Expected.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTypeArguments.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"
#include "SmTranslationContexts.h"
#include "SmFactory.h"
#include "ImExp.h"
#include "SmBodyRes.h"
#include "Misc.h"

using namespace std;

namespace Citron {

namespace {
struct SmDeclResTranslator
{
    using ResultType = expected<ImExp*, DiagPtr>;
    RTypeArguments* memberTypeArgs;
    SmTranslationContexts& contexts;

    template<typename TImExp, typename... TArgs> requires derived_from<TImExp, ImExp>
    ImExp* MakeImExp(TArgs&&... args)
    {
        return contexts.smFactory->MakeImExp<TImExp>(forward<TArgs>(args)...);
    }

    template<typename TMLoc, typename... TArgs> requires derived_from<TMLoc, MLoc>
    ImExp* MakeImExp_ReExp_Loc(TArgs&&... args)
    {
        auto* loc = contexts.mFactory->MakeMLoc<TMLoc>(forward<TArgs>(args)...);
        return contexts.smFactory->MakeImExp<ImExp_ReExp>(ReExp_Loc{loc});
    }

    RTypeArguments* MergeTypeArgs(RTypeArguments* outerTypeArgs, RTypeArguments* memberTypeArgs)
    {
        return contexts.rFactory->MergeTypeArguments(outerTypeArgs, memberTypeArgs);
    }

    ResultType operator()(auto&& declRes) { return Visit(std::forward<decltype(declRes)>(declRes)); }

    ResultType Visit(SmDeclRes_Namespaces&& declRes)
    {
        return MakeImExp<ImExp_Namespaces>(move(declRes.namespaces));
    }

    ResultType Visit(SmDeclRes_GlobalFuncs&& declRes)
    {
        return MakeImExp<ImExp_GlobalFuncs>(declRes.outerAppliedFuncDecls, memberTypeArgs);
    }

    ResultType Visit(SmDeclRes_Class&& declRes)
    {
        auto* typeArgs = MergeTypeArgs(declRes.outerAppliedDecl.outerTypeArgs, memberTypeArgs);
        return MakeImExp<ImExp_Class>(declRes.outerAppliedDecl.decl, typeArgs);
    }

    ResultType Visit(SmDeclRes_ClassFuncs&& declRes)
    {
        // BodyRes_SmDeclRes는 SExp_Identifier에서 ResolveIdentifier를 통해서 얻게 되므로, ExplicitInstance가 없다
        return MakeImExp<ImExp_ClassFuncs>(declRes.outerAppliedFuncDecls, memberTypeArgs, ImExpInstanceKind_Implicit{});
    }

    ResultType Visit(SmDeclRes_ClassVar&& declRes)
    {
        // BodyRes_SmDeclRes는 SExp_Identifier에서 ResolveIdentifier를 통해서 얻게 되므로, ExplicitInstance가 없다
        assert(memberTypeArgs->GetCount() == 0);
        return MakeImExp<ImExp_ClassVar>(declRes.appliedDecl.decl, declRes.appliedDecl.typeArgs, ImExpInstanceKind_Implicit{});
    }

    ResultType Visit(SmDeclRes_Struct&& declRes)
    {
        auto* typeArgs = MergeTypeArgs(declRes.outerAppliedDecl.outerTypeArgs, memberTypeArgs);
        return MakeImExp<ImExp_Struct>(declRes.outerAppliedDecl.decl, typeArgs);
    }

    ResultType Visit(SmDeclRes_StructFuncs&& declRes)
    {
        // BodyRes_SmDeclRes는 SExp_Identifier에서 ResolveIdentifier를 통해서 얻게 되므로, ExplicitInstance가 없다
        return MakeImExp<ImExp_StructFuncs>(declRes.outerAppliedFuncDecls, memberTypeArgs, ImExpInstanceKind_Implicit{});
    }

    ResultType Visit(SmDeclRes_StructVar&& declRes)
    {
        // BodyRes_SmDeclRes는 SExp_Identifier에서 ResolveIdentifier를 통해서 얻게 되므로, ExplicitInstance가 없다
        assert(memberTypeArgs->GetCount() == 0);
        return MakeImExp<ImExp_StructVar>(declRes.appliedDecl.decl, declRes.appliedDecl.typeArgs, ImExpInstanceKind_Implicit{});
    }

    ResultType Visit(SmDeclRes_Enum&& declRes)
    {
        auto* typeArgs = MergeTypeArgs(declRes.outerAppliedDecl.outerTypeArgs, memberTypeArgs);
        return MakeImExp<ImExp_Enum>(declRes.outerAppliedDecl.decl, typeArgs);
    }

    ResultType Visit(SmDeclRes_EnumElem&& declRes)
    {
        assert(memberTypeArgs->GetCount() == 0); // enumElem은 typeArgs를 갖을수 없다
        return MakeImExp<ImExp_EnumElem>(declRes.outerAppliedDecl.decl, declRes.outerAppliedDecl.outerTypeArgs);
    }

    ResultType Visit(SmDeclRes_EnumElemVar&& declRes)
    {
        // ResolveIdentifier를 통해서 EnumElemVar를 가져올수 없다. (instance의 type에 대해서 GetMember를 할때만 얻을 수 있다)
        // this가 EnumElem일 경우 가능할텐데, 지금은 그럴일이 없다
        throw RuntimeFatalException{};
    }

    ResultType Visit(SmDeclRes_Lambda&& declRes)
    {
        // TODO: [65] 2026-07-06, RLambdaDecl제거, RStructDecl을 쓰도록 변경
        throw NotImplementedException{};
    }

    ResultType Visit(SmDeclRes_LambdaVar&& declRes)
    {
        assert(memberTypeArgs->GetCount() == 0); // var에 typeArgs가 있을 수 없다
        return MakeImExp_ReExp_Loc<MLoc_LambdaVar>(declRes.outerAppliedDecl.decl, declRes.outerAppliedDecl.outerTypeArgs);
    }

    ResultType Visit(SmDeclRes_Interface&& declRes)
    {
        // TODO: [71] 2026-07-18, interface 구현
        throw NotImplementedException{};
    }

    ResultType Visit(SmDeclRes_TupleVar&& declRes)
    {
        // ResolveIdentifier를 통해서 tuple var가 가능하려면 this가 tuple이어야 하는데 현재는 가능하지 않다
        throw RuntimeFatalException{};
    }

    ResultType Visit(SmDeclRes_TypeVar&& declRes)
    {
        // typeVar에 <typeArgs>를 지원하지 않는다
        assert(memberTypeArgs->GetCount() == 0);

        auto* typeVar = contexts.rFactory->MakeTypeVarType(declRes.decl);
        return MakeImExp<ImExp_TypeVar>(typeVar);
    }
    
    ResultType Visit(SmDeclRes_Trait&& declRes)
    {
        return Error<Error_ResolveIdentifier_CantUseTraitAsExpression>();
    }

    ResultType Visit(SmDeclRes_TraitFuncs&& declRes)
    {
        return Error<Error_ResolveIdentifier_CantUseTraitFuncAsExpression>();
    }
};

struct BodyResTranslator
{
    using ResultType = expected<ImExp*, DiagPtr>;
    RTypeArguments* memberTypeArgs;
    SmTranslationContexts& contexts;

    template<typename TMLoc, typename... TArgs> requires derived_from<TMLoc, MLoc>
    ImExp* MakeImExp_ReExp_Loc(TArgs&&... args)
    {
        auto* loc = contexts.mFactory->MakeMLoc<TMLoc>(forward<TArgs>(args)...);
        return contexts.smFactory->MakeImExp<ImExp_ReExp>(ReExp_Loc{loc});
    }

    ResultType operator()(auto&& bodyRes) { return Visit(std::forward<decltype(bodyRes)>(bodyRes)); }

    ResultType Visit(SmBodyRes_DeclRes&& bodyRes)
    {
        return move(bodyRes.declRes).Visit(SmDeclResTranslator{memberTypeArgs, contexts});
    }

    ResultType Visit(SmBodyRes_LocalVar&& bodyRes)
    {
        assert(memberTypeArgs->GetCount() == 0);
        return MakeImExp_ReExp_Loc<MLoc_LocalVar>(bodyRes.name, bodyRes.type);
    }

    ResultType Visit(SmBodyRes_LocalRef&& bodyRes)
    {
        assert(memberTypeArgs->GetCount() == 0);
        return MakeImExp_ReExp_Loc<MLoc_LocalRef>(bodyRes.name, bodyRes.type);
    }

    ResultType Visit(SmBodyRes_NeedCapture&& bodyRes)
    {
        // TODO: [42] SmBodyRes.NeedCapture구현
        throw NotImplementedException{};
    }

    ResultType Visit(SmBodyRes_ThisVar&& bodyRes)
    {
        assert(memberTypeArgs->GetCount() == 0);
        return MakeImExp_ReExp_Loc<MLoc_This>(bodyRes.type);
    }
};

} // namespace 

expected<ImExp*, DiagPtr> TranslateSExp_IdentifierToImExp(SExp_Identifier* sExp, SmTranslationContexts& contexts)
{
    auto e_rMemberTypeArgs = MakeRTypeArgs(sExp->typeArgs, contexts);
    RETURN_ON_ERROR(e_rMemberTypeArgs);

    auto e_bodyRes = ResolveIdentifier(RName::Normal(sExp->value), contexts);
    RETURN_ON_ERROR(e_bodyRes);

    return move(*e_bodyRes).Visit(BodyResTranslator{*e_rMemberTypeArgs, contexts});
}

} // namespace Citron