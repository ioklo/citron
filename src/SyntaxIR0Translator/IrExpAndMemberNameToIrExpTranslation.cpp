#include "IrExpAndMemberNameToIrExpTranslation.h"

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

#include "IrExp.h"
#include "FuncContext.h"
#include "ScopeContext.h"
#include "TranslationContexts.h"
#include "SRTFactory.h"
#include "IrExpAndMemberNameTranslation.h"
#include "Misc.h"
#include "IrExpToMLocTranslation.h"
#include "IrExpToMSharedExpTranslation.h"

using namespace std;

namespace Citron {

namespace {

class StaticParentTranslator
{
    RTypeArguments* typeArgsExceptOuter;
    TranslationContexts& contexts;

public:
    StaticParentTranslator(RTypeArguments* typeArgsExceptOuter, TranslationContexts& contexts)
        : typeArgsExceptOuter(typeArgsExceptOuter), contexts{contexts}
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
        return Error<Error_Reference_CantMakeReference>();
    }

    expected<IrExp*, DiagPtr> Visit(RDeclRes_Class& member)
    {
        auto typeArgs = contexts.rFactory->MergeTypeArguments(member.outerTypeArgs, typeArgsExceptOuter);
        return contexts.srtFactory->MakeIrExp<IrExp_Class>(member.decl, typeArgs);
    }

    // 에러,
    expected<IrExp*, DiagPtr> Visit(RDeclRes_ClassFuncs& member)
    {
        return Error<Error_Reference_CantMakeReference>();
    }

    // C.x
    expected<IrExp*, DiagPtr> Visit(RDeclRes_ClassVar& member)
    {
        if (!member.decl->IsStatic())
        {
            return Error<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>();
        }

        if (!contexts.funcContext->CanAccess(member.decl))
        {
            return Error<Error_ResolveIdentifier_TryAccessingPrivateMember>();
        }

        assert(member.typeArgs->GetCount() == 0);
        auto* loc = contexts.mFactory->MakeMLoc<MLoc_ClassVar>(/*instance*/nullptr, member.decl, member.typeArgs);
        return contexts.srtFactory->MakeIrExp<IrExp_Static>(loc, contexts.rFactory);
    }

    expected<IrExp*, DiagPtr> Visit(RDeclRes_Struct& member)
    {
        auto typeArgs = contexts.rFactory->MergeTypeArguments(member.outerTypeArgs, typeArgsExceptOuter);
        return contexts.srtFactory->MakeIrExp<IrExp_Struct>(member.decl, typeArgs);
    }

    expected<IrExp*, DiagPtr> Visit(RDeclRes_StructFuncs& member)
    {
        return Error<Error_Reference_CantMakeReference>();
    }

    expected<IrExp*, DiagPtr> Visit(RDeclRes_StructVar& member)
    {
        if (!member.decl->IsStatic())
        {
            return Error<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>();
        }

        if (!contexts.funcContext->CanAccess(member.decl))
        {
            return Error<Error_ResolveIdentifier_TryAccessingPrivateMember>();
        }

        assert(member.typeArgs->GetCount() == 0);

        auto* loc = contexts.mFactory->MakeMLoc<MLoc_StructVar>(/*instance*/nullptr, member.decl, member.typeArgs);
        return contexts.srtFactory->MakeIrExp<IrExp_Static>(loc, contexts.rFactory);
    }

    // E
    expected<IrExp*, DiagPtr> Visit(RDeclRes_Enum& member)
    {
        return Error<Error_SharedTranslation_MemberBaseShouldBeShared>();
    }

    // &E.First.x
    expected<IrExp*, DiagPtr> Visit(RDeclRes_EnumElem& member)
    {   
        return Error<Error_Reference_CantMakeReference>();
    }

    // &E.x
    expected<IrExp*, DiagPtr> Visit(RDeclRes_EnumElemVar& member)
    {
        // 표현 불가능
        throw RuntimeFatalException{};
    }

    expected<IrExp*, DiagPtr> Visit(RDeclRes_LambdaVar& member)
    {
        throw RuntimeFatalException{};
    }

    expected<IrExp*, DiagPtr> Visit(RDeclRes_TupleVar& member)
    {
        throw RuntimeFatalException{};
    }

    expected<IrExp*, DiagPtr> Visit(RDeclRes_TypeVar& member)
    {
        throw NotImplementedException{};
    }

    expected<IrExp*, DiagPtr> Visit(BodyRes_LocalVar& member)
    {
        throw NotImplementedException{};
    }

    expected<IrExp*, DiagPtr> Visit(BodyRes_LocalRef& member)
    {
        throw NotImplementedException{};
    }

    expected<IrExp*, DiagPtr> Visit(BodyRes_ThisVar& member)
    {
        throw NotImplementedException{};
    }


    /*TranslationResult<IntermediateRefExp> ISymbolQueryResultVisitor<TranslationResult<IntermediateRefExp>>.VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result)
    {
        return Fatal(A2014_ResolveIdentifier_MultipleCandidatesForMember);
    }*/
};

class IrExpAndMemberNameToIrExpTranslator
{
public:
    using ResultType = expected<IrExp*, DiagPtr>;

private:
    RName name;
    RTypeArguments* typeArgsExceptOuter;

    TranslationContexts& contexts;

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, IrExp>
    ResultType Value(TArgs&&... args)
    {
        return contexts.srtFactory->MakeIrExp<TValue>(forward<TArgs>(args)...);
    }

    ResultType HandleStaticParent(RDecl& decl, RTypeArguments* typeArgs)
    {
        auto o_member = decl.GetMember(typeArgs, name, typeArgsExceptOuter->GetCount());
        if (!o_member)
        {
            return Error<Error_ResolveIdentifier_NotFound>();
        }

        StaticParentTranslator binder(typeArgsExceptOuter, contexts);
        return visit(binder, *o_member);
    }

public:
    IrExpAndMemberNameToIrExpTranslator(const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContexts& contexts)
        : name{name}, typeArgsExceptOuter{typeArgsExceptOuter}, contexts{contexts}
    {
    }

    ResultType Visit(IrExp_Namespace* irBaseExp) 
    {
        return HandleStaticParent(*irBaseExp->decl, contexts.rFactory->MakeTypeArguments({}));
    }

    ResultType Visit(IrExp_Class* irBaseExp) 
    {
        return HandleStaticParent(*irBaseExp->decl, irBaseExp->typeArgs);
    }

    ResultType Visit(IrExp_Struct* irBaseExp) 
    {
        return HandleStaticParent(*irBaseExp->decl, irBaseExp->typeArgs);
    }

    ResultType Visit(IrExp_Static* irBaseExp) 
    {
        // irBaseExp->loc이 class일때
        auto* locType = irBaseExp->loc->GetType();

        if (auto* classType = dynamic_cast<RType_Class*>(locType))
        {
            auto e_result = GetClassVar(classType, name, typeArgsExceptOuter, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            return contexts.srtFactory->MakeIrExp<IrExp_ClassVar>(
                irBaseExp->loc, result.decl, result.typeArgs, contexts.rFactory);
        }
        else if (auto* structType = dynamic_cast<RType_Struct*>(locType))
        {
            auto e_result = GetStructVar(structType, name, typeArgsExceptOuter, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            return contexts.srtFactory->MakeIrExp<IrExp_StructVar>(
                irBaseExp, result.decl, result.typeArgs, contexts.rFactory);
        }
        else throw NotImplementedException{};
    }

    ResultType Visit(IrExp_ClassVar* irBaseExp)
    {
        auto* declType = irBaseExp->decl->GetDeclType(irBaseExp->typeArgs);

        if (auto* classType = dynamic_cast<RType_Class*>(declType))
        {
            auto e_result = GetClassVar(classType, name, typeArgsExceptOuter, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            auto* baseLoc = TranslateIrExp_ClassVarToMLoc(irBaseExp, contexts);
            return contexts.srtFactory->MakeIrExp<IrExp_ClassVar>(baseLoc, result.decl, result.typeArgs, contexts.rFactory);
        }
        else if (auto* structType = dynamic_cast<RType_Struct*>(declType))
        {
            auto e_result = GetStructVar(structType, name, typeArgsExceptOuter, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            auto* baseSharedExp = TranslateIrExp_ClassVarToMSharedExp(irBaseExp, contexts);
            return contexts.srtFactory->MakeIrExp<IrExp_StructVar>(baseSharedExp, result.decl, result.typeArgs, contexts.rFactory);
        }
        else throw NotImplementedException{};
    }

    ResultType Visit(IrExp_SharedStructVar* irBaseExp)
    {
        auto* declType = irBaseExp->decl->GetDeclType(irBaseExp->typeArgs);

        // &(pS->c).id 
        // TranslateIrExpAndMemberNameToMSharedExp(IrExp_SharedStructVar(pS, S::c), id)
        // => MSharedExp_ClassVar(pS->c, C::id)
        if (auto* classType = dynamic_cast<RType_Class*>(declType))
        {
            auto e_result = GetClassVar(classType, name, typeArgsExceptOuter, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            auto* baseLoc = TranslateIrExp_SharedStructVarToMLoc(irBaseExp, contexts);
            return contexts.srtFactory->MakeIrExp<IrExp_ClassVar>(baseLoc, result.decl, result.typeArgs, contexts.rFactory);
        }
        else if (auto* structType = dynamic_cast<RType_Struct*>(declType))
        {
            auto e_result = GetStructVar(structType, name, typeArgsExceptOuter, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            auto baseSharedExp = TranslateIrExp_SharedStructVarToMSharedExp(irBaseExp, contexts);
            return contexts.srtFactory->MakeIrExp<IrExp_StructVar>(baseSharedExp, result.decl, result.typeArgs, contexts.rFactory);
        }
        else throw NotImplementedException{};
    }

    ResultType Visit(IrExp_StructVar* irBaseExp)
    {
        auto* declType = irBaseExp->decl->GetDeclType(irBaseExp->typeArgs);

        if (auto* classDeclType = dynamic_cast<RType_Class*>(declType))
        {
            auto e_result = GetClassVar(classDeclType, name, typeArgsExceptOuter, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            auto e_baseLoc = TranslateIrExp_StructVarToMLoc(irBaseExp, contexts);
            RETURN_ON_ERROR(e_baseLoc);

            return contexts.srtFactory->MakeIrExp<IrExp_ClassVar>(*e_baseLoc, result.decl, result.typeArgs, contexts.rFactory);
        }
        else if (auto* structDeclType = dynamic_cast<RType_Struct*>(declType))
        {
            auto e_result = GetStructVar(structDeclType, name, typeArgsExceptOuter, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            auto e_baseSharedExp = TranslateIrExp_StructVarToMSharedExp(irBaseExp, contexts);
            RETURN_ON_ERROR(e_baseSharedExp);

            return contexts.srtFactory->MakeIrExp<IrExp_StructVar>(*e_baseSharedExp, result.decl, result.typeArgs, contexts.rFactory);
        }
        else throw NotImplementedException{};
    }
    
    // *pS, 오직 value type에만 작동을 하도록 보장해야 한다
    ResultType Visit(IrExp_Deref* irBaseExp)
    {
        // if (!sharedLocType) return Error<Error_SharedTranslation_MemberBaseShouldBeShared>();

        // 1. *pS꼴이라면
        if (auto* sharedLocType = dynamic_cast<RType_Shared*>(irBaseExp->innerLoc->GetType()))
        {
            // shared<S>
            auto* structTargetLocType = dynamic_cast<RType_Struct*>(sharedLocType->innerType);
            if (!structTargetLocType) return Error<Error_SharedTranslation_MemberBaseShouldBeShared>();

            auto e_result = GetStructVar(structTargetLocType, name, typeArgsExceptOuter, /*bExpectedStatic*/false);
            RETURN_ON_ERROR_REFDECL(e_result, result);

            return contexts.srtFactory->MakeIrExp<IrExp_SharedStructVar>(irBaseExp->innerLoc, result.decl, result.typeArgs, contexts.rFactory);
        }


    }

    ResultType Visit(IrExp_Exp* irExp) 
    {
        // exp.id
        // 함수 호출 인자 제외 temp 참조 불가
        static_assert(false);
    }

    ResultType Visit(IrExp_Loc* irExp)
    {
        // loc.id
        static_assert(false);
    }
};

} // namespace 

expected<IrExp*, DiagPtr> TranslateIrExpAndMemberNameToIrExp(IrExp* irExp, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContexts& contexts)
{
    IrExpAndMemberNameToIrExpTranslator binder{name, typeArgsExceptOuter, contexts};
    return Accept(binder, irExp);
}

} // namespace Citron

