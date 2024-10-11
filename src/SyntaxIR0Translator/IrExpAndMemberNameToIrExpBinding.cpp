#include "pch.h"
#include "IrExpAndMemberNameToIrExpBinding.h"

#include <cassert>

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <Logging/Logger.h>
#include <IR0/RNames.h>
#include <IR0/RMember.h>
#include <IR0/RTypeFactory.h>
#include <IR0/RClassDecl.h>
#include <IR0/RClassMemberVarDecl.h>
#include <IR0/RStructDecl.h>
#include <IR0/RStructMemberVarDecl.h>
#include <IR0/REnumDecl.h>
#include <IR0/RNamespaceDecl.h>

#include "IrExp.h"
#include "ScopeContext.h"

namespace Citron::SyntaxIR0Translator {

namespace {

class StaticParentBinder : public RMemberVisitor
{
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr *result;

    ScopeContextPtr context;
    LoggerPtr logger;
    RTypeFactory& factory;

public:
    StaticParentBinder(const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
        : typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context), logger(logger), factory(factory)
    {
    }

    void Visit(RMember_Namespace& member) override 
    {
        *result = MakePtr<IrExp_Namespace>(member.decl);
    }

    // S.F
    void Visit(RMember_GlobalFuncs& member) override 
    {   
        logger->Fatal_CantMakeReference();
        *result = nullptr;
    }

    void Visit(RMember_Class& member) override 
    {
        auto typeArgs = factory.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        *result = MakePtr<IrExp_Class>(member.decl, std::move(typeArgs));
    }

    // 에러,
    void Visit(RMember_ClassMemberFuncs& member) override 
    {
        logger->Fatal_CantMakeReference();
        *result = nullptr;
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

        assert(member.typeArgs->GetCount() == 0);
        *result = MakePtr<IrExp_StaticRef>(MakePtr<RLoc_ClassMember>(/*instance*/ nullptr, member.decl, member.typeArgs));
    }

    void Visit(RMember_Struct& member) override 
    {
        auto typeArgs = factory.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        *result = MakePtr<IrExp_Struct>(member.decl, std::move(typeArgs));
    }

    void Visit(RMember_StructMemberFuncs& member) override 
    {
        logger->Fatal_CantMakeReference();
        *result = nullptr;
    }

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

        assert(member.typeArgs->GetCount() == 0);
        *result = MakePtr<IrExp_StaticRef>(MakePtr<RLoc_StructMember>(/*instance*/ nullptr, member.decl, member.typeArgs));
    }

    // E
    void Visit(RMember_Enum& member) override 
    {   
        auto typeArgs = factory.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        *result = MakePtr<IrExp_Enum>(member.decl, std::move(typeArgs));
    }

    // &E.First.x
    void Visit(RMember_EnumElem& member) override 
    {   
        logger->Fatal_CantMakeReference();
        *result = nullptr;
    }

    // &E.x
    void Visit(RMember_EnumElemMemberVar& member) override 
    {
        // 표현 불가능
        throw RuntimeFatalException();
    }

    void Visit(RMember_LambdaMemberVar& member) override 
    {
        throw RuntimeFatalException();
    }

    void Visit(RMember_TupleMemberVar& member) override 
    {
        throw RuntimeFatalException();
    }

    /*TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result)
    {
        return Fatal(A2014_ResolveIdentifier_MultipleCandidatesForMember);
    }*/
};

class StaticRefTypeBinder : public RTypeVisitor
{
    std::shared_ptr<IrExp_StaticRef> parent;
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;

public:
    StaticRefTypeBinder(const std::shared_ptr<IrExp_StaticRef>& parent, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, 
        const ScopeContextPtr& context,
        const LoggerPtr& logger)
        : parent(parent), name(name), typeArgsExceptOuter(typeArgsExceptOuter), result(result)
        , context(context), logger(logger)
    {
    }

    // &C.optS.id
    void Visit(RType_NullableValue& type) override 
    {
        throw NotImplementedException();
    }

    // &C.optS.id
    void Visit(RType_NullableRef& type) override 
    {
        throw NotImplementedException();
    }

    void Visit(RType_TypeVar& type) override 
    {
        throw NotImplementedException();
    }

    void Visit(RType_Void& type) override 
    {
        // void인 멤버가 나올 수 없으므로
        throw RuntimeFatalException();
    }

    void Visit(RType_Tuple& type) override 
    {
        // TupleMemberLoc이 없으므로 일단 보류
        throw NotImplementedException();
        //int count = type.GetMemberVarCount();
        //for (int i = 0; i < count; i++)
        //{
        //    var memberVar = type.GetMemberVar(i);
        //    if (memberVar.GetName().Equals(name))
        //    {
        //        return Valid(new IntermediateRefExp.StaticRef(new TupleMemberLoc parent.Loc)
        //    }
        //}

        //return Fatal();
    }

    // &C.f.id
    void Visit(RType_Func& type) override
    {
        logger->Fatal_FuncInstanceCantHaveMember();
        *result = nullptr;        
    }

    // &C.pS.id;
    void Visit(RType_LocalPtr& type) override 
    {   
        logger->Fatal_LocalPtrCantHaveMember();
        *result = nullptr;
    }

    void Visit(RType_BoxPtr& type) override 
    {
        // &(C.x).a
        logger->Fatal_CantMakeReference();
        *result = nullptr;
    }

    void Visit(RType_Class& type) override 
    {
        auto memberVar = type.GetMemberVar(name);

        if (!memberVar)
        {
            logger->Fatal_NotFound();
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() == 0)
        {
            logger->Fatal_VarWithTypeArg();
            *result = nullptr;
            return;
        }

        // 이제 BoxRef로 변경
        *result = MakePtr<IrExp_BoxRef_ClassMember>(parent->loc, memberVar->decl, memberVar->typeArgs);
    }

    // &C.s.id
    void Visit(RType_Struct& type) override 
    {   
        auto memberVar = type.GetMemberVar(name);

        if (!memberVar)
        {
            logger->Fatal_NotFound();
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            logger->Fatal_VarWithTypeArg();
            *result = nullptr;
            return;
        }

        *result = MakePtr<IrExp_StaticRef>(MakePtr<RLoc_StructMember>(parent->loc, memberVar->decl, memberVar->typeArgs));
    }

    // Enum자체는 member를 가져올 수 없다
    void Visit(RType_Enum& type) override 
    {
        logger->Fatal_NotFound();
        *result = nullptr;
    }

    // e.x (E.Second.x)
    void Visit(RType_EnumElem& type) override 
    {   
        auto memberVar = type.GetMemberVar(name);
        if (!memberVar)
        {
            logger->Fatal_NotFound();
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            logger->Fatal_VarWithTypeArg();
            *result = nullptr;
        }

        *result = MakePtr<IrExp_StaticRef>(MakePtr<RLoc_EnumElemMember>(parent->loc, memberVar->decl, memberVar->typeArgs));
    }

    // &C.i.id
    void Visit(RType_Interface& type) override 
    {   
        throw NotImplementedException();
    }

    // &C.l.id
    void Visit(RType_Lambda& type) override 
    {   
        logger->Fatal_LambdaInstanceCantHaveMember();
        *result = nullptr;
    }
};

class BoxRefTypeBinder : public RTypeVisitor
{
    std::shared_ptr<IrExp_BoxRef> parent;
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;

public: 
    BoxRefTypeBinder(const std::shared_ptr<IrExp_BoxRef>& parent, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger)
        : parent(parent), name(name), typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context), logger(logger)
    {
    }

    void Visit(RType_NullableValue& type) override 
    {
        // &c.optS.x
        throw NotImplementedException();
    }

    void Visit(RType_NullableRef& type) override 
    {
        // &c.c.x
        throw NotImplementedException();
    }

    void Visit(RType_TypeVar& type) override 
    {
        // &c.t.x
        throw NotImplementedException();
    }

    void Visit(RType_Void& type) override 
    {
        // &c.v
        // void인 멤버가 나올 수 없으므로
        throw RuntimeFatalException();
    }

    void Visit(RType_Tuple& type) override 
    {
        // &c.t.x
        throw NotImplementedException();
    }

    void Visit(RType_Func& type) override 
    {
        // &c.f.x
        logger->Fatal_FuncInstanceCantHaveMember();
        *result = nullptr;
    }

    void Visit(RType_LocalPtr& type) override 
    {
        // &c.p.x
        logger->Fatal_LocalPtrCantHaveMember();
        *result = nullptr;
    }

    void Visit(RType_BoxPtr& type) override 
    {
        // &c.p.x, 문법에러        
        logger->Fatal_BoxPtrCantHaveMember();
        *result = nullptr;
    }

    void Visit(RType_Class& type) override 
    {
        // &c.c.x
        auto memberVar = type.GetMemberVar(name);
        if (!memberVar)
        {
            logger->Fatal_NotFound();
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            logger->Fatal_VarWithTypeArg();
            *result = nullptr;
            return;
        }

        *result = MakePtr<IrExp_BoxRef_ClassMember>(parent->MakeLoc(), memberVar->decl, memberVar->typeArgs);
    }

    void Visit(RType_Struct& type) override 
    {
        // &c.s.x
        auto memberVar = type.GetMemberVar(name);
        if (!memberVar)
        {
            logger->Fatal_NotFound();
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            logger->Fatal_VarWithTypeArg();
            *result = nullptr;
            return;
        }

        *result = MakePtr<IrExp_BoxRef_StructMember>(parent, memberVar->decl, memberVar->typeArgs);
    }

    void Visit(RType_Enum& type) override 
    {
        // &c.e.x
        logger->Fatal_EnumInstanceCantHaveMember();
        *result = nullptr;
    }

    void Visit(RType_EnumElem& type) override 
    {
        // &c.e.x
        throw NotImplementedException();
    }

    void Visit(RType_Interface& type) override 
    {
        // &c.i.x
        throw NotImplementedException();
    }

    void Visit(RType_Lambda& type) override 
    {
        // &c.l.x
        logger->Fatal_LambdaInstanceCantHaveMember();
        *result = nullptr;
    }
};

class LocalRefTypeBinder : public RTypeVisitor
{
    std::shared_ptr<IrExp_LocalRef> parent;
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;

public:
    LocalRefTypeBinder(const std::shared_ptr<IrExp_LocalRef>& parent, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger)
        : parent(parent), name(name), typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context), logger(logger)
    {
    }

    void Visit(RType_NullableValue& type) override 
    {
        // &s.optS.x
        throw NotImplementedException();
    }

    void Visit(RType_NullableRef& type) override 
    {
        // &s.c.x
        throw NotImplementedException();
    }

    void Visit(RType_TypeVar& type) override 
    {
        // &s.t.x
        throw NotImplementedException();
    }

    void Visit(RType_Void& type) override 
    {
        // void인 멤버가 나올 수 없으므로
        throw RuntimeFatalException();
    }

    void Visit(RType_Tuple& type) override 
    {
        // &s.t.x
        throw NotImplementedException();
    }

    void Visit(RType_Func& type) override 
    {
        // &s.f.x
        logger->Fatal_FuncInstanceCantHaveMember();
        *result = nullptr;
    }

    void Visit(RType_LocalPtr& type) override 
    {
        // &s.p.x
        logger->Fatal_LocalPtrCantHaveMember();
        *result = nullptr;
    }

    void Visit(RType_BoxPtr& type) override 
    {
        // &s.p.x
        logger->Fatal_BoxPtrCantHaveMember();
        *result = nullptr;
    }

    void Visit(RType_Class& type) override 
    {
        // &s.c.x
        auto memberVar = type.GetMemberVar(name);
        if (!memberVar)
        {
            logger->Fatal_NotFound();
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            logger->Fatal_VarWithTypeArg();
            *result = nullptr;
            return;
        }

        *result = MakePtr<IrExp_BoxRef_ClassMember>(parent->loc, memberVar->decl, memberVar->typeArgs);
    }

    void Visit(RType_Struct& type) override 
    {
        // &s.s.x
        auto memberVar = type.GetMemberVar(name);
        if (!memberVar)
        {
            logger->Fatal_NotFound();
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            logger->Fatal_VarWithTypeArg();
            *result = nullptr;
            return;
        }

        *result = MakePtr<IrExp_LocalRef>(MakePtr<RLoc_StructMember>(parent->loc, memberVar->decl, memberVar->typeArgs));
    }

    void Visit(RType_Enum& type) override 
    {
        // &s.e.x
        logger->Fatal_EnumInstanceCantHaveMember();
        *result = nullptr;
    }

    void Visit(RType_EnumElem& type) override 
    {
        // &s.e.x
        auto memberVar = type.GetMemberVar(name);
        if (!memberVar)
        {
            logger->Fatal_NotFound();
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            logger->Fatal_VarWithTypeArg();
            *result = nullptr;
        }

        *result = MakePtr<IrExp_LocalRef>(MakePtr<RLoc_EnumElemMember>(parent->loc, memberVar->decl, memberVar->typeArgs));
    }

    void Visit(RType_Interface& type) override 
    {
        // &s.i.x
        throw NotImplementedException();
    }

    void Visit(RType_Lambda& type) override 
    {
        // &s.l.x
        logger->Fatal_LambdaInstanceCantHaveMember();
        *result = nullptr;
    }
};

// *pS, valueType일때만 여기를 거치도록 나머지는 value로 가게
class BoxValueTypeBinder : public RTypeVisitor
{
    std::shared_ptr<IrExp_DerefedBoxValue> parent;
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;

public:
    BoxValueTypeBinder(const std::shared_ptr<IrExp_DerefedBoxValue>& parent, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger)
        : parent(parent), name(name), typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context), logger(logger)
    {
    }

    void Visit(RType_NullableValue& type) override 
    {
        // &(*pOptS).x
        throw NotImplementedException();
    }

    void Visit(RType_NullableRef& type) override 
    {
        // &(*c).x ?
        throw NotImplementedException();
    }

    void Visit(RType_TypeVar& type) override 
    {
        // &(*pT).x
        throw NotImplementedException();
    }

    void Visit(RType_Void& type) override 
    {
        throw RuntimeFatalException();
    }

    void Visit(RType_Tuple& type) override 
    {
        // &(*pT).x
        throw NotImplementedException();
    }

    void Visit(RType_Func& type) override 
    {
        // box ref contained
        throw RuntimeFatalException();
    }

    void Visit(RType_LocalPtr& type) override 
    {
        throw RuntimeFatalException();
    }

    void Visit(RType_BoxPtr& type) override 
    {
        logger->Fatal_BoxPtrCantHaveMember();
        *result = nullptr;
    }

    void Visit(RType_Class& type) override 
    {
        // &(*pC).x
        throw RuntimeFatalException();
    }

    void Visit(RType_Struct& type) override 
    {
        // &(*pS).x
        auto memberVar = type.GetMemberVar(name);
        if (!memberVar)
        {
            logger->Fatal_NotFound();
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            logger->Fatal_VarWithTypeArg();
            *result = nullptr;
            return;
        }

        *result = MakePtr<IrExp_BoxRef_StructIndirectMember>(parent->innerLoc, memberVar->decl, memberVar->typeArgs);
    }

    void Visit(RType_Enum& type) override 
    {
        // (*pE).x
        throw RuntimeFatalException();
    }

    void Visit(RType_EnumElem& type) override 
    {
        // box E.Second* pE = ...
        // &(*pE).x
        throw NotImplementedException();

        //var memberVar = type.Symbol.GetMemberVar(name);
        //if (memberVar == null)
        //    return Fatal();

        //return Valid(new IntermediateRefExp.BoxRef.EnumMember(parent, memberVar));
    }

    void Visit(RType_Interface& type) override 
    {
        // box ref contained
        throw RuntimeFatalException();
    }

    void Visit(RType_Lambda& type) override 
    {
        // doesn't have member variable
        logger->Fatal_LambdaInstanceCantHaveMember();
        *result = nullptr;
    }
};

class ThisTypeBinder : public RTypeVisitor
{   
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;
    RTypeFactory& factory;

public:
    ThisTypeBinder(const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
        : name(name), typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context), logger(logger), factory(factory)
    {
    }

    void Visit(RType_NullableValue& type) override 
    {
        // NullableValue는 멤버함수를 가질 수 없다
        throw RuntimeFatalException();
    }

    void Visit(RType_NullableRef& type) override 
    {
        // Nullable은 멤버함수를 가질 수 없다?
        throw RuntimeFatalException();
        static_assert(false);
    }

    void Visit(RType_TypeVar& type) override 
    {
        // TypeVar는 멤버함수를 가질 수 없다
        throw RuntimeFatalException();
    }

    void Visit(RType_Void& type) override 
    {
        // void는 멤버함수를 가질 수 없다
        throw RuntimeFatalException();
    }

    void Visit(RType_Tuple& type) override 
    {
        // Tuple은 멤버함수를 가질 수 없다
        throw RuntimeFatalException();
    }

    void Visit(RType_Func& type) override 
    {
        // Func가 멤버함수를 갖기 전까진 여기 들어오지 않는다
        throw RuntimeFatalException();
    }

    void Visit(RType_LocalPtr& type) override 
    {
        logger->Fatal_LocalPtrCantHaveMember();
        *result = nullptr;
    }

    void Visit(RType_BoxPtr& type) override 
    {
        logger->Fatal_BoxPtrCantHaveMember();
        *result = nullptr;
    }

    void Visit(RType_Class& type) override 
    {
        // &this.x
        auto memberVar = type.GetMemberVar(name);
        if (!memberVar)
        {
            logger->Fatal_NotFound();
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            logger->Fatal_VarWithTypeArg();
            *result = nullptr;
            return;
        }
        
        *result = MakePtr<IrExp_BoxRef_ClassMember>(context->MakeThisLoc(factory), memberVar->decl, memberVar->typeArgs);
    }

    void Visit(RType_Struct& type) override 
    {
        // &this.x
        // TODO: [10] box함수인 경우 에러 메시지를 다르게 해야 한다
        logger->Fatal_LocalPtrCantHaveMember();
        *result = nullptr;
    }

    void Visit(RType_Enum& type) override 
    {
        // Enum이 멤버 함수를 갖기 전까진 여기 들어오지 않는다
        throw RuntimeFatalException();
    }

    void Visit(RType_EnumElem& type) override 
    {
        // EnumElem이 멤버함수를 갖기 전까진 여기 들어오지 않는다
        throw RuntimeFatalException();
    }

    void Visit(RType_Interface& type) override 
    {
        // Interface가 멤버함수를 갖기 전까진 여기 들어오지 않는다
        throw RuntimeFatalException();
    }

    void Visit(RType_Lambda& type) override 
    {
        // Lambda는 멤버함수를 가질 수 없다
        throw RuntimeFatalException();
    }
};

class IrExpAndMemberNameToIrExpBinder : public IrExpVisitor
{
    IrExpPtr irThis;
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;
    RTypeFactory& factory;

public:
    IrExpAndMemberNameToIrExpBinder(const IrExpPtr& irThis, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
        : irThis(irThis), name(name), typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context), logger(logger), factory(factory)
    {
    }

    void HandleStaticParent(RDecl& decl, const RTypeArgumentsPtr& typeArgs)
    {
        auto member = decl.GetMember(typeArgs, name, typeArgsExceptOuter->GetCount());
        if (!member)
        {
            logger->Fatal_NotFound();
            *result = nullptr;
            return;
        }

        StaticParentBinder binder(typeArgsExceptOuter, result, context, logger, factory);
        member->Accept(binder);
    }

    void Visit(IrExp_Namespace& irExp) override 
    {
        return HandleStaticParent(*irExp.decl, factory.MakeTypeArguments({}));
    }

    void Visit(IrExp_TypeVar& irExp) override 
    {
        // 이건 진짜
        throw NotImplementedException();
    }

    void Visit(IrExp_Class& irExp) override 
    {
        return HandleStaticParent(*irExp.decl, irExp.typeArgs);
    }

    void Visit(IrExp_Struct& irExp) override 
    {
        return HandleStaticParent(*irExp.decl, irExp.typeArgs);
    }

    void Visit(IrExp_Enum& irExp) override 
    {
        return HandleStaticParent(*irExp.decl, irExp.typeArgs);
    }

    void Visit(IrExp_ThisVar& irExp) override 
    {
        // this.id        
        ThisTypeBinder binder(name, typeArgsExceptOuter, result, context, logger, factory);
        irExp.type->Accept(binder);
    }

    void Visit(IrExp_StaticRef& irExp) override 
    {
        auto irStaticRefThis = dynamic_pointer_cast<IrExp_StaticRef>(irThis);
        assert(irStaticRefThis);

        auto locType = irExp.loc->GetType(factory);

        // static ref가 부모이면
        StaticRefTypeBinder binder(irStaticRefThis, name, typeArgsExceptOuter, result, context, logger);
        locType->Accept(binder);
    }

    void Visit(IrExp_BoxRef& irExp) override 
    {
        auto irBoxRefThis = dynamic_pointer_cast<IrExp_BoxRef>(irThis);
        assert(irBoxRefThis);

        auto targetType = irExp.GetTargetType(factory);
        BoxRefTypeBinder binder(irBoxRefThis, name, typeArgsExceptOuter, result, context, logger);
        targetType->Accept(binder);
    }

    void Visit(IrExp_LocalRef& irExp) override 
    {
        auto irLocalRefThis = dynamic_pointer_cast<IrExp_LocalRef>(irThis);
        assert(irLocalRefThis);

        auto locType = irExp.loc->GetType(factory);

        LocalRefTypeBinder binder(irLocalRefThis, name, typeArgsExceptOuter, result, context, logger);
        locType->Accept(binder);
    }

    // *pS, 오직 value type에만 작동을 하도록 보장해야 한다
    void Visit(IrExp_DerefedBoxValue& irExp) override 
    {
        auto irDerefedBoxThis = dynamic_pointer_cast<IrExp_DerefedBoxValue>(irThis);
        assert(irDerefedBoxThis);

        auto innerType = irExp.innerLoc->GetType(factory);

        BoxValueTypeBinder binder(irDerefedBoxThis, name, typeArgsExceptOuter, result, context, logger);
        innerType->Accept(binder);
    }

    void Visit(IrExp_LocalValue& irExp) override 
    {
        // exp.id
        // 함수 호출 인자 제외 temp 참조 불가
        logger->Fatal_CantReferenceTempValue();
        *result = nullptr;
    }
};

} // namespace 

IrExpPtr BindIrExpAndMemberNameToIrExp(const IrExpPtr& irExp, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    IrExpPtr irBoundExp;
    IrExpAndMemberNameToIrExpBinder binder(irExp, name, typeArgsExceptOuter, &irBoundExp, context, logger, factory);
    irExp->Accept(binder);
    return irBoundExp;
}

} // namespace Citron::SyntaxIR0Translator

