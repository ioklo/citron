module Citron.SyntaxIR0Translator:ImExpAndMemberNameToImExpTranslation;

import <cassert>;

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
    ImExpPtr operator()(RMember_Namespace& member) 
    { 
        return MakePtr<ImExp_Namespace>(member.decl); 
    }

    // NS.F
    ImExpPtr operator()(RMember_GlobalFuncs& member) { return MakePtr<ImExp_GlobalFuncs>(member.items, typeArgsExceptOuter); }

    // T.C
    ImExpPtr operator()(RMember_Class& member)
    {
        // check access, TODO: ? 여기서 Access체크를 왜 하나? 이미 decl찾을때 access 체크를 했을텐데
        if (!context.CanAccess(member.decl.get()))
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_TryAccessingPrivateMember);
            return nullptr;
        }

        auto typeArgs = context.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);

        return MakePtr<ImExp_Class>(member.decl, std::move(typeArgs));
    }

    // C.F
    ImExpPtr operator()(RMember_ClassFuncs& member)
    {
        return MakePtr<ImExp_ClassFuncs>(member.items, typeArgsExceptOuter, /*hasExplicitInstance*/ true, /*explicitInstance*/ nullptr);
    }

    // C.x
    ImExpPtr operator()(RMember_ClassVar& member)
    {
        if (!member.decl->IsStatic())
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_CantGetInstanceMemberThroughType);
            return nullptr;
        }

        if (!context.CanAccess(member.decl.get()))
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_TryAccessingPrivateMember);
            return nullptr;
        }

        // variable은 typeArgs가 없다
        assert(typeArgsExceptOuter->GetCount() == 0);

        return MakePtr<ImExp_ClassVar>(member.decl, member.typeArgs, /*hasExplicitInstance*/ true, /*explicitInstance*/ nullptr);
    }

    // T.S
    ImExpPtr operator()(RMember_Struct& member)
    {
        // check access, TODO: ? 여기서 Access체크를 왜 하나? 이미 decl찾을때 access 체크를 했을텐데
        if (!context.CanAccess(member.decl.get()))
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_TryAccessingPrivateMember);
            return nullptr;
        }

        auto typeArgs = context.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);

        return MakePtr<ImExp_Struct>(member.decl, std::move(typeArgs));
    }

    // S.F
    ImExpPtr operator()(RMember_StructFuncs& member)
    {
        return MakePtr<ImExp_StructFuncs>(member.items, typeArgsExceptOuter, /*hasExplicitInstance*/ true, /*explicitInstance*/ nullptr);
    }

    // S.x
    ImExpPtr operator()(RMember_StructVar& member)
    {
        if (!member.decl->IsStatic())
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_CantGetInstanceMemberThroughType);
            return nullptr;
        }

        if (!context.CanAccess(member.decl.get()))
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_TryAccessingPrivateMember);
            return nullptr;
        }

        // variable은 typeArgs가 없다
        assert(typeArgsExceptOuter->GetCount() == 0);
        return MakePtr<ImExp_StructVar>(member.decl, member.typeArgs, /*hasExplicitInstance*/ true, /*explicitInstance*/ nullptr);
    }

    // T.E
    ImExpPtr operator()(RMember_Enum& member)
    {
        // check access
        if (!context.CanAccess(member.decl.get()))
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_TryAccessingPrivateMember);
            return nullptr;
        }

        auto typeArgs = context.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        return MakePtr<ImExp_Enum>(member.decl, std::move(typeArgs));
    }

    // E.First
    ImExpPtr operator()(RMember_EnumElem& member)
    {
        // EnumElem은 TypeArgs를 가질 수 없다
        assert(typeArgsExceptOuter->GetCount() == 0);
        return MakePtr<ImExp_EnumElem>(member.decl, member.outerTypeArgs);
    }

    // 표현 불가능
    ImExpPtr operator()(RMember_EnumElemVar& member)
    {
        throw RuntimeFatalException();
    }

    // 표현 불가능
    ImExpPtr operator()(RMember_LambdaVar& member)
    {
        throw RuntimeFatalException();
    }

    // 표현 불가능
    ImExpPtr operator()(RMember_TupleVar& member)
    {
        throw RuntimeFatalException();
    }

    // 
    ImExpPtr operator()(RMember_TypeVar& member)
    {
        throw NotImplementedException();
    }

    ImExpPtr operator()(RMember_LocalVar& member)
    {
        throw NotImplementedException();
    }

    ImExpPtr operator()(RMember_ThisVar& member)
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
        : reInstExp(std::move(reInstExp)), typeArgsExceptOuter(typeArgsExceptOuter), context(context)
    {
    }

    // 표현 불가
    ImExpPtr operator()(RMember_Namespace& member) 
    {   
        throw RuntimeFatalException();
    }

    // 표현 불가
    ImExpPtr operator()(RMember_GlobalFuncs& member) 
    {   
        throw RuntimeFatalException();
    }

    // exp.C
    ImExpPtr operator()(RMember_Class& member) 
    {
        context.Log(&Logger::Fatal_ResolveIdentifier_CantGetTypeMemberThroughInstance);
        return nullptr;
    }

    // exp.F
    ImExpPtr operator()(RMember_ClassFuncs& member) 
    {   
        return MakePtr<ImExp_ClassFuncs>(member.items, typeArgsExceptOuter, /*hasExplicitInstance*/ true, reInstExp);
    }

    // exp.x
    ImExpPtr operator()(RMember_ClassVar& member) 
    {   
        // static인지 검사
        if (member.decl->IsStatic())
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_CantGetStaticMemberThroughInstance);
            return nullptr;
        }

        // access modifier 검사?
        if (!context.CanAccess(member.decl.get()))
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_TryAccessingPrivateMember);
            return nullptr;
        }

        return  MakePtr<ImExp_ClassVar>(member.decl, member.typeArgs, /*hasExplicitInstance*/ true, reInstExp);
    }

    // exp.S
    ImExpPtr operator()(RMember_Struct& member) 
    {   
        context.Log(&Logger::Fatal_ResolveIdentifier_CantGetTypeMemberThroughInstance);
        return nullptr;
    }

    // exp.F
    ImExpPtr operator()(RMember_StructFuncs& member) 
    {   
        return MakePtr<ImExp_StructFuncs>(member.items, typeArgsExceptOuter, /*hasExplicitInstance*/ true, reInstExp);
    }

    // exp.x
    ImExpPtr operator()(RMember_StructVar& member) 
    {   
        // static인지 검사
        if (member.decl->IsStatic())
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_CantGetStaticMemberThroughInstance);
            return nullptr;
        }

        // access modifier 검사                            
        if (!context.CanAccess(member.decl.get()))
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_TryAccessingPrivateMember);
            return nullptr;
        }

        return MakePtr<ImExp_StructVar>(member.decl, member.typeArgs, /*hasExplicitInstance*/ true, reInstExp);
    }

    // exp.E
    ImExpPtr operator()(RMember_Enum& member) 
    {
        context.Log(&Logger::Fatal_ResolveIdentifier_CantGetTypeMemberThroughInstance);
        return nullptr;
    }

    // exp.First
    ImExpPtr operator()(RMember_EnumElem& member) 
    {   
        context.Log(&Logger::Fatal_ResolveIdentifier_CantGetTypeMemberThroughInstance);
        return nullptr;
    }

    // exp.firstX
    ImExpPtr operator()(RMember_EnumElemVar& member) 
    {   
        return MakePtr<ImExp_EnumElemVar>(member.decl, member.outerTypeArgs, reInstExp);
    }

    // 표현 불가
    ImExpPtr operator()(RMember_LambdaVar& member) 
    {   
        throw RuntimeFatalException();
    }

    ImExpPtr operator()(RMember_TupleVar& member) 
    {
        throw NotImplementedException();
    }

    ImExpPtr operator()(RMember_TypeVar& member)
    {
        throw NotImplementedException();
    }

    ImExpPtr operator()(RMember_LocalVar& member)
    {
        throw NotImplementedException();
    }

    ImExpPtr operator()(RMember_ThisVar& member)
    {
        throw NotImplementedException();
    }
};

// MemberParent And Id Binder
// (IntermediateExp, name, typeArgs) -> IntermediateExp
class ImExpAndMemberNameToImExpTranslator : public ImExpVisitor
{
    std::string name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    ImExpPtr* result;

    TranslationContext& context;

    void TranslateStaticParent(RDecl& decl, const RTypeArgumentsPtr& typeArgs)
    {
        auto oMember = decl.GetMember(typeArgs, RName_Normal(name), typeArgsExceptOuter->GetCount());
        StaticParentTranslator binder{typeArgsExceptOuter, context};
        *result = visit(binder, *oMember);
    }

    void TranslateInstanceParent(ImExp& imExp)
    {
        auto reInstExp = TranslateImExpToReExp(imExp, context);
        if (!reInstExp)
        {
            *result = nullptr;
            return;
        }

        auto type = context.GetType(*reInstExp);
        auto oMember = type->GetMember(RName_Normal(name), typeArgsExceptOuter->GetCount());
        if (!oMember)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_NotFound);
            *result = nullptr;
            return;
        }

        InstanceParentTranslator binder(std::move(reInstExp), typeArgsExceptOuter, context);
        *result = visit(binder, *oMember);
    }

public:
    ImExpAndMemberNameToImExpTranslator(const std::string& name, const RTypeArgumentsPtr& typeArgsExceptOuter, ImExpPtr* result, TranslationContext& context)
        : name(name), typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context)
    {
    }

    void Visit(ImExp_Namespace& imExp) override
    {
        TranslateStaticParent(*imExp._namespace, context.MakeTypeArguments({}));
    }

    void Visit(ImExp_GlobalFuncs& imExp) override
    {
        context.Log(&Logger::Fatal_ResolveIdentifier_FuncCantHaveMember);
        *result = nullptr;
    }

    void Visit(ImExp_TypeVar& imExp) override
    {
        throw NotImplementedException();
    }

    void Visit(ImExp_Class& imExp) override
    {
        TranslateStaticParent(*imExp.classDecl, imExp.typeArgs);
    }

    void Visit(ImExp_ClassFuncs& imExp) override
    {
        context.Log(&Logger::Fatal_ResolveIdentifier_FuncCantHaveMember);
        *result = nullptr;
    }

    void Visit(ImExp_Struct& imExp) override
    {
        TranslateStaticParent(*imExp.structDecl, imExp.typeArgs);
    }

    void Visit(ImExp_StructFuncs& imExp) override
    {
        context.Log(&Logger::Fatal_ResolveIdentifier_FuncCantHaveMember);
        *result = nullptr;
    }

    // (E).F
    void Visit(ImExp_Enum& imExp) override
    {
        TranslateStaticParent(*imExp.decl, imExp.typeArgs);
    }

    void Visit(ImExp_EnumElem& imExp) override
    {
        context.Log(&Logger::Fatal_ResolveIdentifier_EnumElemCantHaveMember);
        *result = nullptr;
    }

    void Visit(ImExp_ThisVar& imExp) override
    {
        TranslateInstanceParent(imExp);
    }

    void Visit(ImExp_LocalVar& imExp) override
    {
        TranslateInstanceParent(imExp);
    }

    void Visit(ImExp_LambdaVar& imExp) override
    {
        TranslateInstanceParent(imExp);
    }

    void Visit(ImExp_ClassVar& imExp) override
    {
        TranslateInstanceParent(imExp);
    }

    void Visit(ImExp_StructVar& imExp) override
    {
        TranslateInstanceParent(imExp);
    }

    void Visit(ImExp_EnumElemVar& imExp) override
    {
        TranslateInstanceParent(imExp);
    }

    void Visit(ImExp_ListIndexer& imExp) override
    {
        throw NotImplementedException();
    }

    void Visit(ImExp_LocalDeref& imExp) override
    {
        TranslateInstanceParent(imExp);
    }

    void Visit(ImExp_BoxDeref& imExp) override
    {
        TranslateInstanceParent(imExp);
    }

    void Visit(ImExp_Else& imExp) override
    {
        TranslateInstanceParent(imExp);
    }
};

} // namespace

ImExpPtr TranslateImExpAndMemberNameToImExp(ImExp& imExp, const std::string& name, const RTypeArgumentsPtr& typeArgsExceptOuter, TranslationContext& context)
{
    ImExpPtr boundImExp;
    ImExpAndMemberNameToImExpTranslator binder(name, typeArgsExceptOuter, &boundImExp, context);
    imExp.Accept(binder);
    return boundImExp;
}

}