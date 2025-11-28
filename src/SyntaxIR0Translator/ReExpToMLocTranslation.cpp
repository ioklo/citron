#include "ReExpToMLocTranslation.h"

#include "Infra/Ptr.h"
#include "Logging/Logger.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "NSymbol/NLambdaVarDecl.h"
#include "MIR/MLoc.h"

#include "TranslationContext.h"
#include "ScopeContext.h"
#include "DesignatedDiagnostic.h"
#include "ReExp.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

expected<MLoc*, DiagPtr> TranslateReThisVarExpToMLoc(ReExp_ThisVar* reExp, TranslationContext& context) // nothrow
{
    return context.MakeThisLoc();
}

expected<MLoc*, DiagPtr> TranslateReClassVarExpToMLoc(ReExp_ClassVar* reExp, TranslationContext& context)
{
    if (reExp->hasExplicitInstance) // c.x, C.x 둘다 해당
    {
        MLoc* instance = nullptr;
        
        if (reExp->explicitInstance != nullptr)
        {   
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
            auto eInstance = TranslateReExpToMLoc(reExp->explicitInstance, /* bWrapExpAsLoc */ true, &designatedDiag, context);

            if (!eInstance)
                return unexpected{move(eInstance).error()};

            instance = *eInstance;
        }

        return context.MakeNLoc<MLoc_ClassVar>(instance, reExp->decl, reExp->typeArgs);
    }
    else // x, x (static) 둘다 해당
    {   
        MLoc* nInstanceLoc = reExp->decl->IsStatic()? nullptr : context.MakeThisLoc();
        return context.MakeNLoc<MLoc_ClassVar>(nInstanceLoc, reExp->decl, reExp->typeArgs);
    }
}

expected<MLoc*, DiagPtr> TranslateReLocalVarExpToMLoc(ReExp_LocalVar* reExp, TranslationContext& context)
{
    return context.MakeNLoc<MLoc_LocalVar>(reExp->name, reExp->type);
}

expected<MLoc*, DiagPtr> TranslateReLambdaVarExpToMLoc(ReExp_LambdaVar* reExp, TranslationContext& context)
{
    return context.MakeNLoc<MLoc_LambdaVar>(reExp->decl, reExp->typeArgs);
}

expected<MLoc*, DiagPtr> TranslateReStructVarExpToMLoc(ReExp_StructVar* reExp, TranslationContext& context)
{
    if (reExp->hasExplicitInstance) // c.x, C.x 둘다 해당
    {
        MLoc* instance = nullptr;

        if (reExp->explicitInstance != nullptr)
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
            auto eInstance = TranslateReExpToMLoc(reExp->explicitInstance, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
            if (!eInstance) return unexpected{move(eInstance).error()};

            instance = *eInstance;
        }

        return context.MakeNLoc<MLoc_StructVar>(instance, reExp->decl, reExp->typeArgs);
    }
    else // x, x (static) 둘다 해당
    {   
        // TODO: [10] box 함수 내부이면, local ptr대신 box ptr로 변경해야 한다
        MLoc* nInstanceLoc = reExp->decl->IsStatic() ? nullptr : context.MakeNLoc<MLoc_LocalDeref>(context.MakeThisLoc());
        return context.MakeNLoc<MLoc_StructVar>(nInstanceLoc, reExp->decl, reExp->typeArgs);
    }
}

expected<MLoc*, DiagPtr> TranslateReEnumElemVarExpToMLoc(ReExp_EnumElemVar* reExp, TranslationContext& context)
{   
    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

    auto eInst = TranslateReExpToMLoc(reExp->instance, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
    if (!eInst) return unexpected{move(eInst).error()};

    return context.MakeNLoc<MLoc_EnumElemVar>(*eInst, reExp->decl, reExp->typeArgs);
}

expected<MLoc*, DiagPtr> TranslateReListIndexerExpToMLoc(ReExp_ListIndexer* reExp, TranslationContext& context)
{
    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

    auto eInst = TranslateReExpToMLoc(reExp->instance, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
    if (!eInst) return unexpected{move(eInst).error()};

    auto eIndex = TranslateReExpToMLoc(reExp->index, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
    if (!eIndex) return unexpected{move(eIndex).error()};

    return context.MakeNLoc<MLoc_ListIndexer>(*eInst, *eIndex, reExp->itemType);
}

expected<MLoc*, DiagPtr> TranslateReLocalDerefExpToMLoc(ReExp_LocalDeref* reExp, TranslationContext& context)
{
    // *x, *G()
    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

    auto eTarget = TranslateReExpToMLoc(reExp->target, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
    if (!eTarget) return unexpected{move(eTarget).error()};

    return context.MakeNLoc<MLoc_LocalDeref>(*eTarget);
}

expected<MLoc*, DiagPtr> TranslateReBoxDerefExpToMLoc(ReExp_BoxDeref* reExp, TranslationContext& context)
{
    // *x, *G()
    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

    auto eTarget = TranslateReExpToMLoc(reExp->target, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
    if (!eTarget) return unexpected{move(eTarget).error()};

    return context.MakeNLoc<MLoc_BoxDeref>(*eTarget);
}

namespace {

class ReExpToMLocTranslator
{
public:
    using ResultType = expected<MLoc*, DiagPtr>;

private:
    bool bWrapExpAsLoc;
    IDesignatedDiagnostic* notLocationDiag;
    TranslationContext& context;

public:
    ReExpToMLocTranslator(bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContext& context)
        : bWrapExpAsLoc(bWrapExpAsLoc), notLocationDiag(notLocationDiag), context(context)
    {
    }

    ResultType Visit(ReExp_ThisVar* exp)
    {
        return TranslateReThisVarExpToMLoc(exp, context);
    }

    ResultType Visit(ReExp_LocalVar* exp)
    {
        return TranslateReLocalVarExpToMLoc(exp, context);
    }

    ResultType Visit(ReExp_LambdaVar* exp)
    {
        return TranslateReLambdaVarExpToMLoc(exp, context);
    }

    ResultType Visit(ReExp_ClassVar* exp)
    {
        return TranslateReClassVarExpToMLoc(exp, context);
    }

    ResultType Visit(ReExp_StructVar* exp)
    {
        return TranslateReStructVarExpToMLoc(exp, context);
    }

    ResultType Visit(ReExp_EnumElemVar* exp)
    {
        return TranslateReEnumElemVarExpToMLoc(exp, context);
    }

    ResultType Visit(ReExp_LocalDeref* exp)
    {
        return TranslateReLocalDerefExpToMLoc(exp, context);
    }

    ResultType Visit(ReExp_BoxDeref* exp)
    {
        return TranslateReBoxDerefExpToMLoc(exp, context);
    }

    ResultType Visit(ReExp_ListIndexer* exp)
    {
        return TranslateReListIndexerExpToMLoc(exp, context);
    }

    ResultType Visit(ReExp_Else* exp)
    {
        if (bWrapExpAsLoc)
        {
            return context.MakeNLoc<MLoc_Temp>(exp->mExp);
        }
        else
        {
            return unexpected{notLocationDiag->MakeDiag()};
        }
    }
};

}

expected<MLoc*, DiagPtr> TranslateReExpToMLoc(ReExp* reExp, bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContext& context)
{
    ReExpToMLocTranslator translator{bWrapExpAsLoc, notLocationDiag, context};
    return Accept(translator, reExp);
}

}
