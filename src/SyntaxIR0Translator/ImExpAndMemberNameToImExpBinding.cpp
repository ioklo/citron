#include "pch.h"
#include "ImExpAndMemberNameToImExpBinding.h"

#include <cassert>

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <Logging/Logger.h>
#include <IR0/RTypeFactory.h>
#include <IR0/RClassMemberVarDecl.h>
#include <IR0/RStructMemberVarDecl.h>
#include <IR0/RMember.h>
#include <IR0/RNamespaceDecl.h>
#include <IR0/RClassDecl.h>
#include <IR0/RStructDecl.h>
#include <IR0/REnumDecl.h>

#include "ScopeContext.h"

#include "ImExp.h"
#include "ReExp.h"

#include "ImExpToReExpTranslation.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

class StaticParentBinder : public RMemberVisitor
{
    RTypeArgumentsPtr typeArgsExceptOuter; // outer 제외
    ImExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;
    RTypeFactory& factory;

public:
    StaticParentBinder(const RTypeArgumentsPtr& typeArgsExceptOuter, ImExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
        : typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context), logger(logger), factory(factory)
    {
    }

    // NS.'NS'
    void Visit(RMember_Namespace& member) override
    {
        *result = MakePtr<ImNamespaceExp>(member.decl);
    }

    // NS.F
    void Visit(RMember_GlobalFuncs& member) override
    {
        *result = MakePtr<ImGlobalFuncsExp>(member.items, typeArgsExceptOuter);
    }

    // T.C
    void Visit(RMember_Class& member) override
    {
        // check access, TODO: ? 여기서 Access체크를 왜 하나? 이미 decl찾을때 access 체크를 했을텐데
        if (!context->CanAccess(*member.decl))
        {
            logger->Fatal_TryAccessingPrivateMember();
            *result = nullptr;
            return;
        }

        auto typeArgs = factory.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);

        *result = MakePtr<ImClassExp>(member.decl, std::move(typeArgs));
    }

    // C.F
    void Visit(RMember_ClassMemberFuncs& member) override
    {
        *result = MakePtr<ImClassMemberFuncsExp>(member.items, typeArgsExceptOuter, /*hasExplicitInstance*/ true, /*explicitInstance*/ nullptr);
    }

    // C.x
    void Visit(RMember_ClassMemberVar& member) override
    {
        if (!member.decl->bStatic)
        {
            logger->Fatal_CantGetInstanceMemberThroughType();
            *result = nullptr;
            return;
        }

        if (!context->CanAccess(*member.decl))
        {
            logger->Fatal_TryAccessingPrivateMember();
            *result = nullptr;
            return;
        }

        // variable은 typeArgs가 없다
        assert(typeArgsExceptOuter->GetCount() == 0);

        *result = MakePtr<ImClassMemberVarExp>(member.decl, member.typeArgs, /*hasExplicitInstance*/ true, /*explicitInstance*/ nullptr);
    }

    // T.S
    void Visit(RMember_Struct& member) override
    {
        // check access, TODO: ? 여기서 Access체크를 왜 하나? 이미 decl찾을때 access 체크를 했을텐데
        if (!context->CanAccess(*member.decl))
        {
            logger->Fatal_TryAccessingPrivateMember();
            *result = nullptr;
            return;
        }

        auto typeArgs = factory.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);

        *result = MakePtr<ImStructExp>(member.decl, std::move(typeArgs));
    }

    // S.F
    void Visit(RMember_StructMemberFuncs& member) override
    {
        *result = MakePtr<ImStructMemberFuncsExp>(member.items, typeArgsExceptOuter, /*hasExplicitInstance*/ true, /*explicitInstance*/ nullptr);
    }

    // S.x
    void Visit(RMember_StructMemberVar& member) override
    {
        if (!member.decl->bStatic)
        {
            logger->Fatal_CantGetInstanceMemberThroughType();
            *result = nullptr;
            return;
        }

        if (!context->CanAccess(*member.decl))
        {
            logger->Fatal_TryAccessingPrivateMember();
            *result = nullptr;
            return;
        }

        // variable은 typeArgs가 없다
        assert(typeArgsExceptOuter->GetCount() == 0);
        *result = MakePtr<ImStructMemberVarExp>(member.decl, member.typeArgs, /*hasExplicitInstance*/ true, /*explicitInstance*/ nullptr);
    }

    // T.E
    void Visit(RMember_Enum& member) override
    {
        // check access
        if (!context->CanAccess(*member.decl))
        {
            logger->Fatal_TryAccessingPrivateMember();
            *result = nullptr;
            return;
        }

        auto typeArgs = factory.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        *result = MakePtr<ImEnumExp>(member.decl, std::move(typeArgs));
    }

    // E.First
    void Visit(RMember_EnumElem& member) override
    {
        // EnumElem은 TypeArgs를 가질 수 없다
        assert(typeArgsExceptOuter->GetCount() == 0);
        *result = MakePtr<ImEnumElemExp>(member.decl, member.typeArgs);
    }

    // 표현 불가능
    void Visit(RMember_EnumElemMemberVar& member) override
    {
        throw RuntimeFatalException();
    }

    // 표현 불가능
    void Visit(RMember_LambdaMemberVar& member) override
    {
        throw RuntimeFatalException();
    }

    // 표현 불가능
    void Visit(RMember_TupleMemberVar& member) override
    {
        throw RuntimeFatalException();
    }
};

class InstanceParentBinder : public RMemberVisitor
{
    ReExpPtr reInstExp;
    RTypeArgumentsPtr typeArgsExceptOuter;
    ImExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;
    
    /*TranslationResult<IntermediateExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateExp>>.VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result)
    {
        return Fatal(A2014_ResolveIdentifier_MultipleCandidatesForMember);
    }*/

public:
    InstanceParentBinder(ReExpPtr&& reInstExp, const RTypeArgumentsPtr& typeArgsExceptOuter, ImExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger)
        : reInstExp(std::move(reInstExp)), typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context), logger(logger)
    {
    }

    // 표현 불가
    void Visit(RMember_Namespace& member) override 
    {   
        throw RuntimeFatalException();
    }

    // 표현 불가
    void Visit(RMember_GlobalFuncs& member) override 
    {   
        throw RuntimeFatalException();
    }

    // exp.C
    void Visit(RMember_Class& member) override 
    {
        logger->Fatal_CantGetTypeMemberThroughInstance();
        *result = nullptr;
    }

    // exp.F
    void Visit(RMember_ClassMemberFuncs& member) override 
    {   
        *result = MakePtr<ImClassMemberFuncsExp>(member.items, typeArgsExceptOuter, /*hasExplicitInstance*/ true, reInstExp);
    }

    // exp.x
    void Visit(RMember_ClassMemberVar& member) override 
    {   
        // static인지 검사
        if (member.decl->bStatic)
        {
            logger->Fatal_CantGetStaticMemberThroughInstance();
            *result = nullptr;
            return;
        }

        // access modifier 검사?
        if (!context->CanAccess(*member.decl))
        {
            logger->Fatal_TryAccessingPrivateMember();
            *result = nullptr;
            return;
        }

        *result = MakePtr<ImClassMemberVarExp>(member.decl, member.typeArgs, /*hasExplicitInstance*/ true, reInstExp);
    }

    // exp.S
    void Visit(RMember_Struct& member) override 
    {   
        logger->Fatal_CantGetTypeMemberThroughInstance();
        *result = nullptr;
    }

    // exp.F
    void Visit(RMember_StructMemberFuncs& member) override 
    {   
        *result = MakePtr<ImStructMemberFuncsExp>(member.items, typeArgsExceptOuter, /*hasExplicitInstance*/ true, reInstExp);
    }

    // exp.x
    void Visit(RMember_StructMemberVar& member) override 
    {   
        // static인지 검사
        if (member.decl->bStatic)
        {
            logger->Fatal_CantGetStaticMemberThroughInstance();
            *result = nullptr;
            return;
        }

        // access modifier 검사                            
        if (!context->CanAccess(*member.decl))
        {
            logger->Fatal_TryAccessingPrivateMember();
            *result = nullptr;
            return;
        }

        *result = MakePtr<ImStructMemberVarExp>(member.decl, member.typeArgs, /*hasExplicitInstance*/ true, reInstExp);
    }

    // exp.E
    void Visit(RMember_Enum& member) override 
    {
        logger->Fatal_CantGetTypeMemberThroughInstance();
        *result = nullptr;
    }

    // exp.First
    void Visit(RMember_EnumElem& member) override 
    {   
        logger->Fatal_CantGetTypeMemberThroughInstance();
        *result = nullptr;
    }

    // exp.firstX
    void Visit(RMember_EnumElemMemberVar& member) override 
    {   
        *result = MakePtr<ImEnumElemMemberVarExp>(member.decl, member.typeArgs, reInstExp);
    }

    // 표현 불가
    void Visit(RMember_LambdaMemberVar& member) override 
    {   
        throw RuntimeFatalException();
    }

    void Visit(RMember_TupleMemberVar& member) override 
    {
        throw NotImplementedException();
    }
};


} // namespace

// MemberParent And Id Binder
// (IntermediateExp, name, typeArgs) -> IntermediateExp
class ImExpAndMemberNameToImExpBinder : public ImExpVisitor
{
    std::string name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    ImExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;
    RTypeFactory& factory;

    void BindStaticParent(RDecl& decl, const RTypeArgumentsPtr& typeArgs)
    {
        auto member = decl.GetMember(typeArgs, RNormalName(name), typeArgsExceptOuter->GetCount());
        StaticParentBinder binder(typeArgsExceptOuter, result, context, logger, factory);
        member->Accept(binder);
    }

    void BindInstanceParent(ImExp& imExp)
    {
        auto reInstExp = TranslateImExpToReExp(imExp, context, logger);
        if (!reInstExp)
        {
            *result = nullptr;
            return;
        }

        auto type = reInstExp->GetType(factory);
        auto member = type->GetMember(RNormalName(name), typeArgsExceptOuter->GetCount());
        if (!member)
        {
            logger->Fatal_NotFound();
            *result = nullptr;
            return;
        }

        InstanceParentBinder binder(std::move(reInstExp), typeArgsExceptOuter, result, context, logger);
        member->Accept(binder);
    }

public:
    ImExpAndMemberNameToImExpBinder(const std::string& name, const RTypeArgumentsPtr& typeArgsExceptOuter, ImExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
        : name(name), typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context), logger(logger), factory(factory)
    {
    }

    void Visit(ImNamespaceExp& imExp) override 
    {
        return BindStaticParent(*imExp._namespace, factory.MakeTypeArguments(vector<RTypePtr>()));
    }

    void Visit(ImGlobalFuncsExp& imExp) override 
    {
        logger->Fatal_FuncCantHaveMember();
        *result = nullptr;
    }

    void Visit(ImTypeVarExp& imExp) override 
    {
        throw NotImplementedException();
    }

    void Visit(ImClassExp& imExp) override 
    {
        BindStaticParent(*imExp.classDecl, imExp.typeArgs);
    }

    void Visit(ImClassMemberFuncsExp& imExp) override 
    {
        logger->Fatal_FuncCantHaveMember();
        *result = nullptr;
    }

    void Visit(ImStructExp& imExp) override 
    {
        BindStaticParent(*imExp.structDecl, imExp.typeArgs);
    }

    void Visit(ImStructMemberFuncsExp& imExp) override 
    {
        logger->Fatal_FuncCantHaveMember();
        *result = nullptr;
    }

    // (E).F
    void Visit(ImEnumExp& imExp) override 
    {
        BindStaticParent(*imExp.decl, imExp.typeArgs);
    }

    void Visit(ImEnumElemExp& imExp) override 
    {
        logger->Fatal_EnumElemCantHaveMember();
        *result = nullptr;
    }

    void Visit(ImThisVarExp& imExp) override 
    {
        BindInstanceParent(imExp);
    }

    void Visit(ImLocalVarExp& imExp) override 
    {
        BindInstanceParent(imExp);
    }

    void Visit(ImLambdaMemberVarExp& imExp) override 
    {
        BindInstanceParent(imExp);
    }

    void Visit(ImClassMemberVarExp& imExp) override 
    {
        BindInstanceParent(imExp);
    }

    void Visit(ImStructMemberVarExp& imExp) override 
    {
        BindInstanceParent(imExp);
    }

    void Visit(ImEnumElemMemberVarExp& imExp) override 
    {
        BindInstanceParent(imExp);
    }

    void Visit(ImListIndexerExp& imExp) override 
    {
        throw NotImplementedException();
    }

    void Visit(ImLocalDerefExp& imExp) override 
    {
        BindInstanceParent(imExp);
    }

    void Visit(ImBoxDerefExp& imExp) override 
    {
        BindInstanceParent(imExp);
    }

    void Visit(ImElseExp& imExp) override 
    {
        BindInstanceParent(imExp);
    }
};

ImExpPtr BindImExpAndMemberNameToImExp(ImExp& imExp, const std::string& name, const RTypeArgumentsPtr& typeArgsExceptOuter, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    ImExpPtr boundImExp;
    ImExpAndMemberNameToImExpBinder binder(name, typeArgsExceptOuter, &boundImExp, context, logger, factory);
    imExp.Accept(binder);
    return boundImExp;
}

}