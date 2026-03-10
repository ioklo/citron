//#include "IrExpAndMemberNameToMSharedExpTranslation.h"
//
//#include "Infra/Exceptions.h"
//#include "Infra/Expected.h"
//#include "Logging/Diag.h"
//#include "RSymbol/RTypes.h"
//#include "RSymbol/RTypeArguments.h"
//#include "RSymbol/RClassDecl.h"
//#include "RSymbol/RClassVarDecl.h"
//#include "RSymbol/RStructDecl.h"
//#include "RSymbol/RStructVarDecl.h"
//#include "MIR/MSharedExp.h"
//#include "MIR/MLoc.h"
//#include "MIR/MFactory.h"
//
//#include "IrExp.h"
//#include "Misc.h"
//#include "TranslationContexts.h"
//#include "IrExpAndMemberNameTranslation.h"
//#include "IrExpToMLocTranslation.h"
//#include "IrExpToMSharedExpTranslation.h"
//
//using namespace std;
//
//namespace Citron {
//
//struct IrExpAndMemberNameToMSharedExpTranslator
//{
//    using ResultType = expected<MSharedExp*, DiagPtr>;
//
//    const RName& name;
//    RTypeArguments* typeArgsExceptOuter;
//    TranslationContexts& contexts;
//
//    IrExpAndMemberNameToMSharedExpTranslator(const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContexts& contexts)
//        : name{name}, typeArgsExceptOuter{typeArgsExceptOuter}, contexts{contexts}
//    {}
//
//    // 단일 오브젝트는 shared로 만들수 없다. (&s 불가) 따라서, NS.x는 불가
//    ResultType Visit(IrExp_Namespace* irBaseExp) 
//    {
//        return Error<Error_SharedTranslation_SingleRefNotAllowed>();
//    }
//
//    // &C.id
//    ResultType Visit(IrExp_Class* irBaseExp) 
//    {
//        auto o_rMember = irBaseExp->decl->GetMember(irBaseExp->typeArgs, name, typeArgsExceptOuter->GetCount());
//        if (!o_rMember) return Error<Error_ResolveIdentifier_NotFound>();
//
//        auto* rClassVarMember = get_if<RDeclRes_ClassVar>(&*o_rMember);
//        if (!rClassVarMember) return Error<Error_SharedTranslation_StaticSharedShouldBeVar>();
//
//        // static만 가능
//        if (!rClassVarMember->decl->IsStatic()) return Error<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>();
//
//        MLoc* loc = contexts.mFactory->MakeMLoc<MLoc_ClassVar>(/*instance*/nullptr, rClassVarMember->decl, rClassVarMember->typeArgs);
//        return contexts.mFactory->MakeMSharedExp<MSharedExp_Static>(loc, contexts.rFactory);
//    }
//
//    ResultType Visit(IrExp_Struct* irBaseExp)
//    {
//        auto o_rMember = irBaseExp->decl->GetMember(irBaseExp->typeArgs, name, typeArgsExceptOuter->GetCount());
//        if (!o_rMember) return Error<Error_ResolveIdentifier_NotFound>();
//
//        auto* rStructVarMember = get_if<RDeclRes_StructVar>(&*o_rMember);
//        if (!rStructVarMember) return Error<Error_SharedTranslation_StaticSharedShouldBeVar>();
//
//        // static만 가능
//        if (!rStructVarMember->decl->IsStatic()) return Error<Error_ResolveIdentifier_CantGetInstanceMemberThroughType>();
//
//        MLoc* loc = contexts.mFactory->MakeMLoc<MLoc_ClassVar>(/*instance*/nullptr, rStructVarMember->decl, rStructVarMember->typeArgs);
//        return contexts.mFactory->MakeMSharedExp<MSharedExp_Static>(loc, contexts.rFactory);
//    }
//    
//    // base 부분이 static인 경우
//    ResultType Visit(IrExp_Static* irBaseExp) 
//    {
//        // irBaseExp->loc이 class일때
//        auto* locType = irBaseExp->loc->GetType();
//
//        if (auto* classType = dynamic_cast<RType_Class*>(locType))
//        {
//            auto e_result = GetClassVar(classType, name, typeArgsExceptOuter, /*bExpectedStatic*/true);
//            RETURN_ON_ERROR_REFDECL(e_result, result);
//
//            return contexts.mFactory->MakeMSharedExp<MSharedExp_ClassVar>(
//                irBaseExp->loc, result.decl, result.typeArgs, contexts.rFactory);
//        }
//        else if (auto* structType = dynamic_cast<RType_Struct*>(locType))
//        {
//            auto e_result = GetStructVar(structType, name, typeArgsExceptOuter, /*bExpectedStatic*/true);
//            RETURN_ON_ERROR_REFDECL(e_result, result);
//
//            auto* baseSharedExp = contexts.mFactory->MakeMSharedExp<MSharedExp_Static>(irBaseExp->loc, contexts.rFactory);
//
//            return contexts.mFactory->MakeMSharedExp<MSharedExp_StructVar>(
//                baseSharedExp, result.decl, result.typeArgs, contexts.rFactory);
//        }
//        else throw NotImplementedException{};
//    }
//
//    // &(c.x).id
//    ResultType Visit(IrExp_ClassVar* irBaseExp) 
//    {   
//        auto* declType = irBaseExp->decl->GetDeclType(*irBaseExp->typeArgs);
//
//        if (auto* classType = dynamic_cast<RType_Class*>(declType))
//        {
//            auto e_result = GetClassVar(classType, name, typeArgsExceptOuter, /*bExpectedStatic*/false);
//            RETURN_ON_ERROR_REFDECL(e_result, result);
//
//            auto* baseLoc = TranslateIrExp_ClassVarToMLoc(irBaseExp, contexts);
//            return contexts.mFactory->MakeMSharedExp<MSharedExp_ClassVar>(baseLoc, result.decl, result.typeArgs, contexts.rFactory);
//        }
//        else if (auto* structType = dynamic_cast<RType_Struct*>(declType))
//        {
//            auto e_result = GetStructVar(structType, name, typeArgsExceptOuter, /*bExpectedStatic*/false);
//            RETURN_ON_ERROR_REFDECL(e_result, result);
//
//            auto* baseSharedExp = TranslateIrExp_ClassVarToMSharedExp(irBaseExp, contexts);
//            return contexts.mFactory->MakeMSharedExp<MSharedExp_StructVar>(baseSharedExp, result.decl, result.typeArgs, contexts.rFactory);
//        }
//        else throw NotImplementedException{};
//    }
//
//    // 
//    ResultType Visit(IrExp_SharedStructVar* irBaseExp) 
//    {
//        auto* declType = irBaseExp->decl->GetDeclType(*irBaseExp->typeArgs);
//
//        // &(pS->c).id 
//        // TranslateIrExpAndMemberNameToMSharedExp(IrExp_SharedStructVar(pS, S::c), id)
//        // => MSharedExp_ClassVar(pS->c, C::id)
//        if (auto* classType = dynamic_cast<RType_Class*>(declType))
//        {
//            auto e_result = GetClassVar(classType, name, typeArgsExceptOuter, /*bExpectedStatic*/false);
//            RETURN_ON_ERROR_REFDECL(e_result, result);
//
//            auto* baseLoc = TranslateIrExp_SharedStructVarToMLoc(irBaseExp, contexts);
//            return contexts.mFactory->MakeMSharedExp<MSharedExp_ClassVar>(baseLoc, result.decl, result.typeArgs, contexts.rFactory);
//        }
//        else if (auto* structType = dynamic_cast<RType_Struct*>(declType))
//        {
//            auto e_result = GetStructVar(structType, name, typeArgsExceptOuter, /*bExpectedStatic*/false);
//            RETURN_ON_ERROR_REFDECL(e_result, result);
//
//            auto baseSharedExp = TranslateIrExp_SharedStructVarToMSharedExp(irBaseExp, contexts);
//            return contexts.mFactory->MakeMSharedExp<MSharedExp_StructVar>(baseSharedExp, result.decl, result.typeArgs, contexts.rFactory);
//        }
//        else throw NotImplementedException{};
//    }
//
//    // &c.s.x . id
//    // &C.s.x . id
//    // IrExp_StructVar(base, decl, typeArgs)
//    ResultType Visit(IrExp_StructVar* irBaseExp)
//    {
//        auto* declType = irBaseExp->decl->GetDeclType(*irBaseExp->typeArgs);
//
//        if (auto* classDeclType = dynamic_cast<RType_Class*>(declType))
//        {
//            auto e_result = GetClassVar(classDeclType, name, typeArgsExceptOuter, /*bExpectedStatic*/false);
//            RETURN_ON_ERROR_REFDECL(e_result, result);
//
//            auto e_baseLoc = TranslateIrExp_StructVarToMLoc(irBaseExp, contexts);
//            RETURN_ON_ERROR(e_baseLoc);
//
//            return contexts.mFactory->MakeMSharedExp<MSharedExp_ClassVar>(*e_baseLoc, result.decl, result.typeArgs, contexts.rFactory);
//        }
//        else if (auto* structDeclType = dynamic_cast<RType_Struct*>(declType))
//        {
//            auto e_result = GetStructVar(structDeclType, name, typeArgsExceptOuter, /*bExpectedStatic*/false);
//            RETURN_ON_ERROR_REFDECL(e_result, result);
//
//            auto e_baseSharedExp = TranslateIrExp_StructVarToMSharedExp(irBaseExp, contexts);
//            RETURN_ON_ERROR(e_baseSharedExp);
//            
//            return contexts.mFactory->MakeMSharedExp<MSharedExp_StructVar>(*e_baseSharedExp, result.decl, result.typeArgs, contexts.rFactory);
//        }
//        else throw NotImplementedException{};
//    }
//
//    // (*pS).id => MSharedExp_SharedStructVar(pS, S::id)
//    ResultType Visit(IrExp_SharedDeref* irBaseExp) 
//    {
//        auto* sharedLocType = dynamic_cast<RType_Shared*>(irBaseExp->innerLoc->GetType());
//        if (!sharedLocType) return Error<Error_SharedTranslation_MemberBaseShouldBeShared>();
//
//        // shared<S>
//        auto* structTargetLocType = dynamic_cast<RType_Struct*>(sharedLocType->innerType);
//        if (!structTargetLocType) return Error<Error_SharedTranslation_MemberBaseShouldBeShared>();
//
//        auto e_result = GetStructVar(structTargetLocType, name, typeArgsExceptOuter, /*bExpectedStatic*/false);
//        RETURN_ON_ERROR_REFDECL(e_result, result);
//
//        return contexts.mFactory->MakeMSharedExp<MSharedExp_SharedStructVar>(irBaseExp->innerLoc, result.decl, result.typeArgs, contexts.rFactory);
//    }
//
//    ResultType Visit(IrExp_Exp* irBaseExp) 
//    {
//        throw NotImplementedException{};
//    }
//
//    ResultType Visit(IrExp_Loc* irBaseExp) 
//    {
//        throw NotImplementedException{};
//    }
//};
//
//expected<MSharedExp*, DiagPtr> TranslateIrExpAndMemberNameToMSharedExp(IrExp* irBaseExp, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContexts& contexts)
//{
//    IrExpAndMemberNameToMSharedExpTranslator translator{name, typeArgsExceptOuter, contexts};
//    return Accept(translator, irBaseExp);
//}
//
//} // namespace Citron