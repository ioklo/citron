#include "ImExpAndMemberNameToImExpTranslation.h"

#include <cassert>
#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"

#include "RSymbol/RDeclRes.h"
#include "RSymbol/RClassDecl.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RTypeArguments.h"
#include "RSymbol/RNamespaceDecl.h"
#include "RSymbol/REnumDecl.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"

#include "TranslationContexts.h"
#include "SRTFactory.h"
#include "FuncContext.h"
#include "ScopeContext.h"
#include "ImExp.h"
#include "ReExp.h"
#include "ImExpToReExpTranslation.h"

using namespace std;

namespace Citron {

namespace {

class StaticParentTranslator
{
    RTypeArguments* typeArgsExceptOuter; // outer 제외
    TranslationContexts& contexts;

public:
    StaticParentTranslator(RTypeArguments* typeArgsExceptOuter, TranslationContexts& contexts)
        : typeArgsExceptOuter{typeArgsExceptOuter}, contexts{contexts}
    {
    }

    expected<ImExp*, DiagPtr> operator()(auto& member) { return Visit(member); }

    template<typename TImExp, typename... TArgs>
    TImExp* MakeImExp(TArgs&&... args)
    {
        return contexts.srtFactory->MakeImExp<TImExp>(std::forward<TArgs>(args)...);
    }

    // NS.'NS'
    expected<ImExp*, DiagPtr> Visit(RDeclRes_Namespace& member) 
    { 
        return MakeImExp<ImExp_Namespace>(member.decl); 
    }

    // NS.F
    expected<ImExp*, DiagPtr> Visit(RDeclRes_GlobalFuncs& member) 
    { 
        return MakeImExp<ImExp_GlobalFuncs>(member.items, typeArgsExceptOuter);
    }

    // T.C
    expected<ImExp*, DiagPtr> Visit(RDeclRes_Class& member)
    {
        // check access, TODO: ? 여기서 Access체크를 왜 하나? 이미 decl찾을때 access 체크를 했을텐데
        if (!contexts.funcContext->CanAccess(member.decl))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        auto* typeArgs = contexts.rFactory->MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        return MakeImExp<ImExp_Class>(member.decl, typeArgs);
    }

    // C.F
    expected<ImExp*, DiagPtr> Visit(RDeclRes_ClassFuncs& member)
    {
        return MakeImExp<ImExp_ClassFuncs>(member.items, typeArgsExceptOuter, /*hasExplicitInstance*/true, /*explicitInstance*/nullptr);
    }

    // C.x
    expected<ImExp*, DiagPtr> Visit(RDeclRes_ClassVar& member)
    {
        if (!member.decl->IsStatic())
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>()};
        }

        if (!contexts.funcContext->CanAccess(member.decl))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        // variable은 typeArgs가 없다
        assert(typeArgsExceptOuter->GetCount() == 0);

        return MakeImExp<ImExp_ClassVar>(member.decl, member.typeArgs, /*hasExplicitInstance*/true, /*explicitInstance*/nullptr);
    }

    // T.S
    expected<ImExp*, DiagPtr> Visit(RDeclRes_Struct& member)
    {
        // check access, TODO: ? 여기서 Access체크를 왜 하나? 이미 decl찾을때 access 체크를 했을텐데
        if (!contexts.funcContext->CanAccess(member.decl))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        auto typeArgs = contexts.rFactory->MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);

        return MakeImExp<ImExp_Struct>(member.decl, typeArgs);
    }

    // S.F
    expected<ImExp*, DiagPtr> Visit(RDeclRes_StructFuncs& member)
    {
        return MakeImExp<ImExp_StructFuncs>(member.items, typeArgsExceptOuter, /*hasExplicitInstance*/true, /*explicitInstance*/nullptr);
    }

    // S.x
    expected<ImExp*, DiagPtr> Visit(RDeclRes_StructVar& member)
    {
        if (!member.decl->IsStatic())
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>()};
        }

        if (!contexts.funcContext->CanAccess(member.decl))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        // variable은 typeArgs가 없다
        assert(typeArgsExceptOuter->GetCount() == 0);
        return MakeImExp<ImExp_StructVar>(member.decl, member.typeArgs, /*hasExplicitInstance*/true, /*explicitInstance*/nullptr);
    }

    // T.E
    expected<ImExp*, DiagPtr> Visit(RDeclRes_Enum& member)
    {
        // check access
        if (!contexts.funcContext->CanAccess(member.decl))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        auto typeArgs = contexts.rFactory->MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        return MakeImExp<ImExp_Enum>(member.decl, typeArgs);
    }

    // E.First
    expected<ImExp*, DiagPtr> Visit(RDeclRes_EnumElem& member)
    {
        // EnumElem은 TypeArgs를 가질 수 없다
        assert(typeArgsExceptOuter->GetCount() == 0);
        return MakeImExp<ImExp_EnumElem>(member.decl, member.outerTypeArgs);
    }

    // 표현 불가능
    expected<ImExp*, DiagPtr> Visit(RDeclRes_EnumElemVar& member)
    {
        throw RuntimeFatalException{};
    }

    // 표현 불가능
    expected<ImExp*, DiagPtr> Visit(RDeclRes_LambdaVar& member)
    {
        throw RuntimeFatalException{};
    }

    // 표현 불가능
    expected<ImExp*, DiagPtr> Visit(RDeclRes_TupleVar& member)
    {
        throw RuntimeFatalException{};
    }

    // 
    expected<ImExp*, DiagPtr> Visit(RDeclRes_TypeVar& member)
    {
        throw NotImplementedException{};
    }

    expected<ImExp*, DiagPtr> Visit(BodyRes_LocalVar& member)
    {
        throw NotImplementedException{};
    }

    expected<ImExp*, DiagPtr> Visit(BodyRes_LocalRef& member)
    {
        throw NotImplementedException{};
    }

    expected<ImExp*, DiagPtr> Visit(BodyRes_ThisVar& member)
    {
        throw NotImplementedException{};
    }

};

class InstanceParentTranslator
{
    ReExp* reInstExp;
    RTypeArguments* typeArgsExceptOuter;

    TranslationContexts& contexts;
    
    /*TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result)
    {
        return Fatal(A2014_ResolveIdentifier_MultipleCandidatesForMember);
    }*/

    template<typename TImExp, typename... TArgs>
    TImExp* MakeImExp(TArgs&&... args)
    {
        return contexts.srtFactory->MakeImExp<TImExp>(std::forward<TArgs>(args)...);
    }

public:
    InstanceParentTranslator(ReExp* reInstExp, RTypeArguments* typeArgsExceptOuter, TranslationContexts& contexts)
        : reInstExp{reInstExp}, typeArgsExceptOuter{typeArgsExceptOuter}, contexts{contexts}
    {
    }

    expected<ImExp*, DiagPtr> operator()(auto& member) { return Visit(member); }

    // 표현 불가
    expected<ImExp*, DiagPtr> Visit(RDeclRes_Namespace& member)
    {   
        throw RuntimeFatalException{};
    }

    // 표현 불가
    expected<ImExp*, DiagPtr> Visit(RDeclRes_GlobalFuncs& member)
    {   
        throw RuntimeFatalException{};
    }

    // exp.C
    expected<ImExp*, DiagPtr> Visit(RDeclRes_Class& member)
    {
        return unexpected{MakePtr<Error_ResolveIdentifier_CantGetTypeMemberThroughInstance>()};
    }

    // exp.F
    expected<ImExp*, DiagPtr> Visit(RDeclRes_ClassFuncs& member)
    {   
        return MakeImExp<ImExp_ClassFuncs>(member.items, typeArgsExceptOuter, /*hasExplicitInstance*/true, reInstExp);
    }

    // exp.x
    expected<ImExp*, DiagPtr> Visit(RDeclRes_ClassVar& member)
    {   
        // static인지 검사
        if (member.decl->IsStatic())
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_CantGetStaticMemberThroughInstance>()};
        }

        // access modifier 검사?
        if (!contexts.funcContext->CanAccess(member.decl))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        return MakeImExp<ImExp_ClassVar>(member.decl, member.typeArgs, /*hasExplicitInstance*/true, reInstExp);
    }

    // exp.S
    expected<ImExp*, DiagPtr> Visit(RDeclRes_Struct& member)
    {   
        return unexpected{MakePtr<Error_ResolveIdentifier_CantGetTypeMemberThroughInstance>()};
    }

    // exp.F
    expected<ImExp*, DiagPtr> Visit(RDeclRes_StructFuncs& member)
    {   
        return MakeImExp<ImExp_StructFuncs>(member.items, typeArgsExceptOuter, /*hasExplicitInstance*/true, reInstExp);
    }

    // exp.x
    expected<ImExp*, DiagPtr> Visit(RDeclRes_StructVar& member)
    {   
        // static인지 검사
        if (member.decl->IsStatic())
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_CantGetStaticMemberThroughInstance>()};
        }

        // access modifier 검사                            
        if (!contexts.funcContext->CanAccess(member.decl))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        return MakeImExp<ImExp_StructVar>(member.decl, member.typeArgs, /*hasExplicitInstance*/true, reInstExp);
    }

    // exp.E
    expected<ImExp*, DiagPtr> Visit(RDeclRes_Enum& member)
    {
        return unexpected{MakePtr<Error_ResolveIdentifier_CantGetTypeMemberThroughInstance>()};
    }

    // exp.First
    expected<ImExp*, DiagPtr> Visit(RDeclRes_EnumElem& member)
    {   
        return unexpected{MakePtr<Error_ResolveIdentifier_CantGetTypeMemberThroughInstance>()};
    }

    // exp.firstX
    expected<ImExp*, DiagPtr> Visit(RDeclRes_EnumElemVar& member)
    {   
        return MakeImExp<ImExp_EnumElemVar>(member.decl, member.outerTypeArgs, reInstExp);
    }

    // 표현 불가
    expected<ImExp*, DiagPtr> Visit(RDeclRes_LambdaVar& member)
    {   
        throw RuntimeFatalException{};
    }

    expected<ImExp*, DiagPtr> Visit(RDeclRes_TupleVar& member)
    {
        throw NotImplementedException{};
    }

    expected<ImExp*, DiagPtr> Visit(RDeclRes_TypeVar& member)
    {
        throw NotImplementedException{};
    }

    expected<ImExp*, DiagPtr> Visit(BodyRes_LocalVar& member)
    {
        throw NotImplementedException{};
    }

    expected<ImExp*, DiagPtr> Visit(BodyRes_LocalRef& member)
    {
        throw NotImplementedException{};
    }

    expected<ImExp*, DiagPtr> Visit(BodyRes_ThisVar& member)
    {
        throw NotImplementedException{};
    }
};

// MemberParent And Id Binder
// (IntermediateExp, name, typeArgs) -> IntermediateExp
class ImExpAndMemberNameToImExpTranslator
{
public:
    using ResultType = expected<ImExp*, DiagPtr>;

private:
    string name;
    RTypeArguments* typeArgsExceptOuter;

    TranslationContexts& contexts;
    
    ResultType TranslateStaticParent(RDecl* decl, RTypeArguments* typeArgs)
    {
        auto o_member = decl->GetMember(typeArgs, RName_Normal(name), typeArgsExceptOuter->GetCount());
        StaticParentTranslator binder{typeArgsExceptOuter, contexts};
        return visit(binder, *o_member);
    }

    ResultType TranslateInstanceParent(ImExp* imExp)
    {
        auto e_reInstExp = TranslateImExpToReExp(imExp, contexts);
        RETURN_ON_ERROR(e_reInstExp);

        auto type = (*e_reInstExp)->GetType();
        auto o_member = type->GetMember(RName_Normal(name), typeArgsExceptOuter->GetCount());
        if (!o_member)
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_NotFound>()};
        }

        InstanceParentTranslator binder(*e_reInstExp, typeArgsExceptOuter, contexts);
        return visit(binder, *o_member);
    }

public:
    ImExpAndMemberNameToImExpTranslator(const std::string& name, RTypeArguments* typeArgsExceptOuter, TranslationContexts& contexts)
        : name{name}, typeArgsExceptOuter{typeArgsExceptOuter}, contexts{contexts}
    {
    }

    ResultType Visit(ImExp_Namespace* imExp)
    {
        return TranslateStaticParent(imExp->_namespace, contexts.rFactory->MakeTypeArguments({}));
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

    ResultType Visit(ImExp_ThisVar* imExp)
    {
        return TranslateInstanceParent(imExp);
    }

    ResultType Visit(ImExp_LocalVar* imExp)
    {
        return TranslateInstanceParent(imExp);
    }

    ResultType Visit(ImExp_LocalRef* imExp)
    {
        return TranslateInstanceParent(imExp);
    }

    ResultType Visit(ImExp_LambdaVar* imExp)
    {
        return TranslateInstanceParent(imExp);
    }

    ResultType Visit(ImExp_ClassVar* imExp)
    {
        return TranslateInstanceParent(imExp);
    }

    ResultType Visit(ImExp_StructVar* imExp)
    {
        return TranslateInstanceParent(imExp);
    }

    ResultType Visit(ImExp_EnumElemVar* imExp)
    {
        return TranslateInstanceParent(imExp);
    }

    ResultType Visit(ImExp_ListIndexer* imExp)
    {
        throw NotImplementedException{};
    }

    ResultType Visit(ImExp_PtrDeref* imExp)
    {
        return TranslateInstanceParent(imExp);
    }

    ResultType Visit(ImExp_SharedDeref* imExp)
    {
        return TranslateInstanceParent(imExp);
    }

    ResultType Visit(ImExp_Else* imExp)
    {
        return TranslateInstanceParent(imExp);
    }
};

} // namespace

expected<ImExp*, DiagPtr> TranslateImExpAndMemberNameToImExp(ImExp* imExp, const std::string& name, RTypeArguments* typeArgsExceptOuter, TranslationContexts& contexts)
{
    ImExpAndMemberNameToImExpTranslator binder{name, typeArgsExceptOuter, contexts};
    return Accept(binder, imExp);
}

}