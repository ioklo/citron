#include "pch.h"
#include "IrExpAndMemberNameToIrExpBinding.h"

#include <cassert>

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <Logging/Logger.h>
#include <IR0/RNames.h>
#include <IR0/RMember.h>
#include <IR0/RTypeFactory.h>
#include <IR0/RClassMemberVarDecl.h>
#include <IR0/RStructMemberVarDecl.h>

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
        *result = MakePtr<IrNamespaceExp>(member.decl);
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
        *result = MakePtr<IrClassExp>(member.decl, std::move(typeArgs));
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
        *result = MakePtr<IrStaticRefExp>(MakePtr<RClassMemberLoc>(/*instance*/ nullptr, member.decl, member.typeArgs));
    }

    void Visit(RMember_Struct& member) override 
    {
        auto typeArgs = factory.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        *result = MakePtr<IrStructExp>(member.decl, std::move(typeArgs));
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
        *result = MakePtr<IrStaticRefExp>(MakePtr<RStructMemberLoc>(/*instance*/ nullptr, member.decl, member.typeArgs));
    }

    // E
    void Visit(RMember_Enum& member) override 
    {   
        auto typeArgs = factory.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        *result = MakePtr<IrEnumExp>(member.decl, std::move(typeArgs));
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
    std::shared_ptr<IrStaticRefExp> parent;
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;

public:
    StaticRefTypeBinder(const std::shared_ptr<IrStaticRefExp>& parent, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, 
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
        auto memberVar = type->GetMemberVar(name);

        if (memberVar == nullptr)
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
        *result = MakePtr<IrClassMemberBoxRefExp>(parent->loc, memberVar->decl, memberVar->typeArgs);
    }

    // &C.s.id
    void Visit(RType_Struct& type) override 
    {   
        auto memberVar = type.GetMemberVar(name);

        if (memberVar == nullptr)
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

        *result = MakePtr<IrStaticRefExp>(MakePtr<RStructMemberLoc>(parent->loc, memberVar->decl, memberVar->typeArgs));
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
        if (memberVar == nullptr)
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

        *result = MakePtr<IrStaticRefExp>(MakePtr<REnumElemMemberLoc>(parent->loc, memberVar->decl, memberVar->typeArgs));
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

struct BoxRefTypeBinder : public RTypeVisitor
{
    std::shared_ptr<IrBoxRefExp> parent;
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;

public: 
    BoxRefTypeBinder(const std::shared_ptr<IrBoxRefExp>& parent, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger)
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
        return Fatal(A2019_ResolveIdentifier_FuncInstanceCantHaveMember);
    }

    void Visit(RType_LocalPtr& type) override 
    {
        // &c.p.x
        return Fatal(A2017_ResolveIdentifier_LocalPtrCantHaveMember);
    }

    void Visit(RType_BoxPtr& type) override 
    {
        // &c.p.x, 문법에러
        return Fatal(A2018_ResolveIdentifier_BoxPtrCantHaveMember);
    }

    void Visit(RType_Class& type) override 
    {
        // &c.c.x
        var memberVar = type.GetMemberVar(name);
        if (memberVar == null)
            return Fatal(A2007_ResolveIdentifier_NotFound);

        if (typeArgs.Length != 0)
            return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);

        return Valid(new IntermediateRefExp.BoxRef.ClassMember(parent.MakeLoc(), memberVar));
    }

    void Visit(RType_Struct& type) override 
    {
        // &c.s.x
        var memberVar = type.GetMemberVar(name);
        if (memberVar == null)
            return Fatal(A2007_ResolveIdentifier_NotFound);

        if (typeArgs.Length != 0)
            return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);

        return Valid(new IntermediateRefExp.BoxRef.StructMember(parent, memberVar));
    }

    void Visit(RType_Enum& type) override 
    {
        // &c.e.x
        return Fatal(A2013_ResolveIdentifier_EnumInstanceCantHaveMember);
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
        return Fatal(A2016_ResolveIdentifier_LambdaInstanceCantHaveMember);
    }
};

struct LocalRefTypeVisitor : public RTypeVisitor
{
    std::shared_ptr<IrLocalRefExp> parent;
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;

public:
    LocalRefTypeVisitor(const std::shared_ptr<IrLocalRefExp>& parent, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger)
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
        return Fatal(A2019_ResolveIdentifier_FuncInstanceCantHaveMember);
    }

    void Visit(RType_LocalPtr& type) override 
    {
        // &s.p.x
        return Fatal(A2017_ResolveIdentifier_LocalPtrCantHaveMember);
    }

    void Visit(RType_BoxPtr& type) override 
    {
        // &s.p.x
        return Fatal(A2018_ResolveIdentifier_BoxPtrCantHaveMember);
    }

    void Visit(RType_Class& type) override 
    {
        // &s.c.x
        var memberVar = type.GetMemberVar(name);
        if (memberVar == null)
            return Fatal(A2007_ResolveIdentifier_NotFound);

        if (typeArgs.Length != 0)
            return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);

        return Valid(new IntermediateRefExp.BoxRef.ClassMember(parent.Loc, memberVar));
    }

    void Visit(RType_Struct& type) override 
    {
        // &s.s.x
        var memberVar = type.GetMemberVar(name);
        if (memberVar == null)
            return Fatal(A2007_ResolveIdentifier_NotFound);

        if (typeArgs.Length != 0)
            return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);

        return Valid(new IntermediateRefExp.LocalRef(new StructMemberLoc(parent.Loc, memberVar), memberVar.GetDeclType()));
    }

    void Visit(RType_Enum& type) override 
    {
        // &s.e.x
        return Fatal(A2013_ResolveIdentifier_EnumInstanceCantHaveMember);
    }

    void Visit(RType_EnumElem& type) override 
    {
        // &s.e.x
        var memberVar = type.GetMemberVar(name);
        if (memberVar == null)
            return Fatal(A2007_ResolveIdentifier_NotFound);

        if (typeArgs.Length != 0)
            return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);

        return Valid(new IntermediateRefExp.LocalRef(new EnumElemMemberLoc(parent.Loc, memberVar), memberVar.GetDeclType()));
    }

    void Visit(RType_Interface& type) override 
    {
        // &s.i.x
        throw NotImplementedException();
    }

    void Visit(RType_Lambda& type) override 
    {
        // &s.l.x
        return Fatal(A2016_ResolveIdentifier_LambdaInstanceCantHaveMember);
    }
};

// *pS, valueType일때만 여기를 거치도록 나머지는 value로 가게
struct BoxValueTypeVisitor : public RTypeVisitor
{
    std::shared_ptr<IrDerefedBoxValueExp> parent;
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;

public:
    BoxValueTypeVisitor(const std::shared_ptr<IrDerefedBoxValueExp>& parent, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger)
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
        return Fatal(A2018_ResolveIdentifier_BoxPtrCantHaveMember);
    }

    void Visit(RType_Class& type) override 
    {
        // &(*pC).x
        throw RuntimeFatalException();
    }

    void Visit(RType_Struct& type) override 
    {
        // &(*pS).x
        var memberVar = type.GetMemberVar(name);
        if (memberVar == null)
            return Fatal(A2007_ResolveIdentifier_NotFound);

        if (typeArgs.Length != 0)
            return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);

        return Valid(new IntermediateRefExp.BoxRef.StructIndirectMember(parent.InnerLoc, memberVar));
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
        return Fatal(A2016_ResolveIdentifier_LambdaInstanceCantHaveMember);
    }
};

struct ThisTypeVisitor : public RTypeVisitor
{
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;

public:
    ThisTypeVisitor(const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger)
        : name(name), typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context), logger(logger)
    {
    }

    void Visit(RType_NullableValue& type) override 
    {
        // Nullable은 멤버함수를 가질 수 없다
        throw RuntimeFatalException();
    }

    void Visit(RType_NullableRef& type) override 
    {
        // Nullable은 멤버함수를 가질 수 없다
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
        return Fatal(A2017_ResolveIdentifier_LocalPtrCantHaveMember);
    }

    void Visit(RType_BoxPtr& type) override 
    {
        return Fatal(A2018_ResolveIdentifier_BoxPtrCantHaveMember);
    }

    void Visit(RType_Class& type) override 
    {
        // &this.x
        if (typeArgs.Length != 0)
            return Fatal(A2002_ResolveIdentifier_VarWithTypeArg);

        var memberVar = type.GetMemberVar(name);
        if (memberVar == null)
            return Fatal(A2007_ResolveIdentifier_NotFound);

        return Valid(new IntermediateRefExp.BoxRef.ClassMember(new ThisLoc(), memberVar));
    }

    void Visit(RType_Struct& type) override 
    {
        // &this.x
        // TODO: [10] box함수인 경우 에러 메시지를 다르게 해야 한다
        return Fatal(A2017_ResolveIdentifier_LocalPtrCantHaveMember);
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

class IrExpAndMemberNameToRExpBinder : public IrExpVisitor
{
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    RExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;

public:
    IrExpAndMemberNameToRExpBinder(const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, RExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger)
        : name(name), typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context), logger(logger)
    {
    }

    void HandleStaticParent(ISymbolNode symbol)
    {
        var result = symbol.QueryMember(name, typeArgs.Length);
        if (result == null)
            return Fatal(A2007_ResolveIdentifier_NotFound);

        var binder = new StaticParentBinder();
        return result.Accept<StaticParentBinder, TranslationResult<IntermediateRefExp>>(ref binder);
    }

    void Visit(IrNamespaceExp& irExp) override 
    {
        return HandleStaticParent(imRefExp.Symbol);
    }

    void Visit(IrTypeVarExp& irExp) override 
    {
        // 이건 진짜
        throw System.NotImplementedException();
    }

    void Visit(IrClassExp& irExp) override 
    {
        return HandleStaticParent(imRefExp.Symbol);
    }

    void Visit(IrStructExp& irExp) override 
    {
        return HandleStaticParent(imRefExp.Symbol);
    }

    void Visit(IrEnumExp& irExp) override 
    {
        return HandleStaticParent(imRefExp.Symbol);
    }

    void Visit(IrThisVarExp& irExp) override 
    {
        // this.id
        var visitor = new ThisTypeVisitor(name, typeArgs, context, nodeForErrorReport);
        return imRefExp.Type.Accept<ThisTypeVisitor, TranslationResult<IntermediateRefExp>>(ref visitor);
    }

    void Visit(IrStaticRefExp& irExp) override 
    {
        // static ref가 부모이면
        var visitor = new StaticRefTypeBinder(imRefExp, name, typeArgs, context, nodeForErrorReport);
        return imRefExp.LocType.Accept<StaticRefTypeBinder, TranslationResult<IntermediateRefExp>>(ref visitor);
    }

    void Visit(IrBoxRefExp& irExp) override 
    {
        var visitor = new BoxRefTypeBinder(imRefExp, name, typeArgs, context, nodeForErrorReport);
        return imRefExp.GetTargetType().Accept<BoxRefTypeBinder, TranslationResult<IntermediateRefExp>>(ref visitor);
    }

    void Visit(IrLocalRefExp& irExp) override 
    {
        var visitor = new LocalRefTypeVisitor();
        return imRefExp.LocType.Accept<LocalRefTypeVisitor, TranslationResult<IntermediateRefExp>>(ref visitor);
    }

    void Visit(IrDerefedBoxValueExp& irExp) override 
    {
        // *pS, 오직 value type에만 작동을 하도록 보장해야 한다
        var visitor = new BoxValueTypeVisitor(imRefExp, name, typeArgs, context, nodeForErrorReport);
        return imRefExp.InnerType.Accept<BoxValueTypeVisitor, TranslationResult<IntermediateRefExp>>(ref visitor);
    }

    void Visit(IrLocalValueExp& irExp) override 
    {
        // exp.id
        // 함수 호출 인자 제외 temp 참조 불가
        return Fatal(A3002_Reference_CantReferenceTempValue);
    }
};

} // namespace 

IrExpPtr BindIrExpAndMemberNameToIrExp(IrExp& irExp, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, const ScopeContextPtr& context, const LoggerPtr& logger)
{
    IrExpPtr irBoundExp;
    IrExpAndMemberNameToRExpBinder binder(name, typeArgsExceptOuter, &irBoundExp, context, logger);
    irExp.Accept(binder);
    return irBoundExp;
}

} // namespace Citron::SyntaxIR0Translator

