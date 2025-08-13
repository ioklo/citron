#include "IrExpAndMemberNameToIrExpTranslation.h"

#include <cassert>
#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Logging/Logger.h"
#include "IR0/RMember.h"
#include "IR0/RClassDecl.h"
#include "IR0/RClassVarDecl.h"
#include "IR0/RStructDecl.h"
#include "IR0/RStructVarDecl.h"
#include "IR0/REnumDecl.h"
#include "IR0/RTypeArguments.h"
#include "IR0/RTypes.h"
#include "IR0/RNamespaceDecl.h"
#include "IR0/NLoc.h"

#include "IrExp.h"
#include "TranslationContext.h"
#include "FuncContext.h"
#include "ScopeContext.h"

using namespace std;

namespace Citron {

namespace SyntaxIR0Translator {

namespace {

class StaticParentTranslator
{
    RTypeArguments* typeArgsExceptOuter;
    TranslationContext& context;

public:
    StaticParentTranslator(RTypeArguments* typeArgsExceptOuter, TranslationContext& context)
        : typeArgsExceptOuter(typeArgsExceptOuter), context(context)
    {
    }

    expected<IrExpPtr, DiagPtr> operator()(RMember_Namespace& member) 
    {
        return MakePtr<IrExp_Namespace>(member.decl);
    }

    // S.F
    expected<IrExpPtr, DiagPtr> operator()(RMember_GlobalFuncs& member)
    {   
        return unexpected{MakePtr<Error_Reference_CantMakeReference>()};
    }

    expected<IrExpPtr, DiagPtr> operator()(RMember_Class& member)
    {
        auto typeArgs = context.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        return MakePtr<IrExp_Class>(member.decl, move(typeArgs));
    }

    // 에러,
    expected<IrExpPtr, DiagPtr> operator()(RMember_ClassFuncs& member)
    {
        return unexpected{MakePtr<Error_Reference_CantMakeReference>()};
    }

    // C.x
    expected<IrExpPtr, DiagPtr> operator()(RMember_ClassVar& member)
    {
        if (!member.decl->IsStatic())
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>()};
        }

        if (!context.CanAccess(member.decl.get()))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        assert(member.typeArgs->GetCount() == 0);
        return MakePtr<IrExp_StaticRef>(MakePtr<NLoc_ClassVar>(/*instance*/ nullptr, member.decl, member.typeArgs));
    }

    expected<IrExpPtr, DiagPtr> operator()(RMember_Struct& member)
    {
        auto typeArgs = context.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        return MakePtr<IrExp_Struct>(member.decl, move(typeArgs));
    }

    expected<IrExpPtr, DiagPtr> operator()(RMember_StructFuncs& member)
    {
        return unexpected{MakePtr<Error_Reference_CantMakeReference>()};
    }

    expected<IrExpPtr, DiagPtr> operator()(RMember_StructVar& member)
    {
        if (!member.decl->IsStatic())
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>()};
        }

        if (!context.CanAccess(member.decl.get()))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        assert(member.typeArgs->GetCount() == 0);
        return MakePtr<IrExp_StaticRef>(MakePtr<NLoc_StructVar>(/*instance*/ nullptr, member.decl, member.typeArgs));
    }

    // E
    expected<IrExpPtr, DiagPtr> operator()(RMember_Enum& member)
    {   
        auto typeArgs = context.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        return MakePtr<IrExp_Enum>(member.decl, move(typeArgs));
    }

    // &E.First.x
    expected<IrExpPtr, DiagPtr> operator()(RMember_EnumElem& member)
    {   
        return unexpected{MakePtr<Error_Reference_CantMakeReference>()};
    }

    // &E.x
    expected<IrExpPtr, DiagPtr> operator()(RMember_EnumElemVar& member)
    {
        // 표현 불가능
        throw RuntimeFatalException();
    }

    expected<IrExpPtr, DiagPtr> operator()(RMember_LambdaVar& member)
    {
        throw RuntimeFatalException();
    }

    expected<IrExpPtr, DiagPtr> operator()(RMember_TupleVar& member)
    {
        throw RuntimeFatalException();
    }

    expected<IrExpPtr, DiagPtr> operator()(RMember_TypeVar& member)
    {
        throw NotImplementedException();
    }

    expected<IrExpPtr, DiagPtr> operator()(RMember_LocalVar& member)
    {
        throw NotImplementedException();
    }

    expected<IrExpPtr, DiagPtr> operator()(RMember_ThisVar& member)
    {
        throw NotImplementedException();
    }


    /*TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result)
    {
        return Fatal(A2014_ResolveIdentifier_MultipleCandidatesForMember);
    }*/
};

class StaticRefTypeTranslator : public RTypeVisitor
{
    expected<IrExpPtr, DiagPtr>* result;

    shared_ptr<IrExp_StaticRef> parent;
    RName name;
    RTypeArguments* typeArgsExceptOuter;

    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::is_base_of_v<IrExp, TValue>
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

public:
    StaticRefTypeTranslator(expected<IrExpPtr, DiagPtr>* result, const std::shared_ptr<IrExp_StaticRef>& parent, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContext& context)
        : result(result), parent(parent), name(name), typeArgsExceptOuter(typeArgsExceptOuter), context(context)
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
        //    var var = type.GetVar(i);
        //    if (var.GetName().Equals(name))
        //    {
        //        return Valid(new IntermediateRefExp.StaticRef(new TupleMemberLoc parent.Loc)
        //    }
        //}

        //return Fatal();
    }

    // &C.f.id
    void Visit(RType_Func& type) override
    {
        return Error<Error_ResolveIdentifier_FuncInstanceCantHaveMember>();
    }

    // &C.pS.id;
    void Visit(RType_LocalPtr& type) override 
    {   
        return Error<Error_ResolveIdentifier_LocalPtrCantHaveMember>();
    }

    void Visit(RType_BoxPtr& type) override 
    {
        // &(C.x).a
        return Error<Error_Reference_CantMakeReference>();
    }

    void Visit(RType_Class& type) override 
    {
        auto var = type.GetVar(name);

        if (!var)
        {
            return Error<Error_ResolveIdentifier_NotFound>();
        }

        if (typeArgsExceptOuter->GetCount() == 0)
        {
            return Error<Error_ResolveIdentifier_VarWithTypeArg>();
        }

        // 이제 BoxRef로 변경
        return Value<IrExp_BoxRef_ClassMember>(parent->loc, var->decl, var->typeArgs);
    }

    // &C.s.id
    void Visit(RType_Struct& type) override 
    {   
        auto var = type.GetVar(name);

        if (!var)
        {
            return Error<Error_ResolveIdentifier_NotFound>();
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            return Error<Error_ResolveIdentifier_VarWithTypeArg>();
        }

        return Value<IrExp_StaticRef>(MakePtr<NLoc_StructVar>(parent->loc, var->decl, var->typeArgs));
    }

    // Enum자체는 member를 가져올 수 없다
    void Visit(RType_Enum& type) override 
    {
        return Error<Error_ResolveIdentifier_NotFound>();
    }

    // e.x (E.Second.x)
    void Visit(RType_EnumElem& type) override 
    {   
        auto var = type.GetVar(name);
        if (!var)
        {
            return Error<Error_ResolveIdentifier_NotFound>();
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            return Error<Error_ResolveIdentifier_VarWithTypeArg>();
        }

        return Value<IrExp_StaticRef>(MakePtr<NLoc_EnumElemVar>(parent->loc, var->decl, var->outerTypeArgs));
    }

    // &C.i.id
    void Visit(RType_Interface& type) override 
    {   
        throw NotImplementedException();
    }

    // &C.l.id
    void Visit(RType_Lambda& type) override 
    {   
        return Error<Error_ResolveIdentifier_LambdaInstanceCantHaveMember>();
    }
};

class BoxRefTypeTranslator : public RTypeVisitor
{
    expected<IrExpPtr, DiagPtr>* result;

    std::shared_ptr<IrExp_BoxRef> parent;
    RName name;
    RTypeArguments* typeArgsExceptOuter;

    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::is_base_of_v<IrExp, TValue>
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

public: 
    BoxRefTypeTranslator(expected<IrExpPtr, DiagPtr>* result, const std::shared_ptr<IrExp_BoxRef>& parent, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContext& context)
        : result(result), parent(parent), name(name), typeArgsExceptOuter(typeArgsExceptOuter), context(context)
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
        return Error<Error_ResolveIdentifier_FuncInstanceCantHaveMember>();
    }

    void Visit(RType_LocalPtr& type) override 
    {
        // &c.p.x
        return Error<Error_ResolveIdentifier_LocalPtrCantHaveMember>();
    }

    void Visit(RType_BoxPtr& type) override 
    {
        // &c.p.x, 문법에러        
        return Error<Error_ResolveIdentifier_BoxPtrCantHaveMember>();
    }

    void Visit(RType_Class& type) override 
    {
        // &c.c.x
        auto var = type.GetVar(name);
        if (!var)
        {
            return Error<Error_ResolveIdentifier_NotFound>();
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            return Error<Error_ResolveIdentifier_VarWithTypeArg>();
        }

        return Value<IrExp_BoxRef_ClassMember>(parent->MakeLoc(), var->decl, var->typeArgs);
    }

    void Visit(RType_Struct& type) override 
    {
        // &c.s.x
        auto var = type.GetVar(name);
        if (!var)
        {
            return Error<Error_ResolveIdentifier_NotFound>();
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            return Error<Error_ResolveIdentifier_VarWithTypeArg>();
        }

        return Value<IrExp_BoxRef_StructMember>(parent, var->decl, var->typeArgs);
    }

    void Visit(RType_Enum& type) override 
    {
        // &c.e.x
        return Error<Error_ResolveIdentifier_EnumInstanceCantHaveMember>();
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
        return Error<Error_ResolveIdentifier_LambdaInstanceCantHaveMember>();
    }
};

class LocalRefTypeTranslator : public RTypeVisitor
{
    expected<IrExpPtr, DiagPtr>* result;

    shared_ptr<IrExp_LocalRef> parent;
    RName name;
    RTypeArguments* typeArgsExceptOuter;

    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::is_base_of_v<IrExp, TValue>
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

public:
    LocalRefTypeTranslator(expected<IrExpPtr, DiagPtr>* result, const shared_ptr<IrExp_LocalRef>& parent, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContext& context)
        : result(result), parent(parent), name(name), typeArgsExceptOuter(typeArgsExceptOuter), context(context)
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
        return Error<Error_ResolveIdentifier_FuncInstanceCantHaveMember>();
    }

    void Visit(RType_LocalPtr& type) override 
    {
        // &s.p.x
        return Error<Error_ResolveIdentifier_LocalPtrCantHaveMember>();
    }

    void Visit(RType_BoxPtr& type) override 
    {
        // &s.p.x
        return Error<Error_ResolveIdentifier_BoxPtrCantHaveMember>();
    }

    void Visit(RType_Class& type) override 
    {
        // &s.c.x
        auto var = type.GetVar(name);
        if (!var)
        {
            return Error<Error_ResolveIdentifier_NotFound>();
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            return Error<Error_ResolveIdentifier_VarWithTypeArg>();
        }

        return Value<IrExp_BoxRef_ClassMember>(parent->loc, var->decl, var->typeArgs);
    }

    void Visit(RType_Struct& type) override 
    {
        // &s.s.x
        auto var = type.GetVar(name);
        if (!var)
        {
            return Error<Error_ResolveIdentifier_NotFound>();
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            return Error<Error_ResolveIdentifier_VarWithTypeArg>();
        }

        return Value<IrExp_LocalRef>(MakePtr<NLoc_StructVar>(parent->loc, var->decl, var->typeArgs));
    }

    void Visit(RType_Enum& type) override 
    {
        // &s.e.x
        return Error<Error_ResolveIdentifier_EnumInstanceCantHaveMember>();
    }

    void Visit(RType_EnumElem& type) override 
    {
        // &s.e.x
        auto var = type.GetVar(name);
        if (!var)
        {
            return Error<Error_ResolveIdentifier_NotFound>();
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            return Error<Error_ResolveIdentifier_VarWithTypeArg>();
        }

        return Value<IrExp_LocalRef>(MakePtr<NLoc_EnumElemVar>(parent->loc, var->decl, var->outerTypeArgs));
    }

    void Visit(RType_Interface& type) override 
    {
        // &s.i.x
        throw NotImplementedException();
    }

    void Visit(RType_Lambda& type) override 
    {
        // &s.l.x
        return Error<Error_ResolveIdentifier_LambdaInstanceCantHaveMember>();
    }
};

// *pS, valueType일때만 여기를 거치도록 나머지는 value로 가게
class BoxValueTypeTranslator : public RTypeVisitor
{
    expected<IrExpPtr, DiagPtr>* result;
    shared_ptr<IrExp_DerefedBoxValue> parent;
    RName name;
    RTypeArguments* typeArgsExceptOuter;

    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::is_base_of_v<IrExp, TValue>
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

public:
    BoxValueTypeTranslator(expected<IrExpPtr, DiagPtr>* result, const shared_ptr<IrExp_DerefedBoxValue>& parent, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContext& context)
        : result(result), parent(parent), name(name), typeArgsExceptOuter(typeArgsExceptOuter), context(context)
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
        return Error<Error_ResolveIdentifier_BoxPtrCantHaveMember>();
    }

    void Visit(RType_Class& type) override 
    {
        // &(*pC).x
        throw RuntimeFatalException();
    }

    void Visit(RType_Struct& type) override 
    {
        // &(*pS).x
        auto var = type.GetVar(name);
        if (!var)
        {
            return Error<Error_ResolveIdentifier_NotFound>();
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            return Error<Error_ResolveIdentifier_VarWithTypeArg>();
        }

        return Value<IrExp_BoxRef_StructIndirectMember>(parent->innerLoc, var->decl, var->typeArgs);
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

        //var var = type.Symbol.GetVar(name);
        //if (var == null)
        //    return Fatal();

        //return Valid(new IntermediateRefExp.BoxRef.EnumMember(parent, var));
    }

    void Visit(RType_Interface& type) override 
    {
        // box ref contained
        throw RuntimeFatalException();
    }

    void Visit(RType_Lambda& type) override 
    {
        // doesn't have member variable
        return Error<Error_ResolveIdentifier_LambdaInstanceCantHaveMember>();
    }
};

class ThisTypeTranslator : public RTypeVisitor
{   
    expected<IrExpPtr, DiagPtr>* result;
    RName name;
    RTypeArguments* typeArgsExceptOuter;

    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::is_base_of_v<IrExp, TValue>
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

public:
    ThisTypeTranslator(expected<IrExpPtr, DiagPtr>* result, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContext& context)
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
        return Error<Error_NotImplemented>();
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
        return Error<Error_ResolveIdentifier_LocalPtrCantHaveMember>();
    }

    void Visit(RType_BoxPtr& type) override 
    {
        return Error<Error_ResolveIdentifier_BoxPtrCantHaveMember>();
    }

    void Visit(RType_Class& type) override 
    {
        // &this.x
        auto var = type.GetVar(name);
        if (!var)
        {
            return Error<Error_ResolveIdentifier_NotFound>();
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            return Error<Error_ResolveIdentifier_VarWithTypeArg>();
        }
        
        return Value<IrExp_BoxRef_ClassMember>(context.MakeThisLoc(), var->decl, var->typeArgs);
    }

    void Visit(RType_Struct& type) override 
    {
        // &this.x
        // TODO: [10] box함수인 경우 에러 메시지를 다르게 해야 한다
        return Error<Error_ResolveIdentifier_LocalPtrCantHaveMember>();
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
    expected<IrExpPtr, DiagPtr>* result;

    IrExpPtr irThis;
    RName name;
    RTypeArguments* typeArgsExceptOuter;

    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::is_base_of_v<IrExp, TValue>
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

    void HandleStaticParent(RDecl& decl, RTypeArguments* typeArgs)
    {
        auto oMember = decl.GetMember(typeArgs, name, typeArgsExceptOuter->GetCount());
        if (!oMember)
        {
            *result = unexpected{MakePtr<Error_ResolveIdentifier_NotFound>()};
            return;
        }

        StaticParentTranslator binder(typeArgsExceptOuter, context);
        *result = visit(binder, *oMember);
    }

public:
    IrExpAndMemberNameToIrExpTranslator(expected<IrExpPtr, DiagPtr>* result, const IrExpPtr& irThis, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContext& context)
        : result(result), irThis(irThis), name(name), typeArgsExceptOuter(typeArgsExceptOuter), context(context)
    {
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
        ThisTypeTranslator binder(result, name, typeArgsExceptOuter, context);
        return irExp.type->Accept(binder);
    }

    void Visit(IrExp_StaticRef& irExp) override 
    {
        auto irStaticRefThis = dynamic_pointer_cast<IrExp_StaticRef>(irThis);
        assert(irStaticRefThis);

        auto locType = context.GetType(*irExp.loc);

        // static ref가 부모이면
        StaticRefTypeTranslator binder(result, &irExp, name, typeArgsExceptOuter, context);
        return locType->Accept(binder);
    }

    void Visit(IrExp_BoxRef& irExp) override 
    {
        auto irBoxRefThis = dynamic_pointer_cast<IrExp_BoxRef>(irThis);
        assert(irBoxRefThis);

        auto targetType = context.GetTargetType(irExp);
        BoxRefTypeTranslator binder(result, irBoxRefThis, name, typeArgsExceptOuter, context);
        return targetType->Accept(binder);
    }

    void Visit(IrExp_LocalRef& irExp) override 
    {
        auto irLocalRefThis = dynamic_pointer_cast<IrExp_LocalRef>(irThis);
        assert(irLocalRefThis);

        auto locType = context.GetType(*irExp.loc);

        LocalRefTypeTranslator binder(result, irLocalRefThis, name, typeArgsExceptOuter, context);
        return locType->Accept(binder);
    }

    // *pS, 오직 value type에만 작동을 하도록 보장해야 한다
    void Visit(IrExp_DerefedBoxValue& irExp) override 
    {
        auto irDerefedBoxThis = dynamic_pointer_cast<IrExp_DerefedBoxValue>(irThis);
        assert(irDerefedBoxThis);

        auto innerType = context.GetType(*irExp.innerLoc);

        BoxValueTypeTranslator binder(result, irDerefedBoxThis, name, typeArgsExceptOuter, context);
        return innerType->Accept(binder);
    }

    void Visit(IrExp_LocalValue& irExp) override 
    {
        // exp.id
        // 함수 호출 인자 제외 temp 참조 불가
        return Error<Error_Reference_CantReferenceTempValue>();
    }
};

} // namespace 

expected<IrExpPtr, DiagPtr> TranslateIrExpAndMemberNameToIrExp(const IrExpPtr& irExp, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContext& context)
{
    expected<IrExpPtr, DiagPtr> irBoundExp;
    IrExpAndMemberNameToIrExpTranslator binder(&irBoundExp, irExp, name, typeArgsExceptOuter, context);
    irExp->Accept(binder);
    return irBoundExp;
}

} // namespace SyntaxIR0Translator
} // namespace Citron

