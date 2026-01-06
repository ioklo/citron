#include "ReExpToMLocTranslation.h"

#include "Infra/Ptr.h"
#include "Infra/Expected.h"
#include "Logging/Logger.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "NSymbol/NLambdaVarDecl.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"

#include "ScopeContext.h"
#include "DesignatedDiagnostic.h"
#include "ReExp.h"
#include "TranslationContexts.h"
#include "FuncContext.h"

using namespace std;

namespace Citron {

expected<MLoc*, DiagPtr> TranslateReThisVarExpToMLoc(ReExp_ThisVar* reExp, TranslationContexts& contexts) // nothrow
{
    return contexts.funcContext->MakeThisLoc();
}

expected<MLoc*, DiagPtr> TranslateReClassVarExpToMLoc(ReExp_ClassVar* reExp, TranslationContexts& contexts)
{
    if (reExp->hasExplicitInstance) // c.x, C.x 둘다 해당
    {
        MLoc* instance = nullptr;
        
        if (reExp->explicitInstance != nullptr)
        {   
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
            auto e_instance = TranslateReExpToMLoc(reExp->explicitInstance, /* bWrapExpAsLoc */ true, &designatedDiag, contexts);

            if (!e_instance)
                return unexpected{move(e_instance).error()};

            instance = *e_instance;
        }

        return contexts.mFactory->MakeMLoc<MLoc_ClassVar>(instance, reExp->decl, reExp->typeArgs);
    }
    else // x, x (static) 둘다 해당
    {   
        MLoc* nInstanceLoc = reExp->decl->IsStatic()? nullptr : contexts.funcContext->MakeThisLoc();
        return contexts.mFactory->MakeMLoc<MLoc_ClassVar>(nInstanceLoc, reExp->decl, reExp->typeArgs);
    }
}

expected<MLoc*, DiagPtr> TranslateReLocalVarExpToMLoc(ReExp_LocalVar* reExp, TranslationContexts& contexts)
{
    return contexts.mFactory->MakeMLoc<MLoc_LocalVar>(reExp->name, reExp->type);
}

expected<MLoc*, DiagPtr> TranslateReLocalRefExpToMLoc(ReExp_LocalRef* reExp, TranslationContexts& contexts)
{
    return contexts.mFactory->MakeMLoc<MLoc_LocalRef>(reExp->name, reExp->type);
}

expected<MLoc*, DiagPtr> TranslateReLambdaVarExpToMLoc(ReExp_LambdaVar* reExp, TranslationContexts& contexts)
{
    return contexts.mFactory->MakeMLoc<MLoc_LambdaVar>(reExp->decl, reExp->typeArgs);
}

expected<MLoc*, DiagPtr> TranslateReStructVarExpToMLoc(ReExp_StructVar* reExp, TranslationContexts& contexts)
{
    if (reExp->hasExplicitInstance) // c.x, C.x 둘다 해당
    {
        MLoc* instance = nullptr;

        if (reExp->explicitInstance != nullptr)
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
            auto e_instance = TranslateReExpToMLoc(reExp->explicitInstance, /*bWrapExpAsLoc*/ true, &designatedDiag, contexts);
            RETURN_ON_ERROR(e_instance);

            instance = *e_instance;
        }

        return contexts.mFactory->MakeMLoc<MLoc_StructVar>(instance, reExp->decl, reExp->typeArgs);
    }
    else // x, x (static) 둘다 해당
    {   
        // TODO: [10] box 함수 내부이면, local ptr대신 box ptr로 변경해야 한다
        MLoc* nInstanceLoc = reExp->decl->IsStatic() ? nullptr : contexts.funcContext->MakeThisLoc();
        return contexts.mFactory->MakeMLoc<MLoc_StructVar>(nInstanceLoc, reExp->decl, reExp->typeArgs);
    }
}

expected<MLoc*, DiagPtr> TranslateReEnumElemVarExpToMLoc(ReExp_EnumElemVar* reExp, TranslationContexts& contexts)
{   
    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

    auto e_inst = TranslateReExpToMLoc(reExp->instance, /*bWrapExpAsLoc*/ true, &designatedDiag, contexts);
    RETURN_ON_ERROR(e_inst);

    return contexts.mFactory->MakeMLoc<MLoc_EnumElemVar>(*e_inst, reExp->decl, reExp->typeArgs);
}

expected<MLoc*, DiagPtr> TranslateReListIndexerExpToMLoc(ReExp_ListIndexer* reExp, TranslationContexts& contexts)
{
    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

    auto e_inst = TranslateReExpToMLoc(reExp->instance, /*bWrapExpAsLoc*/ true, &designatedDiag, contexts);
    RETURN_ON_ERROR(e_inst);

    auto e_index = TranslateReExpToMLoc(reExp->index, /*bWrapExpAsLoc*/ true, &designatedDiag, contexts);
    RETURN_ON_ERROR(e_index);

    return contexts.mFactory->MakeMLoc<MLoc_ListIndexer>(*e_inst, *e_index, reExp->itemType);
}

expected<MLoc*, DiagPtr> TranslateReDerefExpToMLoc(ReExp_PtrDeref* reExp, TranslationContexts& contexts)
{
    // *x, *G()
    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

    auto e_target = TranslateReExpToMLoc(reExp->target, /*bWrapExpAsLoc*/ true, &designatedDiag, contexts);
    RETURN_ON_ERROR(e_target);

    return contexts.mFactory->MakeMLoc<MLoc_PtrDeref>(*e_target);
}

expected<MLoc*, DiagPtr> TranslateReBoxDerefExpToMLoc(ReExp_BoxDeref* reExp, TranslationContexts& contexts)
{
    // *x, *G()
    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

    auto e_target = TranslateReExpToMLoc(reExp->target, /*bWrapExpAsLoc*/ true, &designatedDiag, contexts);
    RETURN_ON_ERROR(e_target);

    return contexts.mFactory->MakeMLoc<MLoc_BoxDeref>(*e_target);
}

namespace {

class ReExpToMLocTranslator
{
public:
    using ResultType = expected<MLoc*, DiagPtr>;

private:
    bool bWrapExpAsLoc;
    IDesignatedDiagnostic* notLocationDiag;
    TranslationContexts& contexts;

public:
    ReExpToMLocTranslator(bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContexts& contexts)
        : bWrapExpAsLoc(bWrapExpAsLoc), notLocationDiag(notLocationDiag), contexts{contexts}
    {
    }

    ResultType Visit(ReExp_ThisVar* exp)
    {
        return TranslateReThisVarExpToMLoc(exp, contexts);
    }

    ResultType Visit(ReExp_LocalVar* exp)
    {
        return TranslateReLocalVarExpToMLoc(exp, contexts);
    }

    ResultType Visit(ReExp_LocalRef* exp)
    {
        return TranslateReLocalRefExpToMLoc(exp, contexts);
    }

    ResultType Visit(ReExp_LambdaVar* exp)
    {
        return TranslateReLambdaVarExpToMLoc(exp, contexts);
    }

    ResultType Visit(ReExp_ClassVar* exp)
    {
        return TranslateReClassVarExpToMLoc(exp, contexts);
    }

    ResultType Visit(ReExp_StructVar* exp)
    {
        return TranslateReStructVarExpToMLoc(exp, contexts);
    }

    ResultType Visit(ReExp_EnumElemVar* exp)
    {
        return TranslateReEnumElemVarExpToMLoc(exp, contexts);
    }

    ResultType Visit(ReExp_PtrDeref* exp)
    {
        return TranslateReDerefExpToMLoc(exp, contexts);
    }

    ResultType Visit(ReExp_BoxDeref* exp)
    {
        return TranslateReBoxDerefExpToMLoc(exp, contexts);
    }

    ResultType Visit(ReExp_ListIndexer* exp)
    {
        return TranslateReListIndexerExpToMLoc(exp, contexts);
    }

    ResultType Visit(ReExp_Else* exp)
    {
        if (bWrapExpAsLoc)
        {
            return contexts.mFactory->MakeMLoc<MLoc_Temp>(exp->mExp);
        }
        else
        {
            return unexpected{notLocationDiag->MakeDiag()};
        }
    }
};

}

expected<MLoc*, DiagPtr> TranslateReExpToMLoc(ReExp* reExp, bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContexts& contexts)
{
    ReExpToMLocTranslator translator{bWrapExpAsLoc, notLocationDiag, contexts};
    return Accept(translator, reExp);
}

}
