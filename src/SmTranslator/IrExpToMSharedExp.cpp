#include "IrExpToMSharedExp.h"

#include "Infra/Expected.h"
#include "MIR/MSharedExp.h"
#include "MIR/MFactory.h"

#include "SmTranslationContexts.h"
#include "IrExp.h"
#include "Misc.h"

using namespace std;

namespace Citron {

expected<MSharedExp*, DiagPtr> TranslateIrExp_StructVarToMSharedExp(IrExp_StructVar* irExp, SmTranslationContexts& contexts)
{
    auto e_baseSharedExp = TranslateIrExpToMSharedExp(irExp->base, contexts);
    RETURN_ON_ERROR(e_baseSharedExp);

    struct Visitor
    {
        using ResultType = expected<MSharedExp*, DiagPtr>;
        IrExp_StructVar* irExp;

        ResultType Visit(MSharedExp_Static* sharedExp)
        {
            sharedExp->segments.emplace_back(irExp->appliedDecl);
            return sharedExp;
        }

        ResultType Visit(MSharedExp_ClassVar* sharedExp)
        {
            sharedExp->segments.emplace_back(irExp->appliedDecl);
            return sharedExp;
        }

        ResultType Visit(MSharedExp_SharedStructVar* sharedExp)
        {
            sharedExp->segments.emplace_back(irExp->appliedDecl);
            return sharedExp;
        }
    };

    return Accept(Visitor{irExp}, *e_baseSharedExp);
}

struct IrExpToMSharedExpTranslator
{
    using ResultType = expected<MSharedExp*, DiagPtr>;
    SmTranslationContexts& contexts;

    ResultType Visit(IrExp* irExp)
    {
        return Error<Error_SharedTranslation_CantTranslateToMSharedExp>();
    }

    // ResultType Visit(IrExp_Namespace* irExp);
    // ResultType Visit(IrExp_Class* irExp);
    // ResultType Visit(IrExp_Struct* irExp);
    ResultType Visit(IrExp_Static* irExp)
    {
        return contexts.mFactory->MakeMSharedExp<MSharedExp_Static>(irExp->loc);
    }

    ResultType Visit(IrExp_ClassVar* irExp)
    {
        return contexts.mFactory->MakeMSharedExp<MSharedExp_ClassVar>(irExp->base, irExp->appliedDecl);
    }

    ResultType Visit(IrExp_SharedStructVar* irExp)
    {
        return contexts.mFactory->MakeMSharedExp<MSharedExp_SharedStructVar>(irExp->base, irExp->appliedDecl);
    }

    ResultType Visit(IrExp_StructVar* irExp)
    {
        return TranslateIrExp_StructVarToMSharedExp(irExp, contexts);
    }

    // ResultType Visit(IrExp_SharedDeref* irExp);
    // ResultType Visit(IrExp_Loc* irExp);
};

expected<MSharedExp*, DiagPtr> TranslateIrExpToMSharedExp(IrExp* irExp, SmTranslationContexts& contexts)
{   
    return Accept(IrExpToMSharedExpTranslator{contexts}, irExp);
}

} // namespace 