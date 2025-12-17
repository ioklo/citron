#include "IrExpAndMemberNameToIrExpTranslation.h"

#include <cassert>
#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Logging/Logger.h"
#include "RSymbol/RMember.h"
#include "RSymbol/RClassDecl.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/REnumDecl.h"
#include "RSymbol/RTypeArguments.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RNamespaceDecl.h"
#include "MIR/MLoc.h"

#include "IrExp.h"
#include "TranslationContext.h"
#include "FuncContext.h"
#include "ScopeContext.h"

using namespace std;

namespace Citron {

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

    expected<IrExp*, DiagPtr> operator()(RMember_Namespace& member) 
    {
        return context.MakeIrExp<IrExp_Namespace>(member.decl);
    }

    // S.F
    expected<IrExp*, DiagPtr> operator()(RMember_GlobalFuncs& member)
    {   
        return unexpected{MakePtr<Error_Reference_CantMakeReference>()};
    }

    expected<IrExp*, DiagPtr> operator()(RMember_Class& member)
    {
        auto typeArgs = context.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        return context.MakeIrExp<IrExp_Class>(member.decl, typeArgs);
    }

    // 에러,
    expected<IrExp*, DiagPtr> operator()(RMember_ClassFuncs& member)
    {
        return unexpected{MakePtr<Error_Reference_CantMakeReference>()};
    }

    // C.x
    expected<IrExp*, DiagPtr> operator()(RMember_ClassVar& member)
    {
        if (!member.decl->IsStatic())
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>()};
        }

        if (!context.CanAccess(member.decl))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        assert(member.typeArgs->GetCount() == 0);
        return context.MakeIrExp<IrExp_StaticRef>(context.MakeNLoc<MLoc_ClassVar>(/*instance*/ nullptr, member.decl, member.typeArgs));
    }

    expected<IrExp*, DiagPtr> operator()(RMember_Struct& member)
    {
        auto typeArgs = context.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        return context.MakeIrExp<IrExp_Struct>(member.decl, typeArgs);
    }

    expected<IrExp*, DiagPtr> operator()(RMember_StructFuncs& member)
    {
        return unexpected{MakePtr<Error_Reference_CantMakeReference>()};
    }

    expected<IrExp*, DiagPtr> operator()(RMember_StructVar& member)
    {
        if (!member.decl->IsStatic())
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>()};
        }

        if (!context.CanAccess(member.decl))
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_TryAccessingPrivateMember>()};
        }

        assert(member.typeArgs->GetCount() == 0);
        return context.MakeIrExp<IrExp_StaticRef>(context.MakeNLoc<MLoc_StructVar>(/*instance*/ nullptr, member.decl, member.typeArgs));
    }

    // E
    expected<IrExp*, DiagPtr> operator()(RMember_Enum& member)
    {   
        auto typeArgs = context.MergeTypeArguments(*member.outerTypeArgs, *typeArgsExceptOuter);
        return context.MakeIrExp<IrExp_Enum>(member.decl, typeArgs);
    }

    // &E.First.x
    expected<IrExp*, DiagPtr> operator()(RMember_EnumElem& member)
    {   
        return unexpected{MakePtr<Error_Reference_CantMakeReference>()};
    }

    // &E.x
    expected<IrExp*, DiagPtr> operator()(RMember_EnumElemVar& member)
    {
        // 표현 불가능
        throw RuntimeFatalException{};
    }

    expected<IrExp*, DiagPtr> operator()(RMember_LambdaVar& member)
    {
        throw RuntimeFatalException{};
    }

    expected<IrExp*, DiagPtr> operator()(RMember_TupleVar& member)
    {
        throw RuntimeFatalException{};
    }

    expected<IrExp*, DiagPtr> operator()(RMember_TypeVar& member)
    {
        throw NotImplementedException{};
    }

    expected<IrExp*, DiagPtr> operator()(RMember_LocalVar& member)
    {
        throw NotImplementedException{};
    }

    expected<IrExp*, DiagPtr> operator()(RMember_ThisVar& member)
    {
        throw NotImplementedException{};
    }


    /*TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result)
    {
        return Fatal(A2014_ResolveIdentifier_MultipleCandidatesForMember);
    }*/
};

class StaticRefTypeTranslator
{
public:
    using ResultType = expected<IrExp*, DiagPtr>;

private:
    IrExp_StaticRef* parent;
    RName name;
    RTypeArguments* typeArgsExceptOuter;

    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, IrExp>
    ResultType Value(TArgs&&... args)
    {
        return context.MakeIrExp<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    ResultType Error(expected<TValue, DiagPtr>&& e)
    {
        return unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    ResultType Error(TArgs&&... args)
    {
        return unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

public:
    StaticRefTypeTranslator(IrExp_StaticRef* parent, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContext& context)
        : parent{parent}, name{name}, typeArgsExceptOuter{typeArgsExceptOuter}, context{context}
    {
    }

    // &C.optS.id
    ResultType Visit(RType_NullableValue* type) 
    {
        throw NotImplementedException{};
    }

    // &C.optS.id
    ResultType Visit(RType_NullableRef* type) 
    {
        throw NotImplementedException{};
    }

    ResultType Visit(RType_TypeVar* type) 
    {
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Void* type) 
    {
        // void인 멤버가 나올 수 없으므로
        throw RuntimeFatalException{};
    }

    ResultType Visit(RType_Primitive* type)
    {
        // primitive type에 멤버가 나올 수 없으므로 에러 내고 종료
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Tuple* type) 
    {
        // TupleMemberLoc이 없으므로 일단 보류
        throw NotImplementedException{};
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
    ResultType Visit(RType_Func* type)
    {
        return Error<Error_ResolveIdentifier_FuncInstanceCantHaveMember>();
    }

    // &C.pS.id;
    ResultType Visit(RType_LocalPtr* type) 
    {   
        return Error<Error_ResolveIdentifier_LocalPtrCantHaveMember>();
    }

    ResultType Visit(RType_BoxPtr* type) 
    {
        // T& t = (C.x).a
        return Error<Error_Reference_CantMakeReference>();
    }

    ResultType Visit(RType_Class* type) 
    {
        auto var = type->GetVar(name);

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
    ResultType Visit(RType_Struct* type) 
    {   
        auto var = type->GetVar(name);

        if (!var)
        {
            return Error<Error_ResolveIdentifier_NotFound>();
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            return Error<Error_ResolveIdentifier_VarWithTypeArg>();
        }

        return Value<IrExp_StaticRef>(context.MakeNLoc<MLoc_StructVar>(parent->loc, var->decl, var->typeArgs));
    }

    // Enum자체는 member를 가져올 수 없다
    ResultType Visit(RType_Enum* type) 
    {
        return Error<Error_ResolveIdentifier_NotFound>();
    }

    // e.x (E.Second.x)
    ResultType Visit(RType_EnumElem* type) 
    {   
        auto var = type->GetVar(name);
        if (!var)
        {
            return Error<Error_ResolveIdentifier_NotFound>();
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            return Error<Error_ResolveIdentifier_VarWithTypeArg>();
        }

        return Value<IrExp_StaticRef>(context.MakeNLoc<MLoc_EnumElemVar>(parent->loc, var->decl, var->outerTypeArgs));
    }

    // &C.i.id
    ResultType Visit(RType_Interface* type) 
    {   
        throw NotImplementedException{};
    }

    // &C.l.id
    ResultType Visit(RType_Lambda* type) 
    {   
        return Error<Error_ResolveIdentifier_LambdaInstanceCantHaveMember>();
    }
};

class BoxRefTypeTranslator
{
public:
    using ResultType = expected<IrExp*, DiagPtr>;

    IrExp_BoxRef* parent;
    RName name;
    RTypeArguments* typeArgsExceptOuter;

    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, IrExp>
    ResultType Value(TArgs&&... args)
    {
        return context.MakeIrExp<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    ResultType Error(expected<TValue, DiagPtr>&& e)
    {
        return unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    ResultType Error(TArgs&&... args)
    {
        return unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

public: 
    BoxRefTypeTranslator(IrExp_BoxRef* parent, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContext& context)
        : parent{parent}, name{name}, typeArgsExceptOuter{typeArgsExceptOuter}, context{context}
    {
    }

    ResultType Visit(RType_NullableValue* type) 
    {
        // &c.optS.x
        throw NotImplementedException{};
    }

    ResultType Visit(RType_NullableRef* type) 
    {
        // &c.c.x
        throw NotImplementedException{};
    }

    ResultType Visit(RType_TypeVar* type) 
    {
        // &c.t.x
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Void* type) 
    {
        // &c.v
        // void인 멤버가 나올 수 없으므로
        throw RuntimeFatalException{};
    }

    ResultType Visit(RType_Primitive* type)
    {
        // &c.i.x, primitive type에 멤버는 없으므로 에러 내고 종료
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Tuple* type) 
    {
        // &c.t.x
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Func* type) 
    {
        // &c.f.x
        return Error<Error_ResolveIdentifier_FuncInstanceCantHaveMember>();
    }

    ResultType Visit(RType_LocalPtr* type) 
    {
        // &c.p.x
        return Error<Error_ResolveIdentifier_LocalPtrCantHaveMember>();
    }

    ResultType Visit(RType_BoxPtr* type) 
    {
        // &c.p.x, 문법에러        
        return Error<Error_ResolveIdentifier_BoxPtrCantHaveMember>();
    }

    ResultType Visit(RType_Class* type) 
    {
        // &c.c.x
        auto var = type->GetVar(name);
        if (!var)
        {
            return Error<Error_ResolveIdentifier_NotFound>();
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            return Error<Error_ResolveIdentifier_VarWithTypeArg>();
        }

        return Value<IrExp_BoxRef_ClassMember>(parent->MakeLoc(context), var->decl, var->typeArgs);
    }

    ResultType Visit(RType_Struct* type) 
    {
        // &c.s.x
        auto var = type->GetVar(name);
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

    ResultType Visit(RType_Enum* type) 
    {
        // &c.e.x
        return Error<Error_ResolveIdentifier_EnumInstanceCantHaveMember>();
    }

    ResultType Visit(RType_EnumElem* type) 
    {
        // &c.e.x
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Interface* type) 
    {
        // &c.i.x
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Lambda* type) 
    {
        // &c.l.x
        return Error<Error_ResolveIdentifier_LambdaInstanceCantHaveMember>();
    }
};

class LocalRefTypeTranslator
{
public:
    using ResultType = expected<IrExp*, DiagPtr>;

    IrExp_LocalRef* parent;
    RName name;
    RTypeArguments* typeArgsExceptOuter;

    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, IrExp>
    ResultType Value(TArgs&&... args)
    {
        return context.MakeIrExp<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    ResultType Error(expected<TValue, DiagPtr>&& e)
    {
        return unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    ResultType Error(TArgs&&... args)
    {
        return unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

public:
    LocalRefTypeTranslator(IrExp_LocalRef* parent, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContext& context)
        : parent{parent}, name{name}, typeArgsExceptOuter{typeArgsExceptOuter}, context{context}
    {
    }

    ResultType Visit(RType_NullableValue* type) 
    {
        // &s.optS.x
        throw NotImplementedException{};
    }

    ResultType Visit(RType_NullableRef* type) 
    {
        // &s.c.x
        throw NotImplementedException{};
    }

    ResultType Visit(RType_TypeVar* type) 
    {
        // &s.t.x
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Void* type) 
    {
        // void에 멤버가 나올 수 없으므로, 에러 내고 종료
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Primitive* type)
    {
        // primitive type에 멤버는 없으므로, 에러 내고 종료
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Tuple* type) 
    {
        // &s.t.x
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Func* type) 
    {
        // &s.f.x
        return Error<Error_ResolveIdentifier_FuncInstanceCantHaveMember>();
    }

    ResultType Visit(RType_LocalPtr* type) 
    {
        // &s.p.x
        return Error<Error_ResolveIdentifier_LocalPtrCantHaveMember>();
    }

    ResultType Visit(RType_BoxPtr* type) 
    {
        // &s.p.x
        return Error<Error_ResolveIdentifier_BoxPtrCantHaveMember>();
    }

    ResultType Visit(RType_Class* type) 
    {
        // &s.c.x
        auto var = type->GetVar(name);
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

    ResultType Visit(RType_Struct* type) 
    {
        // &s.s.x
        auto var = type->GetVar(name);
        if (!var)
        {
            return Error<Error_ResolveIdentifier_NotFound>();
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            return Error<Error_ResolveIdentifier_VarWithTypeArg>();
        }

        return Value<IrExp_LocalRef>(context.MakeNLoc<MLoc_StructVar>(parent->loc, var->decl, var->typeArgs));
    }

    ResultType Visit(RType_Enum* type) 
    {
        // &s.e.x
        return Error<Error_ResolveIdentifier_EnumInstanceCantHaveMember>();
    }

    ResultType Visit(RType_EnumElem* type) 
    {
        // &s.e.x
        auto var = type->GetVar(name);
        if (!var)
        {
            return Error<Error_ResolveIdentifier_NotFound>();
        }

        if (typeArgsExceptOuter->GetCount() != 0)
        {
            return Error<Error_ResolveIdentifier_VarWithTypeArg>();
        }

        return Value<IrExp_LocalRef>(context.MakeNLoc<MLoc_EnumElemVar>(parent->loc, var->decl, var->outerTypeArgs));
    }

    ResultType Visit(RType_Interface* type) 
    {
        // &s.i.x
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Lambda* type) 
    {
        // &s.l.x
        return Error<Error_ResolveIdentifier_LambdaInstanceCantHaveMember>();
    }
};

// *pS, valueType일때만 여기를 거치도록 나머지는 value로 가게
class BoxValueTypeTranslator
{
public:
    using ResultType = expected<IrExp*, DiagPtr>;

private:
    IrExp_DerefedBoxValue* parent;
    RName name;
    RTypeArguments* typeArgsExceptOuter;

    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, IrExp>
    ResultType Value(TArgs&&... args)
    {
        return context.MakeIrExp<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    ResultType Error(expected<TValue, DiagPtr>&& e)
    {
        return unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    ResultType Error(TArgs&&... args)
    {
        return unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

public:
    BoxValueTypeTranslator(IrExp_DerefedBoxValue* parent, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContext& context)
        : parent{parent}, name{name}, typeArgsExceptOuter{typeArgsExceptOuter}, context{context}
    {
    }

    ResultType Visit(RType_NullableValue* type) 
    {
        // &(*pOptS).x
        throw NotImplementedException{};
    }

    ResultType Visit(RType_NullableRef* type) 
    {
        // &(*c).x ?
        throw NotImplementedException{};
    }

    ResultType Visit(RType_TypeVar* type) 
    {
        // &(*pT).x
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Void* type) 
    {
        throw RuntimeFatalException{};
    }

    ResultType Visit(RType_Primitive* type)
    {
        // &(*pI).x, 에러를 내고 종료
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Tuple* type) 
    {
        // &(*pT).x
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Func* type) 
    {
        // box ref contained
        throw RuntimeFatalException{};
    }

    ResultType Visit(RType_LocalPtr* type) 
    {
        throw RuntimeFatalException{};
    }

    ResultType Visit(RType_BoxPtr* type) 
    {
        return Error<Error_ResolveIdentifier_BoxPtrCantHaveMember>();
    }

    ResultType Visit(RType_Class* type) 
    {
        // &(*pC).x
        throw RuntimeFatalException{};
    }

    ResultType Visit(RType_Struct* type) 
    {
        // &(*pS).x
        auto var = type->GetVar(name);
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

    ResultType Visit(RType_Enum* type) 
    {
        // (*pE).x
        throw RuntimeFatalException{};
    }

    ResultType Visit(RType_EnumElem* type) 
    {
        // box E.Second* pE = ...
        // &(*pE).x
        throw NotImplementedException{};

        //var var = type->Symbol.GetVar(name);
        //if (var == null)
        //    return Fatal();

        //return Valid(new IntermediateRefExp.BoxRef.EnumMember(parent, var));
    }

    ResultType Visit(RType_Interface* type) 
    {
        // box ref contained
        throw RuntimeFatalException{};
    }

    ResultType Visit(RType_Lambda* type) 
    {
        // doesn't have member variable
        return Error<Error_ResolveIdentifier_LambdaInstanceCantHaveMember>();
    }
};

class ThisTypeTranslator
{
public:
    using ResultType = expected<IrExp*, DiagPtr>;

private:
    RName name;
    RTypeArguments* typeArgsExceptOuter;

    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, IrExp>
    ResultType Value(TArgs&&... args)
    {
        return context.MakeIrExp<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    ResultType Error(expected<TValue, DiagPtr>&& e)
    {
        return unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    ResultType Error(TArgs&&... args)
    {
        return unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

public:
    ThisTypeTranslator(const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContext& context)
        : name{name}, typeArgsExceptOuter{typeArgsExceptOuter}, context{context}
    {
    }

    ResultType Visit(RType_NullableValue* type) 
    {
        // NullableValue는 멤버함수를 가질 수 없다
        throw RuntimeFatalException{};
    }

    ResultType Visit(RType_NullableRef* type) 
    {
        // Nullable은 멤버함수를 가질 수 없다?
        return Error<Error_NotImplemented>();
    }

    ResultType Visit(RType_TypeVar* type) 
    {
        // TypeVar는 멤버함수를 가질 수 없다
        throw RuntimeFatalException{};
    }

    ResultType Visit(RType_Void* type) 
    {
        // void는 멤버함수를 가질 수 없다
        throw RuntimeFatalException{};
    }

    ResultType Visit(RType_Primitive* type)
    {
        // &(*pI).x, 에러를 내고 종료
        throw NotImplementedException{};
    }

    ResultType Visit(RType_Tuple* type) 
    {
        // Tuple은 멤버함수를 가질 수 없다
        throw RuntimeFatalException{};
    }

    ResultType Visit(RType_Func* type) 
    {
        // Func가 멤버함수를 갖기 전까진 여기 들어오지 않는다
        throw RuntimeFatalException{};
    }

    ResultType Visit(RType_LocalPtr* type) 
    {
        return Error<Error_ResolveIdentifier_LocalPtrCantHaveMember>();
    }

    ResultType Visit(RType_BoxPtr* type) 
    {
        return Error<Error_ResolveIdentifier_BoxPtrCantHaveMember>();
    }

    ResultType Visit(RType_Class* type) 
    {
        // &this.x
        auto var = type->GetVar(name);
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

    ResultType Visit(RType_Struct* type) 
    {
        // &this.x
        // TODO: [10] box함수인 경우 에러 메시지를 다르게 해야 한다
        return Error<Error_ResolveIdentifier_LocalPtrCantHaveMember>();
    }

    ResultType Visit(RType_Enum* type) 
    {
        // Enum이 멤버 함수를 갖기 전까진 여기 들어오지 않는다
        throw RuntimeFatalException{};
    }

    ResultType Visit(RType_EnumElem* type) 
    {
        // EnumElem이 멤버함수를 갖기 전까진 여기 들어오지 않는다
        throw RuntimeFatalException{};
    }

    ResultType Visit(RType_Interface* type) 
    {
        // Interface가 멤버함수를 갖기 전까진 여기 들어오지 않는다
        throw RuntimeFatalException{};
    }

    ResultType Visit(RType_Lambda* type) 
    {
        // Lambda는 멤버함수를 가질 수 없다
        throw RuntimeFatalException{};
    }
};

class IrExpAndMemberNameToIrExpTranslator
{
public:
    using ResultType = expected<IrExp*, DiagPtr>;

private:
    IrExp* irThis;
    RName name;
    RTypeArguments* typeArgsExceptOuter;

    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, IrExp>
    ResultType Value(TArgs&&... args)
    {
        return context.MakeIrExp<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    ResultType Error(expected<TValue, DiagPtr>&& e)
    {
        return unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    ResultType Error(TArgs&&... args)
    {
        return unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

    ResultType HandleStaticParent(RDecl& decl, RTypeArguments* typeArgs)
    {
        auto oMember = decl.GetMember(typeArgs, name, typeArgsExceptOuter->GetCount());
        if (!oMember)
        {
            return unexpected{MakePtr<Error_ResolveIdentifier_NotFound>()};
        }

        StaticParentTranslator binder(typeArgsExceptOuter, context);
        return visit(binder, *oMember);
    }

public:
    IrExpAndMemberNameToIrExpTranslator(IrExp* irThis, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContext& context)
        : irThis(irThis), name(name), typeArgsExceptOuter(typeArgsExceptOuter), context(context)
    {
    }

    ResultType Visit(IrExp_Namespace* irExp) 
    {
        return HandleStaticParent(*irExp->decl, context.MakeTypeArguments({}));
    }

    ResultType Visit(IrExp_TypeVar* irExp) 
    {
        // 이건 진짜
        throw NotImplementedException{};
    }

    ResultType Visit(IrExp_Class* irExp) 
    {
        return HandleStaticParent(*irExp->decl, irExp->typeArgs);
    }

    ResultType Visit(IrExp_Struct* irExp) 
    {
        return HandleStaticParent(*irExp->decl, irExp->typeArgs);
    }

    ResultType Visit(IrExp_Enum* irExp) 
    {
        return HandleStaticParent(*irExp->decl, irExp->typeArgs);
    }

    ResultType Visit(IrExp_ThisVar* irExp) 
    {
        // this.id        
        ThisTypeTranslator binder{name, typeArgsExceptOuter, context};
        return Accept(binder, irExp->type);
    }

    ResultType Visit(IrExp_StaticRef* irExp) 
    {
        auto* irStaticRefThis = dynamic_cast<IrExp_StaticRef*>(irThis);
        assert(irStaticRefThis);

        auto* locType = context.GetType(irExp->loc);

        // static ref가 부모이면
        StaticRefTypeTranslator binder{irExp, name, typeArgsExceptOuter, context};
        return Accept(binder, locType);
    }

    ResultType Visit(IrExp_BoxRef* irExp) 
    {
        auto irBoxRefThis = dynamic_cast<IrExp_BoxRef*>(irThis);
        assert(irBoxRefThis);

        auto* targetType = context.GetTargetType(irExp);
        BoxRefTypeTranslator binder{irBoxRefThis, name, typeArgsExceptOuter, context};
        return Accept(binder, targetType);
    }

    ResultType Visit(IrExp_LocalRef* irExp) 
    {
        auto* irLocalRefThis = dynamic_cast<IrExp_LocalRef*>(irThis);
        assert(irLocalRefThis);

        auto locType = context.GetType(irExp->loc);

        LocalRefTypeTranslator binder{irLocalRefThis, name, typeArgsExceptOuter, context};
        return Accept(binder, locType);
    }

    // *pS, 오직 value type에만 작동을 하도록 보장해야 한다
    ResultType Visit(IrExp_DerefedBoxValue* irExp) 
    {
        auto* irDerefedBoxThis = dynamic_cast<IrExp_DerefedBoxValue*>(irThis);
        assert(irDerefedBoxThis);

        auto innerType = context.GetType(irExp->innerLoc);

        BoxValueTypeTranslator binder{irDerefedBoxThis, name, typeArgsExceptOuter, context};
        return Accept(binder, innerType);
    }

    ResultType Visit(IrExp_LocalValue* irExp) 
    {
        // exp.id
        // 함수 호출 인자 제외 temp 참조 불가
        return Error<Error_Reference_CantReferenceTempValue>();
    }
};

} // namespace 

expected<IrExp*, DiagPtr> TranslateIrExpAndMemberNameToIrExp(IrExp* irExp, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContext& context)
{
    IrExpAndMemberNameToIrExpTranslator binder{irExp, name, typeArgsExceptOuter, context};
    return Accept(binder, irExp);
}

} // namespace Citron

