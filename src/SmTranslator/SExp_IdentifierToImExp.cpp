#include "SExp_IdentifierToImExp.h"
#include <cassert>
#include "Infra/Expected.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTypeArguments.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"
#include "TranslationContexts.h"
#include "SRTFactory.h"
#include "ImExp.h"
#include "BodyRes.h"
#include "Misc.h"

using namespace std;

namespace Citron {

namespace {
struct RDeclResTranslator
{
    using ResultType = expected<ImExp*, DiagPtr>;
    RTypeArguments* memberTypeArgs;
    TranslationContexts& contexts;

    template<typename TImExp, typename... TArgs> requires derived_from<TImExp, ImExp>
    ImExp* MakeImExp(TArgs&&... args)
    {
        return contexts.srtFactory->MakeImExp<TImExp>(forward<TArgs>(args)...);
    }

    template<typename TMLoc, typename... TArgs> requires derived_from<TMLoc, MLoc>
    ImExp* MakeImExp_ReExp_Loc(TArgs&&... args)
    {
        auto* loc = contexts.mFactory->MakeMLoc<TMLoc>(forward<TArgs>(args)...);
        return contexts.srtFactory->MakeImExp<ImExp_ReExp>(ReExp_Loc{loc});
    }

    RTypeArguments* MergeTypeArgs(RTypeArguments* outerTypeArgs, RTypeArguments* memberTypeArgs)
    {
        return contexts.rFactory->MergeTypeArguments(outerTypeArgs, memberTypeArgs);
    }

    ResultType operator()(auto& declRes) { return Visit(declRes); }

    ResultType Visit(RDeclRes_Namespace& declRes)
    {
        return MakeImExp<ImExp_Namespace>(declRes.decl);
    }

    ResultType Visit(RDeclRes_GlobalFuncs& declRes)
    {
        return MakeImExp<ImExp_GlobalFuncs>(declRes.items, memberTypeArgs);
    }

    ResultType Visit(RDeclRes_Class& declRes)
    {
        auto* typeArgs = MergeTypeArgs(declRes.outerTypeArgs, memberTypeArgs);
        return MakeImExp<ImExp_Class>(declRes.decl, typeArgs);
    }

    ResultType Visit(RDeclRes_ClassFuncs& declRes)
    {
        // BodyRes_RDeclRes는 SExp_Identifier에서 ResolveIdentifier를 통해서 얻게 되므로, ExplicitInstance가 없다
        return MakeImExp<ImExp_ClassFuncs>(declRes.items, memberTypeArgs, ImExpInstanceKind_Implicit{});
    }

    ResultType Visit(RDeclRes_ClassVar& declRes)
    {
        // BodyRes_RDeclRes는 SExp_Identifier에서 ResolveIdentifier를 통해서 얻게 되므로, ExplicitInstance가 없다
        assert(memberTypeArgs->GetCount() == 0);
        return MakeImExp<ImExp_ClassVar>(declRes.decl, declRes.typeArgs, ImExpInstanceKind_Implicit{});
    }

    ResultType Visit(RDeclRes_Struct& declRes)
    {
        auto* typeArgs = MergeTypeArgs(declRes.outerTypeArgs, memberTypeArgs);
        return MakeImExp<ImExp_Struct>(declRes.decl, typeArgs);
    }

    ResultType Visit(RDeclRes_StructFuncs& declRes)
    {
        // BodyRes_RDeclRes는 SExp_Identifier에서 ResolveIdentifier를 통해서 얻게 되므로, ExplicitInstance가 없다
        return MakeImExp<ImExp_StructFuncs>(declRes.items, memberTypeArgs, ImExpInstanceKind_Implicit{});
    }

    ResultType Visit(RDeclRes_StructVar& declRes)
    {
        // BodyRes_RDeclRes는 SExp_Identifier에서 ResolveIdentifier를 통해서 얻게 되므로, ExplicitInstance가 없다
        assert(memberTypeArgs->GetCount() == 0);
        return MakeImExp<ImExp_StructVar>(declRes.decl, declRes.typeArgs, ImExpInstanceKind_Implicit{});
    }

    ResultType Visit(RDeclRes_Enum& declRes)
    {
        auto* typeArgs = MergeTypeArgs(declRes.outerTypeArgs, memberTypeArgs);
        return MakeImExp<ImExp_Enum>(declRes.decl, typeArgs);
    }

    ResultType Visit(RDeclRes_EnumElem& declRes)
    {
        assert(memberTypeArgs->GetCount() == 0); // enumElem은 typeArgs를 갖을수 없다
        return MakeImExp<ImExp_EnumElem>(declRes.decl, declRes.outerTypeArgs);
    }

    ResultType Visit(RDeclRes_EnumElemVar& declRes)
    {
        // ResolveIdentifier를 통해서 EnumElemVar를 가져올수 없다. (instance의 type에 대해서 GetMember를 할때만 얻을 수 있다)
        // this가 EnumElem일 경우 가능할텐데, 지금은 그럴일이 없다
        throw RuntimeFatalException{};
    }

    ResultType Visit(RDeclRes_LambdaVar& declRes)
    {
        assert(memberTypeArgs->GetCount() == 0); // var에 typeArgs가 있을 수 없다
        return MakeImExp_ReExp_Loc<MLoc_LambdaVar>(declRes.decl, declRes.outerTypeArgs);
    }

    ResultType Visit(RDeclRes_TupleVar& declRes)
    {
        // ResolveIdentifier를 통해서 tuple var가 가능하려면 this가 tuple이어야 하는데 현재는 가능하지 않다
        throw RuntimeFatalException{};
    }

    ResultType Visit(RDeclRes_TypeVar& declRes)
    {
        // typeVar에 <typeArgs>를 지원하지 않는다
        assert(memberTypeArgs->GetCount() == 0);

        auto* typeVar = contexts.rFactory->MakeTypeVarType(declRes.decl);
        return MakeImExp<ImExp_TypeVar>(typeVar);
    }

    ResultType Visit(RDeclRes_FuncParam& declRes)
    {
        assert(memberTypeArgs->GetCount() == 0);

        if (declRes.funcParam.IsRef())
            return MakeImExp_ReExp_Loc<MLoc_LocalRef>(declRes.funcParam.name, declRes.funcParam.type);
        else
            return MakeImExp_ReExp_Loc<MLoc_LocalVar>(declRes.funcParam.name, declRes.funcParam.type);
    }
};

struct BodyResTranslator
{
    using ResultType = expected<ImExp*, DiagPtr>;
    RTypeArguments* memberTypeArgs;
    TranslationContexts& contexts;

    template<typename TMLoc, typename... TArgs> requires derived_from<TMLoc, MLoc>
    ImExp* MakeImExp_ReExp_Loc(TArgs&&... args)
    {
        auto* loc = contexts.mFactory->MakeMLoc<TMLoc>(forward<TArgs>(args)...);
        return contexts.srtFactory->MakeImExp<ImExp_ReExp>(ReExp_Loc{loc});
    }

    ResultType operator()(auto& bodyRes) { return Visit(bodyRes); }

    ResultType Visit(BodyRes_RDeclRes& bodyRes)
    {
        return bodyRes.declRes.Visit(RDeclResTranslator{memberTypeArgs, contexts});
    }

    ResultType Visit(BodyRes_LocalVar& bodyRes)
    {
        assert(memberTypeArgs->GetCount() == 0);
        return MakeImExp_ReExp_Loc<MLoc_LocalVar>(bodyRes.name, bodyRes.type);
    }

    ResultType Visit(BodyRes_LocalRef& bodyRes)
    {
        assert(memberTypeArgs->GetCount() == 0);
        return MakeImExp_ReExp_Loc<MLoc_LocalRef>(bodyRes.name, bodyRes.type);
    }

    ResultType Visit(BodyRes_NeedCapture& bodyRes)
    {
        // TODO: [42] BodyRes.NeedCapture구현
        throw NotImplementedException{};
    }

    ResultType Visit(BodyRes_ThisVar& bodyRes)
    {
        assert(memberTypeArgs->GetCount() == 0);
        return MakeImExp_ReExp_Loc<MLoc_This>(bodyRes.type);
    }
};

} // namespace 

expected<ImExp*, DiagPtr> TranslateSExp_IdentifierToImExp(SExp_Identifier* sExp, TranslationContexts& contexts)
{
    auto e_rMemberTypeArgs = MakeRTypeArgs(sExp->typeArgs, contexts);
    RETURN_ON_ERROR(e_rMemberTypeArgs);

    auto e_bodyRes = ResolveIdentifier(RName_Normal(sExp->value), (*e_rMemberTypeArgs)->GetCount(), contexts);
    RETURN_ON_ERROR(e_bodyRes);

    return e_bodyRes->Visit(BodyResTranslator{*e_rMemberTypeArgs, contexts});
}

} // namespace Citron