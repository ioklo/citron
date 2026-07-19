#include "SExp_MemberToIrExp.h"

#include <cassert>
#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Logging/Logger.h"
#include "RSymbol/RDeclRes.h"
#include "RSymbol/RClassDecl.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/REnumDecl.h"
#include "RSymbol/RTypeArguments.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RNamespaceDecl.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"
#include "MIR/MSharedExp.h"

#include "SExpToIrExp.h"
#include "IrExp.h"
#include "FuncContext.h"
#include "ScopeContext.h"
#include "TranslationContexts.h"
#include "SRTFactory.h"
#include "Misc.h"
#include "IrExpToMLoc.h"
#include "IrExpToMSharedExp.h"

using namespace std;

namespace Citron {

namespace {

struct Result_GetClassVar
{
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;
};

struct Result_GetStructVar
{
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;
};

expected<Result_GetClassVar, DiagPtr> GetClassVar(RType_Class* classType, InRef<RName> name, RTypeArguments* memberTypeArgs, bool bExpectedStatic)
{
    // GetVar로 바로 얻으면, name conflict 처리를 하지 못하기 때문에 GetMember로 얻은 후 캐스팅을 한다
    size_t memberTypeArgsCount = memberTypeArgs->GetCount();
    auto o_declRes = classType->GetMember(*name);
    if (!o_declRes) return Error<Error_ResolveIdentifier_NotFound>();

    auto* classVarMember = o_declRes->GetIf<RDeclRes_ClassVar>();
    if (!classVarMember) return Error<Error_SharedTranslation_CantTranslate>();

    // static 성질이 다르면 에러    
    if (classVarMember->decl->IsStatic() != bExpectedStatic) return Error<Error_SharedTranslation_CantTranslate>();

    // ClassVar이니까. classVarMember->typeArgs와 typeArgsExceptOuter를 합쳐서 쓰지 않고, classVarMember->typeArgs만 사용한다.
    assert(memberTypeArgsCount == 0);
    return Result_GetClassVar{classVarMember->decl, classVarMember->typeArgs};
}

expected<Result_GetStructVar, DiagPtr> GetStructVar(RType_Struct* structType, InRef<RName> name, RTypeArguments* memberTypeArgs, bool bExpectedStatic)
{
    // GetVar로 바로 얻으면, name conflict 처리를 하지 못하기 때문에 GetMember로 얻은 후 캐스팅을 한다
    size_t memberTypeArgsCount = memberTypeArgs->GetCount();
    auto o_declRes = structType->GetMember(*name);
    if (!o_declRes) return Error<Error_ResolveIdentifier_NotFound>();

    auto* structVarMember = o_declRes->GetIf<RDeclRes_StructVar>();
    if (!structVarMember) return Error<Error_SharedTranslation_CantTranslate>();

    // static 이면 에러
    if (structVarMember->decl->IsStatic() == bExpectedStatic) return Error<Error_SharedTranslation_CantTranslate>();
    assert(memberTypeArgsCount == 0);

    return Result_GetStructVar{structVarMember->decl, structVarMember->typeArgs};
}

// NS, S, C로만 이뤄진 Base, GetMember한 결과물이므로 RDeclRes만 있다
class StaticBaseTranslator
{
    RTypeArguments* memberTypeArgs;
    TranslationContexts& contexts;

public:
    StaticBaseTranslator(RTypeArguments* memberTypeArgs, TranslationContexts& contexts)
        : memberTypeArgs(memberTypeArgs), contexts{contexts}
    {
    }

    expected<IrExp*, DiagPtr> operator()(auto& member) { return Visit(member); }

    expected<IrExp*, DiagPtr> Visit(RDeclRes_Namespace& member) 
    {
        return contexts.srtFactory->MakeIrExp<IrExp_Namespace>(member.decl);
    }

    // S.F
    expected<IrExp*, DiagPtr> Visit(RDeclRes_GlobalFuncs& member)
    {   
        // ImExp와 다르게 IrExp는 Callable 자리에 들어가지 않기 때문에, 바로 에러
        return Error<Error_SharedTranslation_CantTranslate>();
    }

    // T.C
    expected<IrExp*, DiagPtr> Visit(RDeclRes_Class& member)
    {
        // TODO: [43] Access Check
        auto* typeArgs = contexts.rFactory->MergeTypeArguments(member.outerTypeArgs, memberTypeArgs);
        return contexts.srtFactory->MakeIrExp<IrExp_Class>(member.decl, typeArgs);
    }

    // IrExp는 Callable자리에 오지 않기때문에 지원하지 않는다
    expected<IrExp*, DiagPtr> Visit(RDeclRes_ClassFuncs& member)
    {
        return Error<Error_SharedTranslation_CantTranslate>();
    }

    // C.x, IrExp_Static으로 만듦
    expected<IrExp*, DiagPtr> Visit(RDeclRes_ClassVar& member)
    {
        // C.x 형식인데, x가 static 변수가 아니라면 에러
        if (!member.decl->IsStatic())
            return Error<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>();

        // TODO: [43] Access Check
        if (!contexts.funcContext->CanAccess(member.decl))
            return Error<Error_ResolveIdentifier_TryAccessingPrivateMember>();

        assert(member.typeArgs->GetCount() == 0);
        auto* loc = contexts.mFactory->MakeMLoc<MLoc_ClassVar>(/*instance*/nullptr, member.decl, member.typeArgs);
        return contexts.srtFactory->MakeIrExp<IrExp_Static>(loc);
    }

    expected<IrExp*, DiagPtr> Visit(RDeclRes_Struct& member)
    {
        // TODO: [43] Access Check
        auto typeArgs = contexts.rFactory->MergeTypeArguments(member.outerTypeArgs, memberTypeArgs);
        return contexts.srtFactory->MakeIrExp<IrExp_Struct>(member.decl, typeArgs);
    }

    expected<IrExp*, DiagPtr> Visit(RDeclRes_StructFuncs& member)
    {        
        return Error<Error_SharedTranslation_CantTranslate>();
    }

    expected<IrExp*, DiagPtr> Visit(RDeclRes_StructVar& member)
    {
        if (!member.decl->IsStatic())
            return Error<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>();

        // TODO: [43] Access Check
        if (!contexts.funcContext->CanAccess(member.decl))
            return Error<Error_ResolveIdentifier_TryAccessingPrivateMember>();

        assert(member.typeArgs->GetCount() == 0);
        auto* loc = contexts.mFactory->MakeMLoc<MLoc_StructVar>(/*instance*/nullptr, member.decl, member.typeArgs);
        return contexts.srtFactory->MakeIrExp<IrExp_Static>(loc);
    }

    // T.E
    expected<IrExp*, DiagPtr> Visit(RDeclRes_Enum& member)
    {
        // IrExp는 StaticBase와 MLoc/MSharedExp를 만드는데 관심이 있는데, 
        // Enum은 StaticBase가 될수 없고, (nested type을 가질수 없고, static var를 가질 수도 없다)
        // Enum은 MLoc/MSharedExp가 될수 없다
        return Error<Error_SharedTranslation_CantTranslate>();
    }

    // T.E.First
    expected<IrExp*, DiagPtr> Visit(RDeclRes_EnumElem& member)
    {   
        return Error<Error_SharedTranslation_CantTranslate>();
    }

    // E.x
    expected<IrExp*, DiagPtr> Visit(RDeclRes_EnumElemVar& member)
    {
        // StaticBase로부터 EnumElemVar가 나올수 있는가, StaticBase에 EnumElem이 나올수 없으므로 불가능
        throw RuntimeFatalException{};
    }

    expected<IrExp*, DiagPtr> Visit(RDeclRes_Lambda& declRes)
    {
        // TODO: [65] 2026-07-06, RLambdaDecl제거, RStructDecl을 쓰도록 변경
        throw NotImplementedException{};
    }

    // NS.x
    expected<IrExp*, DiagPtr> Visit(RDeclRes_LambdaVar& member)
    {
        // 람다 var를 StaticBase로 참조할 방법은 없는거 같다
        throw RuntimeFatalException{};
    }

    expected<IrExp*, DiagPtr> Visit(RDeclRes_Interface& member)
    {
        // TODO: [71] 2026-07-18, interface 구현
        throw NotImplementedException{};
    }

    // NS.t
    expected<IrExp*, DiagPtr> Visit(RDeclRes_TupleVar& member)
    {
        throw RuntimeFatalException{};
    }

    // NS.T
    expected<IrExp*, DiagPtr> Visit(RDeclRes_TypeVar& member)
    {
        // TODO: [52] TypeVar정리
        throw NotImplementedException{};
    }

    // NS.x
    expected<IrExp*, DiagPtr> Visit(RDeclRes_FuncParam& member)
    {
        throw RuntimeFatalException{};
    }

    expected<IrExp*, DiagPtr> Visit(RDeclRes_Trait& member)
    {
        return Error<Error_SharedTranslation_CantUseTraitAsExpression>();
    }

    expected<IrExp*, DiagPtr> Visit(RDeclRes_TraitFuncs& member)
    {
        return Error<Error_SharedTranslation_CantUseTraitFuncAsExpression>();
    }
};

struct Binder
{
    using ResultType = expected<IrExp*, DiagPtr>;

    RName memberName;
    RTypeArguments* memberTypeArgs;
    TranslationContexts& contexts;
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, IrExp>
    ResultType Value(TArgs&&... args)
    {
        return contexts.srtFactory->MakeIrExp<TValue>(forward<TArgs>(args)...);
    }

    ResultType HandleStaticBase(RDecl& decl, RTypeArguments* typeArgs)
    {
        auto o_member = decl.GetMember(memberName);
        if (!o_member)
            return Error<Error_ResolveIdentifier_NotFound>();

        auto declRes = ToRDeclRes(typeArgs, *o_member);

        StaticBaseTranslator binder(memberTypeArgs, contexts);
        return declRes.Visit(binder);
    }

    ResultType Visit(IrExp_Namespace* irBaseExp) 
    {
        return HandleStaticBase(*irBaseExp->decl, contexts.rFactory->MakeEmptyTypeArguments());
    }

    ResultType Visit(IrExp_Class* irBaseExp) 
    {
        return HandleStaticBase(*irBaseExp->decl, irBaseExp->typeArgs);
    }

    ResultType Visit(IrExp_Struct* irBaseExp) 
    {
        return HandleStaticBase(*irBaseExp->decl, irBaseExp->typeArgs);
    }

    // C.x.y
    ResultType Visit(IrExp_Static* irBaseExp)
    {   
        auto* locType = GetType(irBaseExp->loc, &*contexts.rFactory);
        
        // C.x가 class라면, x의 y ClassVar멤버를 가져온다. 없다면 에러.
        // IrExp_ClassVar에서, ClassVar라는거는 base가 Class였고, name이 ClassVar라는 뜻이다
        if (auto* classType = dynamic_cast<RType_Class*>(locType))
        {
            auto e_result = GetClassVar(classType, memberName, memberTypeArgs, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            // IrExp_Static은 분해해서 MLoc으로 참조하고, 보이지 않도록 한다
            return contexts.srtFactory->MakeIrExp<IrExp_ClassVar>(irBaseExp->loc, result.decl, result.typeArgs);
        }
        else if (auto* structType = dynamic_cast<RType_Struct*>(locType))
        {
            auto e_result = GetStructVar(structType, memberName, memberTypeArgs, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            // StructVar는 base로 IrExp를 참조하므로 IrExp_Static이 그대로 들어가게 한다
            return contexts.srtFactory->MakeIrExp<IrExp_StructVar>(irBaseExp, result.decl, result.typeArgs);
        }
        else return Error<Error_SharedTranslation_CantTranslate>();
    }

    // c.x.y
    ResultType Visit(IrExp_ClassVar* irBaseExp)
    {
        // c.x가 base일 때
        // base가 클래스라면, 분해한다 IrExp_ClassVar(MLoc(c), C::x), C::y => IrExp_ClassVar(MLoc_ClassVar(MLoc(c), C::x), C::y)
        // base가 구조체라면, 감싼다   IrExp_ClassVar(MLoc(c), C::x), S::y => IrExp_StructVar(IrExp_ClassVar(MLoc(c), C::x), C::y)

        auto* baseDeclType = irBaseExp->decl->GetUnboundDeclType()->Apply(irBaseExp->typeArgs);

        if (auto* classBaseDeclType = dynamic_cast<RType_Class*>(baseDeclType))
        {
            auto e_result = GetClassVar(classBaseDeclType, memberName, memberTypeArgs, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            auto* baseLoc = TranslateIrExp_ClassVarToMLoc(irBaseExp, contexts);
            return contexts.srtFactory->MakeIrExp<IrExp_ClassVar>(baseLoc, result.decl, result.typeArgs);
        }
        else if (auto* structType = dynamic_cast<RType_Struct*>(baseDeclType))
        {
            auto e_result = GetStructVar(structType, memberName, memberTypeArgs, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            return contexts.srtFactory->MakeIrExp<IrExp_StructVar>(irBaseExp, result.decl, result.typeArgs);
        }
        else return Error<Error_SharedTranslation_CantTranslate>();
    }

    // (*pS).x.y
    ResultType Visit(IrExp_SharedStructVar* irBaseExp)
    {
        // ClassVar랑 비슷하게 처리한다
        // (*pS).x가 base일 때
        // base가 클래스라면, 분해한다 IrExp_SharedStructVar(MLoc(pS), S::x), C::y => IrExp_ClassVar(MLoc_StructVar(MLoc_SharedDeref(MLoc(pS)), S::x), C::y)
        // base가 구조체라면, 감싼다   IrExp_SharedStructVar(MLoc(pS), S::x), S::y => IrExp_StructVar(IrExp_SharedStructVar(MLoc(pS), S::x), C::y)
        auto* baseType = irBaseExp->decl->GetUnboundDeclType()->Apply(irBaseExp->typeArgs);
        
        if (auto* classBaseType = dynamic_cast<RType_Class*>(baseType))
        {
            auto e_result = GetClassVar(classBaseType, memberName, memberTypeArgs, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            auto* baseLoc = TranslateIrExp_SharedStructVarToMLoc(irBaseExp, contexts);
            return contexts.srtFactory->MakeIrExp<IrExp_ClassVar>(baseLoc, result.decl, result.typeArgs);
        }
        else if (auto* structBaseType = dynamic_cast<RType_Struct*>(baseType))
        {
            auto e_result = GetStructVar(structBaseType, memberName, memberTypeArgs, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);
            
            return contexts.srtFactory->MakeIrExp<IrExp_StructVar>(irBaseExp, result.decl, result.typeArgs);
        }
        else return Error<Error_SharedTranslation_CantTranslate>();
    }

    // c.s.x.y
    // C.s.x.y
    ResultType Visit(IrExp_StructVar* irBaseExp)
    {
        // base(c.s.x)가 struct var일 경우
        // c.s.x의 타입이 class인 경우, 기존것을 loc으로 다 치환하고, 새 IrExp_ClassVar를 만든다
        // IrExp_StructVar(IrExp_ClassVar(MLoc(c), C::s), S::x), C::y => IrExp_ClassVar(MLoc_StructVar(MLoc_ClassVar(MLoc(c), C::s), S::x), C::y)
        // 
        // c.s.x의 타입이 struct인 경우, 그대로 감싼다
        // IrExp_StructVar(IrExp_ClassVar(MLoc(c), C::s), S::x), S::y => IrExp_StructVar(IrExp_StructVar(IrExp_ClassVar(MLoc(c), C::s), S::x), S::y)
        auto* baseType = irBaseExp->decl->GetUnboundDeclType()->Apply(irBaseExp->typeArgs);

        if (auto* classBaseType = dynamic_cast<RType_Class*>(baseType))
        {
            auto e_result = GetClassVar(classBaseType, memberName, memberTypeArgs, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            auto e_baseLoc = TranslateIrExp_StructVarToMLoc(irBaseExp, contexts);
            RETURN_ON_ERROR(e_baseLoc);

            return contexts.srtFactory->MakeIrExp<IrExp_ClassVar>(*e_baseLoc, result.decl, result.typeArgs);
        }
        else if (auto* structDeclType = dynamic_cast<RType_Struct*>(baseType))
        {
            auto e_result = GetStructVar(structDeclType, memberName, memberTypeArgs, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            return contexts.srtFactory->MakeIrExp<IrExp_StructVar>(irBaseExp, result.decl, result.typeArgs);
        }
        else return Error<Error_SharedTranslation_CantTranslate>();
    }
    
    // pS->x
    ResultType Visit(IrExp_SharedDeref* irBaseExp)
    {
        // IrExp_SharedDeref의 요구사항이다. IrExp_SharedDeref를 생성할때 체크한다
        // innerLoc(pS)가 shared<S>일 것
        auto* sharedType = dynamic_cast<RType_Shared*>(GetType(irBaseExp->srcShared.loc, &*contexts.rFactory));
        if (!sharedType) throw RuntimeFatalException{}; 
        
        auto* structTargetType = dynamic_cast<RType_Struct*>(sharedType->innerType);
        if (!structTargetType) throw RuntimeFatalException{};
        
        auto e_result = GetStructVar(structTargetType, memberName, memberTypeArgs, /*bExpectedStatic*/false);
        RETURN_ON_ERROR_REFDECL(e_result, result);

        return contexts.srtFactory->MakeIrExp<IrExp_SharedStructVar>(irBaseExp->srcShared.loc, result.decl, result.typeArgs);
    }

    // 임의의 location으로부터
    // loc.x
    // this.x
    // c.x
    ResultType Visit(IrExp_Loc* irExp)
    {
        // loc이 class인 경우, ClassVar를 만든다. IrExp_Loc(MLoc(c)) C::x => IrExp_ClassVar(MLoc(c), C::x)
        // loc이 class이외의 것들인 경우, 에러 
        // loc이 MLoc_SharedDeref()인 경우는 IrExp_SharedDeref인 경우에서 걸러낼 것이기 때문에 여기로 들어오지 않게된다
        auto* baseType = GetType(irExp->loc, &*contexts.rFactory);

        if (auto* classBaseType = dynamic_cast<RType_Class*>(baseType))
        {
            auto e_result = GetClassVar(classBaseType, memberName, memberTypeArgs, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            return contexts.srtFactory->MakeIrExp<IrExp_ClassVar>(irExp->loc, result.decl, result.typeArgs);
        }
        else return Error<Error_SharedTranslation_CantTranslate>();
    }
};

} // namespace 

expected<IrExp*, DiagPtr> TranslateSExp_MemberToIrExp(SExp_Member* sExp, TranslationContexts& contexts)
{
    auto e_base = TranslateSExpToIrExp(sExp->base, contexts);
    RETURN_ON_ERROR(e_base);

    auto e_memberTypeArgs = MakeRTypeArgs(sExp->memberTypeArgs, contexts);
    RETURN_ON_ERROR(e_memberTypeArgs);

    Binder binder{RName_Normal{sExp->memberName}, *e_memberTypeArgs, contexts};
    return Accept(binder, *e_base);
}

} // namespace Citron

