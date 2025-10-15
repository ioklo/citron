#include "ReExpToNLocTranslation.h"

#include "Infra/Ptr.h"
#include "Logging/Logger.h"
#include "IR0/RClassVarDecl.h"
#include "IR0/RStructVarDecl.h"
#include "IR0/NLambdaVarDecl.h"
#include "IR0/NLoc.h"

#include "TranslationContext.h"
#include "ScopeContext.h"
#include "DesignatedDiagnostic.h"
#include "ReExp.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

expected<NLoc*, DiagPtr> TranslateReThisVarExpToNLoc(ReExp_ThisVar* reExp, TranslationContext& context) // nothrow
{
    return context.MakeThisLoc();
}

expected<NLoc*, DiagPtr> TranslateReClassVarExpToNLoc(ReExp_ClassVar* reExp, TranslationContext& context)
{
    if (reExp->hasExplicitInstance) // c.x, C.x 둘다 해당
    {
        NLoc* instance = nullptr;
        
        if (reExp->explicitInstance != nullptr)
        {   
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
            auto eInstance = TranslateReExpToNLoc(reExp->explicitInstance, /* bWrapExpAsLoc */ true, &designatedDiag, context);

            if (!eInstance)
                return unexpected{move(eInstance).error()};

            instance = *eInstance;
        }

        return context.MakeNLoc<NLoc_ClassVar>(instance, reExp->decl, reExp->typeArgs);
    }
    else // x, x (static) 둘다 해당
    {   
        NLoc* nInstanceLoc = reExp->decl->IsStatic()? nullptr : context.MakeThisLoc();
        return context.MakeNLoc<NLoc_ClassVar>(nInstanceLoc, reExp->decl, reExp->typeArgs);
    }
}

expected<NLoc*, DiagPtr> TranslateReLocalVarExpToNLoc(ReExp_LocalVar* reExp, TranslationContext& context)
{
    return context.MakeNLoc<NLoc_LocalVar>(RName_Normal(reExp->name), reExp->type);
}

expected<NLoc*, DiagPtr> TranslateReLambdaVarExpToNLoc(ReExp_LambdaVar* reExp, TranslationContext& context)
{
    return context.MakeNLoc<NLoc_LambdaVar>(reExp->decl, reExp->typeArgs);
}

expected<NLoc*, DiagPtr> TranslateReStructVarExpToNLoc(ReExp_StructVar* reExp, TranslationContext& context)
{
    if (reExp->hasExplicitInstance) // c.x, C.x 둘다 해당
    {
        NLoc* instance = nullptr;

        if (reExp->explicitInstance != nullptr)
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
            auto eInstance = TranslateReExpToNLoc(reExp->explicitInstance, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
            if (!eInstance) return unexpected{move(eInstance).error()};

            instance = *eInstance;
        }

        return context.MakeNLoc<NLoc_StructVar>(instance, reExp->decl, reExp->typeArgs);
    }
    else // x, x (static) 둘다 해당
    {   
        // TODO: [10] box 함수 내부이면, local ptr대신 box ptr로 변경해야 한다
        NLoc* nInstanceLoc = reExp->decl->IsStatic() ? nullptr : context.MakeNLoc<NLoc_LocalDeref>(context.MakeThisLoc());
        return context.MakeNLoc<NLoc_StructVar>(nInstanceLoc, reExp->decl, reExp->typeArgs);
    }
}

expected<NLoc*, DiagPtr> TranslateReEnumElemVarExpToNLoc(ReExp_EnumElemVar* reExp, TranslationContext& context)
{   
    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

    auto eInst = TranslateReExpToNLoc(reExp->instance, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
    if (!eInst) return unexpected{move(eInst).error()};

    return context.MakeNLoc<NLoc_EnumElemVar>(*eInst, reExp->decl, reExp->typeArgs);
}

expected<NLoc*, DiagPtr> TranslateReListIndexerExpToNLoc(ReExp_ListIndexer* reExp, TranslationContext& context)
{
    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

    auto eInst = TranslateReExpToNLoc(reExp->instance, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
    if (!eInst) return unexpected{move(eInst).error()};

    auto eIndex = TranslateReExpToNLoc(reExp->index, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
    if (!eIndex) return unexpected{move(eIndex).error()};

    return context.MakeNLoc<NLoc_ListIndexer>(*eInst, *eIndex, reExp->itemType);
}

expected<NLoc*, DiagPtr> TranslateReLocalDerefExpToNLoc(ReExp_LocalDeref* reExp, TranslationContext& context)
{
    // *x, *G()
    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

    auto eTarget = TranslateReExpToNLoc(reExp->target, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
    if (!eTarget) return unexpected{move(eTarget).error()};

    return context.MakeNLoc<NLoc_LocalDeref>(*eTarget);
}

expected<NLoc*, DiagPtr> TranslateReBoxDerefExpToNLoc(ReExp_BoxDeref* reExp, TranslationContext& context)
{
    // *x, *G()
    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

    auto eTarget = TranslateReExpToNLoc(reExp->target, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
    if (!eTarget) return unexpected{move(eTarget).error()};

    return context.MakeNLoc<NLoc_BoxDeref>(*eTarget);
}

namespace {

class ReExpToNLocTranslator
{
public:
    using ResultType = expected<NLoc*, DiagPtr>;

private:
    bool bWrapExpAsLoc;
    IDesignatedDiagnostic* notLocationDiag;
    TranslationContext& context;

public:
    ReExpToNLocTranslator(bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContext& context)
        : bWrapExpAsLoc(bWrapExpAsLoc), notLocationDiag(notLocationDiag), context(context)
    {
    }

    ResultType Visit(ReExp_ThisVar* exp)
    {
        return TranslateReThisVarExpToNLoc(exp, context);
    }

    ResultType Visit(ReExp_LocalVar* exp)
    {
        return TranslateReLocalVarExpToNLoc(exp, context);
    }

    ResultType Visit(ReExp_LambdaVar* exp)
    {
        return TranslateReLambdaVarExpToNLoc(exp, context);
    }

    ResultType Visit(ReExp_ClassVar* exp)
    {
        return TranslateReClassVarExpToNLoc(exp, context);
    }

    ResultType Visit(ReExp_StructVar* exp)
    {
        return TranslateReStructVarExpToNLoc(exp, context);
    }

    ResultType Visit(ReExp_EnumElemVar* exp)
    {
        return TranslateReEnumElemVarExpToNLoc(exp, context);
    }

    ResultType Visit(ReExp_LocalDeref* exp)
    {
        return TranslateReLocalDerefExpToNLoc(exp, context);
    }

    ResultType Visit(ReExp_BoxDeref* exp)
    {
        return TranslateReBoxDerefExpToNLoc(exp, context);
    }

    ResultType Visit(ReExp_ListIndexer* exp)
    {
        return TranslateReListIndexerExpToNLoc(exp, context);
    }

    ResultType Visit(ReExp_Else* exp)
    {
        if (bWrapExpAsLoc)
        {
            return context.MakeNLoc<NLoc_Temp>(exp->nExp);
        }
        else
        {
            return unexpected{notLocationDiag->MakeDiag()};
        }
    }
};

}

expected<NLoc*, DiagPtr> TranslateReExpToNLoc(ReExp* reExp, bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContext& context)
{
    ReExpToNLocTranslator translator{bWrapExpAsLoc, notLocationDiag, context};
    return Accept(translator, reExp);
}

}
