#include "pch.h"
#include "IrExpAndMemberNameToIrExpTranslation.h"

#include <cassert>

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <Logging/Logger.h>
#include <IR0/RNames.h>
#include <IR0/RMember.h>
#include <IR0/RTypeFactory.h>
#include <IR0/NClassDecl.h>
#include <IR0/RClassMemberVarDecl.h>
#include <IR0/NStructDecl.h>
#include <IR0/NStructMemberVarDecl.h>
#include <IR0/NEnumDecl.h>
#include <IR0/NNamespaceDecl.h>

#include "IrExp.h"
#include "TranslationContext.h"
#include "FuncContext.h"
#include "ScopeContext.h"

namespace Citron::SyntaxIR0Translator {

namespace {

class StaticParentTranslator
{
    RTypeArgumentsPtr typeArgsExceptOuter;
    TranslationContext& context;

public:
    StaticParentTranslator(const RTypeArgumentsPtr& typeArgsExceptOuter, TranslationContext& context)
        : typeArgsExceptOuter(typeArgsExceptOuter), context(context)
    {
    }

    IrExpPtr operator()(RMember_Namespace& member) 
    {
        return MakePtr<IrExp_Namespace>(member.decl);
    }

    // S.F
    IrExpPtr operator()(RMember_GlobalFuncs& member) 
    {   
        context.Log(&Logger::Fatal_Reference_CantMakeReference);
        return nullptr;
    }

    IrExpPtr operator()(RMember_Class& member) 
    {
        auto typeArgs = context.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        return MakePtr<IrExp_Class>(member.decl, std::move(typeArgs));
    }

    // 에러,
    IrExpPtr operator()(RMember_ClassFuncs& member) 
    {
        context.Log(&Logger::Fatal_Reference_CantMakeReference);
        return nullptr;
    }

    // C.x
    IrExpPtr operator()(RMember_ClassMemberVar& member) 
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

        assert(member.typeArgs->GetCount() == 0);
        return MakePtr<IrExp_StaticRef>(MakePtr<NLoc_ClassMember>(/*instance*/ nullptr, member.decl, member.typeArgs));
    }

    IrExpPtr operator()(RMember_Struct& member) 
    {
        auto typeArgs = context.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        return MakePtr<IrExp_Struct>(member.decl, std::move(typeArgs));
    }

    IrExpPtr operator()(RMember_StructFuncs& member) 
    {
        context.Log(&Logger::Fatal_Reference_CantMakeReference);
        return nullptr;
    }

    IrExpPtr operator()(RMember_StructMemberVar& member) 
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

        assert(member.typeArgs->GetCount() == 0);
        return MakePtr<IrExp_StaticRef>(MakePtr<NLoc_StructMember>(/*instance*/ nullptr, member.decl, member.typeArgs));
    }

    // E
    IrExpPtr operator()(RMember_Enum& member) 
    {   
        auto typeArgs = context.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        return MakePtr<IrExp_Enum>(member.decl, std::move(typeArgs));
    }

    // &E.First.x
    IrExpPtr operator()(RMember_EnumElem& member) 
    {   
        context.Log(&Logger::Fatal_Reference_CantMakeReference);
        return nullptr;
    }

    // &E.x
    IrExpPtr operator()(RMember_EnumElemMemberVar& member) 
    {
        // 표현 불가능
        throw RuntimeFatalException();
    }

    IrExpPtr operator()(RMember_LambdaMemberVar& member) 
    {
        throw RuntimeFatalException();
    }

    IrExpPtr operator()(RMember_TupleMemberVar& member) 
    {
        throw RuntimeFatalException();
    }

    /*TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result)
    {
        return Fatal(A2014_ResolveIdentifier_MultipleCandidatesForMember);
    }*/
};

class StaticRefTypeTranslator : public RTypeVisitor
{
    std::shared_ptr<IrExp_StaticRef> parent;
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    TranslationContext& context;

public:
    StaticRefTypeTranslator(const std::shared_ptr<IrExp_StaticRef>& parent, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, 
        TranslationContext& context)
        : parent(parent), name(name), typeArgsExceptOuter(typeArgsExceptOuter), result(result)
        , context(context)
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
        context.Log(&Logger::Fatal_ResolveIdentifier_FuncInstanceCantHaveMember);
        *result = nullptr;        
    }

    // &C.pS.id;
    void Visit(RType_LocalPtr& type) override 
    {   
        context.Log(&Logger::Fatal_ResolveIdentifier_LocalPtrCantHaveMember);
        *result = nullptr;
    }

    void Visit(RType_BoxPtr& type) override 
    {
        // &(C.x).a
        context.Log(&Logger::Fatal_Reference_CantMakeReference);
        *result = nullptr;
    }

    void Visit(RType_Class& type) override 
    {
        auto memberVar = type.GetMemberVar(name);

        if (!memberVar)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_NotFound);
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() == 0)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_VarWithTypeArg);
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
            context.Log(&Logger::Fatal_ResolveIdentifier_NotFound);
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_VarWithTypeArg);
            *result = nullptr;
            return;
        }

        *result = MakePtr<IrExp_StaticRef>(MakePtr<NLoc_StructMember>(parent->loc, memberVar->decl, memberVar->typeArgs));
    }

    // Enum자체는 member를 가져올 수 없다
    void Visit(RType_Enum& type) override 
    {
        context.Log(&Logger::Fatal_ResolveIdentifier_NotFound);
        *result = nullptr;
    }

    // e.x (E.Second.x)
    void Visit(RType_EnumElem& type) override 
    {   
        auto memberVar = type.GetMemberVar(name);
        if (!memberVar)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_NotFound);
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_VarWithTypeArg);
            *result = nullptr;
        }

        *result = MakePtr<IrExp_StaticRef>(MakePtr<NLoc_EnumElemMember>(parent->loc, memberVar->decl, memberVar->outerTypeArgs));
    }

    // &C.i.id
    void Visit(RType_Interface& type) override 
    {   
        throw NotImplementedException();
    }

    // &C.l.id
    void Visit(RType_Lambda& type) override 
    {   
        context.Log(&Logger::Fatal_ResolveIdentifier_LambdaInstanceCantHaveMember);
        *result = nullptr;
    }
};

class BoxRefTypeTranslator : public RTypeVisitor
{
    std::shared_ptr<IrExp_BoxRef> parent;
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    TranslationContext& context;

public: 
    BoxRefTypeTranslator(const std::shared_ptr<IrExp_BoxRef>& parent, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, TranslationContext& context)
        : parent(parent), name(name), typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context)
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
        context.Log(&Logger::Fatal_ResolveIdentifier_FuncInstanceCantHaveMember);
        *result = nullptr;
    }

    void Visit(RType_LocalPtr& type) override 
    {
        // &c.p.x
        context.Log(&Logger::Fatal_ResolveIdentifier_LocalPtrCantHaveMember);
        *result = nullptr;
    }

    void Visit(RType_BoxPtr& type) override 
    {
        // &c.p.x, 문법에러        
        context.Log(&Logger::Fatal_ResolveIdentifier_BoxPtrCantHaveMember);
        *result = nullptr;
    }

    void Visit(RType_Class& type) override 
    {
        // &c.c.x
        auto memberVar = type.GetMemberVar(name);
        if (!memberVar)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_NotFound);
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_VarWithTypeArg);
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
            context.Log(&Logger::Fatal_ResolveIdentifier_NotFound);
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_VarWithTypeArg);
            *result = nullptr;
            return;
        }

        *result = MakePtr<IrExp_BoxRef_StructMember>(parent, memberVar->decl, memberVar->typeArgs);
    }

    void Visit(RType_Enum& type) override 
    {
        // &c.e.x
        context.Log(&Logger::Fatal_ResolveIdentifier_EnumInstanceCantHaveMember);
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
        context.Log(&Logger::Fatal_ResolveIdentifier_LambdaInstanceCantHaveMember);
        *result = nullptr;
    }
};

class LocalRefTypeTranslator : public RTypeVisitor
{
    std::shared_ptr<IrExp_LocalRef> parent;
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    TranslationContext& context;

public:
    LocalRefTypeTranslator(const std::shared_ptr<IrExp_LocalRef>& parent, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, TranslationContext& context)
        : parent(parent), name(name), typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context)
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
        context.Log(&Logger::Fatal_ResolveIdentifier_FuncInstanceCantHaveMember);
        *result = nullptr;
    }

    void Visit(RType_LocalPtr& type) override 
    {
        // &s.p.x
        context.Log(&Logger::Fatal_ResolveIdentifier_LocalPtrCantHaveMember);
        *result = nullptr;
    }

    void Visit(RType_BoxPtr& type) override 
    {
        // &s.p.x
        context.Log(&Logger::Fatal_ResolveIdentifier_BoxPtrCantHaveMember);
        *result = nullptr;
    }

    void Visit(RType_Class& type) override 
    {
        // &s.c.x
        auto memberVar = type.GetMemberVar(name);
        if (!memberVar)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_NotFound);
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_VarWithTypeArg);
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
            context.Log(&Logger::Fatal_ResolveIdentifier_NotFound);
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_VarWithTypeArg);
            *result = nullptr;
            return;
        }

        *result = MakePtr<IrExp_LocalRef>(MakePtr<NLoc_StructMember>(parent->loc, memberVar->decl, memberVar->typeArgs));
    }

    void Visit(RType_Enum& type) override 
    {
        // &s.e.x
        context.Log(&Logger::Fatal_ResolveIdentifier_EnumInstanceCantHaveMember);
        *result = nullptr;
    }

    void Visit(RType_EnumElem& type) override 
    {
        // &s.e.x
        auto memberVar = type.GetMemberVar(name);
        if (!memberVar)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_NotFound);
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_VarWithTypeArg);
            *result = nullptr;
        }

        *result = MakePtr<IrExp_LocalRef>(MakePtr<NLoc_EnumElemMember>(parent->loc, memberVar->decl, memberVar->outerTypeArgs));
    }

    void Visit(RType_Interface& type) override 
    {
        // &s.i.x
        throw NotImplementedException();
    }

    void Visit(RType_Lambda& type) override 
    {
        // &s.l.x
        context.Log(&Logger::Fatal_ResolveIdentifier_LambdaInstanceCantHaveMember);
        *result = nullptr;
    }
};

// *pS, valueType일때만 여기를 거치도록 나머지는 value로 가게
class BoxValueTypeTranslator : public RTypeVisitor
{
    std::shared_ptr<IrExp_DerefedBoxValue> parent;
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    TranslationContext& context;

public:
    BoxValueTypeTranslator(const std::shared_ptr<IrExp_DerefedBoxValue>& parent, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, TranslationContext& context)
        : parent(parent), name(name), typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context)
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
        context.Log(&Logger::Fatal_ResolveIdentifier_BoxPtrCantHaveMember);
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
            context.Log(&Logger::Fatal_ResolveIdentifier_NotFound);
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_VarWithTypeArg);
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
        context.Log(&Logger::Fatal_ResolveIdentifier_LambdaInstanceCantHaveMember);
        *result = nullptr;
    }
};

class ThisTypeTranslator : public RTypeVisitor
{   
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    TranslationContext& context;

public:
    ThisTypeTranslator(const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, TranslationContext& context)
        : name(name), typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context)
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
        context.Log(&Logger::Fatal_ResolveIdentifier_LocalPtrCantHaveMember);
        *result = nullptr;
    }

    void Visit(RType_BoxPtr& type) override 
    {
        context.Log(&Logger::Fatal_ResolveIdentifier_BoxPtrCantHaveMember);
        *result = nullptr;
    }

    void Visit(RType_Class& type) override 
    {
        // &this.x
        auto memberVar = type.GetMemberVar(name);
        if (!memberVar)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_NotFound);
            *result = nullptr;
            return;
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_VarWithTypeArg);
            *result = nullptr;
            return;
        }
        
        *result = MakePtr<IrExp_BoxRef_ClassMember>(context.MakeThisLoc(), memberVar->decl, memberVar->typeArgs);
    }

    void Visit(RType_Struct& type) override 
    {
        // &this.x
        // TODO: [10] box함수인 경우 에러 메시지를 다르게 해야 한다
        context.Log(&Logger::Fatal_ResolveIdentifier_LocalPtrCantHaveMember);
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

class IrExpAndMemberNameToIrExpTranslator : public IrExpVisitor
{
    IrExpPtr irThis;
    RName name;
    RTypeArgumentsPtr typeArgsExceptOuter;
    IrExpPtr* result;

    TranslationContext& context;

public:
    IrExpAndMemberNameToIrExpTranslator(const IrExpPtr& irThis, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, IrExpPtr* result, TranslationContext& context)
        : irThis(irThis), name(name), typeArgsExceptOuter(typeArgsExceptOuter), result(result), context(context)
    {
    }

    void HandleStaticParent(RDecl& decl, const RTypeArgumentsPtr& typeArgs)
    {
        auto oMember = decl.GetMember(typeArgs, name, typeArgsExceptOuter->GetCount());
        if (!oMember)
        {
            context.Log(&Logger::Fatal_ResolveIdentifier_NotFound);
            *result = nullptr;
            return;
        }

        StaticParentTranslator binder(typeArgsExceptOuter, context);
        visit(binder, *oMember);
    }

    void Visit(IrExp_Namespace& irExp) override 
    {
        return HandleStaticParent(*irExp.decl, context.MakeTypeArguments({}));
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
        ThisTypeTranslator binder(name, typeArgsExceptOuter, result, context);
        irExp.type->Accept(binder);
    }

    void Visit(IrExp_StaticRef& irExp) override 
    {
        auto irStaticRefThis = dynamic_pointer_cast<IrExp_StaticRef>(irThis);
        assert(irStaticRefThis);

        auto locType = context.GetType(*irExp.loc);

        // static ref가 부모이면
        StaticRefTypeTranslator binder(irStaticRefThis, name, typeArgsExceptOuter, result, context);
        locType->Accept(binder);
    }

    void Visit(IrExp_BoxRef& irExp) override 
    {
        auto irBoxRefThis = dynamic_pointer_cast<IrExp_BoxRef>(irThis);
        assert(irBoxRefThis);

        auto targetType = context.GetTargetType(irExp);
        BoxRefTypeTranslator binder(irBoxRefThis, name, typeArgsExceptOuter, result, context);
        targetType->Accept(binder);
    }

    void Visit(IrExp_LocalRef& irExp) override 
    {
        auto irLocalRefThis = dynamic_pointer_cast<IrExp_LocalRef>(irThis);
        assert(irLocalRefThis);

        auto locType = context.GetType(*irExp.loc);

        LocalRefTypeTranslator binder(irLocalRefThis, name, typeArgsExceptOuter, result, context);
        locType->Accept(binder);
    }

    // *pS, 오직 value type에만 작동을 하도록 보장해야 한다
    void Visit(IrExp_DerefedBoxValue& irExp) override 
    {
        auto irDerefedBoxThis = dynamic_pointer_cast<IrExp_DerefedBoxValue>(irThis);
        assert(irDerefedBoxThis);

        auto innerType = context.GetType(*irExp.innerLoc);

        BoxValueTypeTranslator binder(irDerefedBoxThis, name, typeArgsExceptOuter, result, context);
        innerType->Accept(binder);
    }

    void Visit(IrExp_LocalValue& irExp) override 
    {
        // exp.id
        // 함수 호출 인자 제외 temp 참조 불가
        context.Log(&Logger::Fatal_Reference_CantReferenceTempValue);
        *result = nullptr;
    }
};

} // namespace 

IrExpPtr TranslateIrExpAndMemberNameToIrExp(const IrExpPtr& irExp, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, TranslationContext& context)
{
    IrExpPtr irBoundExp;
    IrExpAndMemberNameToIrExpTranslator binder(irExp, name, typeArgsExceptOuter, &irBoundExp, context);
    irExp->Accept(binder);
    return irBoundExp;
}

} // namespace Citron::SyntaxIR0Translator

