module Citron.SyntaxIR0Translator:ImExpAndMemberNameToImExpTranslation;

import <cassert>;
import <expected>;

import Citron.Ptr;
import Citron.Exceptions;
import Citron.Logger;
import Citron.RDecls;
import Citron.NDecls;

import :TranslationContext;
import :FuncContext;
import :ScopeContext;
import :ImExp;
import :ReExp;
import :ImExpToReExpTranslation;

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

class StaticParentTranslator
{
    RTypeArgumentsPtr typeArgsExceptOuter; // outer 제외
    TranslationContext& context;

public:
    StaticParentTranslator(const RTypeArgumentsPtr& typeArgsExceptOuter, TranslationContext& context)
        : typeArgsExceptOuter(typeArgsExceptOuter), context(context)
    {
    }

    // NS.'NS'
    expected<ImExpPtr, DiagPtr> operator()(RMember_Namespace& member) 
    { 
        return MakePtr<ImExp_Namespace>(member.decl); 
    }

    // NS.F
    expected<ImExpPtr, DiagPtr> operator()(RMember_GlobalFuncs& member) 
    { 
        return MakePtr<ImExp_GlobalFuncs>(member.items, typeArgsExceptOuter); 
    }

    // T.C
    expected<ImExpPtr, DiagPtr> operator()(RMember_Class& member)
    {
        // check access, TODO: ? 여기서 Access체크를 왜 하나? 이미 decl찾을때 access 체크를 했을텐데
        if (!context.CanAccess(member.decl.get()))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        auto typeArgs = context.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);

        return MakePtr<ImExp_Class>(member.decl, move(typeArgs));
    }

    // C.F
    expected<ImExpPtr, DiagPtr> operator()(RMember_ClassFuncs& member)
    {
        return MakePtr<ImExp_ClassFuncs>(member.items, typeArgsExceptOuter, /*hasExplicitInstance*/ true, /*explicitInstance*/ nullptr);
    }

    // C.x
    expected<ImExpPtr, DiagPtr> operator()(RMember_ClassVar& member)
    {
        if (!member.decl->IsStatic())
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>()};
        }

        if (!context.CanAccess(member.decl.get()))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        // variable은 typeArgs가 없다
        assert(typeArgsExceptOuter->GetCount() == 0);

        return MakePtr<ImExp_ClassVar>(member.decl, member.typeArgs, /*hasExplicitInstance*/ true, /*explicitInstance*/ nullptr);
    }

    // T.S
    expected<ImExpPtr, DiagPtr> operator()(RMember_Struct& member)
    {
        // check access, TODO: ? 여기서 Access체크를 왜 하나? 이미 decl찾을때 access 체크를 했을텐데
        if (!context.CanAccess(member.decl.get()))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        auto typeArgs = context.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);

        return MakePtr<ImExp_Struct>(member.decl, move(typeArgs));
    }

    // S.F
    expected<ImExpPtr, DiagPtr> operator()(RMember_StructFuncs& member)
    {
        return MakePtr<ImExp_StructFuncs>(member.items, typeArgsExceptOuter, /*hasExplicitInstance*/ true, /*explicitInstance*/ nullptr);
    }

    // S.x
    expected<ImExpPtr, DiagPtr> operator()(RMember_StructVar& member)
    {
        if (!member.decl->IsStatic())
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>()};
        }

        if (!context.CanAccess(member.decl.get()))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        // variable은 typeArgs가 없다
        assert(typeArgsExceptOuter->GetCount() == 0);
        return MakePtr<ImExp_StructVar>(member.decl, member.typeArgs, /*hasExplicitInstance*/ true, /*explicitInstance*/ nullptr);
    }

    // T.E
    expected<ImExpPtr, DiagPtr>  operator()(RMember_Enum& member)
    {
        // check access
        if (!context.CanAccess(member.decl.get()))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        auto typeArgs = context.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        return MakePtr<ImExp_Enum>(member.decl, move(typeArgs));
    }

    // E.First
    expected<ImExpPtr, DiagPtr> operator()(RMember_EnumElem& member)
    {
        // EnumElem은 TypeArgs를 가질 수 없다
        assert(typeArgsExceptOuter->GetCount() == 0);
        return MakePtr<ImExp_EnumElem>(member.decl, member.outerTypeArgs);
    }

    // 표현 불가능
    expected<ImExpPtr, DiagPtr> operator()(RMember_EnumElemVar& member)
    {
        throw RuntimeFatalException();
    }

    // 표현 불가능
    expected<ImExpPtr, DiagPtr> operator()(RMember_LambdaVar& member)
    {
        throw RuntimeFatalException();
    }

    // 표현 불가능
    expected<ImExpPtr, DiagPtr> operator()(RMember_TupleVar& member)
    {
        throw RuntimeFatalException();
    }

    // 
    expected<ImExpPtr, DiagPtr> operator()(RMember_TypeVar& member)
    {
        throw NotImplementedException();
    }

    expected<ImExpPtr, DiagPtr> operator()(RMember_LocalVar& member)
    {
        throw NotImplementedException();
    }

    expected<ImExpPtr, DiagPtr> operator()(RMember_ThisVar& member)
    {
        throw NotImplementedException();
    }

};

class InstanceParentTranslator
{
    ReExpPtr reInstExp;
    RTypeArgumentsPtr typeArgsExceptOuter;

    TranslationContext& context;
    
    /*TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result)
    {
        return Fatal(A2014_ResolveIdentifier_MultipleCandidatesForMember);
    }*/

public:
    InstanceParentTranslator(ReExpPtr&& reInstExp, const RTypeArgumentsPtr& typeArgsExceptOuter, TranslationContext& context)
        : reInstExp(move(reInstExp)), typeArgsExceptOuter(typeArgsExceptOuter), context(context)
    {
    }

    // 표현 불가
    expected<ImExpPtr, DiagPtr> operator()(RMember_Namespace& member)
    {   
        throw RuntimeFatalException();
    }

    // 표현 불가
    expected<ImExpPtr, DiagPtr> operator()(RMember_GlobalFuncs& member)
    {   
        throw RuntimeFatalException();
    }

    // exp.C
    expected<ImExpPtr, DiagPtr> operator()(RMember_Class& member)
    {
        return unexpected{MakePtr<Error_ResolveIdentifier_CantGetTypeMemberThroughInstance>()};
    }

    // exp.F
    expected<ImExpPtr, DiagPtr> operator()(RMember_ClassFuncs& member)
    {   
        return MakePtr<ImExp_ClassFuncs>(member.items, typeArgsExceptOuter, /*hasExplicitInstance*/ true, reInstExp);
    }

    // exp.x
    expected<ImExpPtr, DiagPtr> operator()(RMember_ClassVar& member)
    {   
        // static인지 검사
        if (member.decl->IsStatic())
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_CantGetStaticMemberThroughInstance>()};
        }

        // access modifier 검사?
        if (!context.CanAccess(member.decl.get()))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        return  MakePtr<ImExp_ClassVar>(member.decl, member.typeArgs, /*hasExplicitInstance*/ true, reInstExp);
    }

    // exp.S
    expected<ImExpPtr, DiagPtr> operator()(RMember_Struct& member)
    {   
        return unexpected{MakePtr<Error_ResolveIdentifier_CantGetTypeMemberThroughInstance>()};
    }

    // exp.F
    expected<ImExpPtr, DiagPtr> operator()(RMember_StructFuncs& member)
    {   
        return MakePtr<ImExp_StructFuncs>(member.items, typeArgsExceptOuter, /*hasExplicitInstance*/ true, reInstExp);
    }

    // exp.x
    expected<ImExpPtr, DiagPtr> operator()(RMember_StructVar& member)
    {   
        // static인지 검사
        if (member.decl->IsStatic())
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_CantGetStaticMemberThroughInstance>()};
        }

        // access modifier 검사                            
        if (!context.CanAccess(member.decl.get()))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        return MakePtr<ImExp_StructVar>(member.decl, member.typeArgs, /*hasExplicitInstance*/ true, reInstExp);
    }

    // exp.E
    expected<ImExpPtr, DiagPtr> operator()(RMember_Enum& member)
    {
        return unexpected{MakePtr<Error_ResolveIdentifier_CantGetTypeMemberThroughInstance>()};
    }

    // exp.First
    expected<ImExpPtr, DiagPtr> operator()(RMember_EnumElem& member)
    {   
        return unexpected{MakePtr<Error_ResolveIdentifier_CantGetTypeMemberThroughInstance>()};
    }

    // exp.firstX
    expected<ImExpPtr, DiagPtr> operator()(RMember_EnumElemVar& member)
    {   
        return MakePtr<ImExp_EnumElemVar>(member.decl, member.outerTypeArgs, reInstExp);
    }

    // 표현 불가
    expected<ImExpPtr, DiagPtr> operator()(RMember_LambdaVar& member)
    {   
        throw RuntimeFatalException();
    }

    expected<ImExpPtr, DiagPtr> operator()(RMember_TupleVar& member)
    {
        throw NotImplementedException();
    }

    expected<ImExpPtr, DiagPtr> operator()(RMember_TypeVar& member)
    {
        throw NotImplementedException();
    }

    expected<ImExpPtr, DiagPtr> operator()(RMember_LocalVar& member)
    {
        throw NotImplementedException();
    }

    expected<ImExpPtr, DiagPtr> operator()(RMember_ThisVar& member)
    {
        throw NotImplementedException();
    }
};

// MemberParent And Id Binder
// (IntermediateExp, name, typeArgs) -> IntermediateExp
class ImExpAndMemberNameToImExpTranslator : public ImExpVisitor
{
    expected<ImExpPtr, DiagPtr>* result;
    string name;
    RTypeArgumentsPtr typeArgsExceptOuter;

    TranslationContext& context;

    template<typename TValue, typename... TArgs> requires std::is_base_of_v<ImExp, TValue>
    void Value(TArgs&&... args)
    {
        *result = MakePtr<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    void Error(expected<TValue, DiagPtr>&& e)
    {
        *result = unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::is_base_of_v<Diag, TDiag>
    void Error(TArgs&&... args)
    {
        *result = unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

    void TranslateStaticParent(RDecl& decl, const RTypeArgumentsPtr& typeArgs)
    {
        auto oMember = decl.GetMember(typeArgs, RName_Normal(name), typeArgsExceptOuter->GetCount());
        StaticParentTranslator binder{typeArgsExceptOuter, context};
        *result = visit(binder, *oMember);
    }

    void TranslateInstanceParent(ImExp& imExp)
    {
        auto eReInstExp = TranslateImExpToReExp(imExp, context);
        if (!eReInstExp)
        {
            *result = unexpected{move(eReInstExp).error()};
            return;
        }

        auto type = context.GetType(**eReInstExp);
        auto oMember = type->GetMember(RName_Normal(name), typeArgsExceptOuter->GetCount());
        if (!oMember)
        {
            *result = unexpected{MakePtr<Error_ResolveIdentifier_NotFound>()};
            return;
        }

        InstanceParentTranslator binder(move(*eReInstExp), typeArgsExceptOuter, context);
        *result = visit(binder, *oMember);
    }

public:
    ImExpAndMemberNameToImExpTranslator(expected<ImExpPtr, DiagPtr>* result, const std::string& name, const RTypeArgumentsPtr& typeArgsExceptOuter, TranslationContext& context)
        : result(result), name(name), typeArgsExceptOuter(typeArgsExceptOuter), context(context)
    {
    }

    void Visit(ImExp_Namespace& imExp) override
    {
        return TranslateStaticParent(*imExp._namespace, context.MakeTypeArguments({}));
    }

    void Visit(ImExp_GlobalFuncs& imExp) override
    {
        return Error<Error_ResolveIdentifier_FuncCantHaveMember>();
    }

    void Visit(ImExp_TypeVar& imExp) override
    {
        throw NotImplementedException();
    }

    void Visit(ImExp_Class& imExp) override
    {
        return TranslateStaticParent(*imExp.classDecl, imExp.typeArgs);
    }

    void Visit(ImExp_ClassFuncs& imExp) override
    {
        return Error<Error_ResolveIdentifier_FuncCantHaveMember>();
    }

    void Visit(ImExp_Struct& imExp) override
    {
        return TranslateStaticParent(*imExp.structDecl, imExp.typeArgs);
    }

    void Visit(ImExp_StructFuncs& imExp) override
    {
        return Error<Error_ResolveIdentifier_FuncCantHaveMember>();
    }

    // (E).F
    void Visit(ImExp_Enum& imExp) override
    {
        return TranslateStaticParent(*imExp.decl, imExp.typeArgs);
    }

    void Visit(ImExp_EnumElem& imExp) override
    {
        return Error<Error_ResolveIdentifier_EnumElemCantHaveMember>();
    }

    void Visit(ImExp_ThisVar& imExp) override
    {
        return TranslateInstanceParent(imExp);
    }

    void Visit(ImExp_LocalVar& imExp) override
    {
        return TranslateInstanceParent(imExp);
    }

    void Visit(ImExp_LambdaVar& imExp) override
    {
        return TranslateInstanceParent(imExp);
    }

    void Visit(ImExp_ClassVar& imExp) override
    {
        return TranslateInstanceParent(imExp);
    }

    void Visit(ImExp_StructVar& imExp) override
    {
        return TranslateInstanceParent(imExp);
    }

    void Visit(ImExp_EnumElemVar& imExp) override
    {
        return TranslateInstanceParent(imExp);
    }

    void Visit(ImExp_ListIndexer& imExp) override
    {
        throw NotImplementedException();
    }

    void Visit(ImExp_LocalDeref& imExp) override
    {
        return TranslateInstanceParent(imExp);
    }

    void Visit(ImExp_BoxDeref& imExp) override
    {
        return TranslateInstanceParent(imExp);
    }

    void Visit(ImExp_Else& imExp) override
    {
        return TranslateInstanceParent(imExp);
    }
};

} // namespace

expected<ImExpPtr, DiagPtr> TranslateImExpAndMemberNameToImExp(ImExp& imExp, const std::string& name, const RTypeArgumentsPtr& typeArgsExceptOuter, TranslationContext& context)
{
    expected<ImExpPtr, DiagPtr> boundImExp;
    ImExpAndMemberNameToImExpTranslator binder{&boundImExp, name, typeArgsExceptOuter, context};
    imExp.Accept(binder);
    return boundImExp;
}

}