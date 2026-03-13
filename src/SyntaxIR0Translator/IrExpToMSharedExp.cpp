#include "IrExpToMSharedExp.h"

#include "Infra/Expected.h"
#include "MIR/MSharedExp.h"
#include "MIR/MFactory.h"

#include "TranslationContexts.h"
#include "IrExp.h"
#include "Misc.h"

using namespace std;

namespace Citron {

MSharedExp* TranslateIrExp_ClassVarToMSharedExp(IrExp_ClassVar* irExp, TranslationContexts& contexts)
{
    return contexts.mFactory->MakeMSharedExp<MSharedExp_ClassVar>(irExp->base, irExp->decl, irExp->typeArgs, contexts.rFactory);
}

MSharedExp* TranslateIrExp_SharedStructVarToMSharedExp(IrExp_SharedStructVar* irExp, TranslationContexts& contexts)
{
    return contexts.mFactory->MakeMSharedExp<MSharedExp_SharedStructVar>(irExp->base, irExp->decl, irExp->typeArgs, contexts.rFactory);
}

expected<MSharedExp*, DiagPtr> TranslateIrExp_StructVarToMSharedExp(IrExp_StructVar* irExp, TranslationContexts& contexts)
{
    auto e_baseSharedExp = TranslateIrExpToMSharedExp(irExp->base, contexts);
    RETURN_ON_ERROR(e_baseSharedExp);

    return contexts.mFactory->MakeMSharedExp<MSharedExp_StructVar>(*e_baseSharedExp, irExp->decl, irExp->typeArgs, contexts.rFactory);
}

struct IrExpToMSharedExpTranslator
{
    using ResultType = expected<MSharedExp*, DiagPtr>;
    TranslationContexts& contexts;

    ResultType Visit(IrExp* irExp)
    {
        return Error<Error_SharedTranslation_CantTranslateToMSharedExp>();
    }

    // ResultType Visit(IrExp_Namespace* irExp);
    // ResultType Visit(IrExp_Class* irExp);
    // ResultType Visit(IrExp_Struct* irExp);
    ResultType Visit(IrExp_Static* irExp)
    {
        return contexts.mFactory->MakeMSharedExp<MSharedExp_Static>(irExp->loc, contexts.rFactory);
    }

    ResultType Visit(IrExp_ClassVar* irExp)
    {
        return TranslateIrExp_ClassVarToMSharedExp(irExp, contexts);
    }

    ResultType Visit(IrExp_SharedStructVar* irExp)
    {
        return TranslateIrExp_SharedStructVarToMSharedExp(irExp, contexts);
    }

    ResultType Visit(IrExp_StructVar* irExp)
    {
        return TranslateIrExp_StructVarToMSharedExp(irExp, contexts);
    }

    // ResultType Visit(IrExp_SharedDeref* irExp);
    ResultType Visit(IrExp_Exp* irExp) { throw NotImplementedException{}; }
    ResultType Visit(IrExp_Loc* irExp) { throw NotImplementedException{}; }
};

expected<MSharedExp*, DiagPtr> TranslateIrExpToMSharedExp(IrExp* irExp, TranslationContexts& contexts)
{   
    return Accept(IrExpToMSharedExpTranslator{contexts}, irExp);
}

} // namespace 