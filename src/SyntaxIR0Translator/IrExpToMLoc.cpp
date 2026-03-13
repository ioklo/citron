#include "IrExpToMLoc.h"

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Logging/Diag.h"
#include "MIR/MFactory.h"
#include "MIR/MLoc.h"

#include "IrExp.h"
#include "Misc.h"
#include "TranslationContexts.h"

using namespace std;

namespace Citron {

MLoc* TranslateIrExp_ClassVarToMLoc(IrExp_ClassVar* irExp, TranslationContexts& contexts)
{
    return contexts.mFactory->MakeMLoc<MLoc_ClassVar>(irExp->base, irExp->decl, irExp->typeArgs);
}

MLoc* TranslateIrExp_SharedStructVarToMLoc(IrExp_SharedStructVar* irExp, TranslationContexts& contexts)
{
    auto* mDerefLoc = contexts.mFactory->MakeMLoc<MLoc_SharedDeref>(irExp->base);
    return contexts.mFactory->MakeMLoc<MLoc_StructVar>(mDerefLoc, irExp->decl, irExp->typeArgs);
}

expected<MLoc*, DiagPtr> TranslateIrExp_StructVarToMLoc(IrExp_StructVar* irExp, TranslationContexts& contexts)
{
    auto e_baseLoc = TranslateIrExpToMLoc(irExp->base, contexts);
    RETURN_ON_ERROR(e_baseLoc);

    return contexts.mFactory->MakeMLoc<MLoc_StructVar>(*e_baseLoc, irExp->decl, irExp->typeArgs);
}

struct IrExpToMLocTranslator
{
    using ResultType = expected<MLoc*, DiagPtr>;
    TranslationContexts& contexts;

    // 기본
    ResultType Visit(IrExp* irExp)
    {
        return Error<Error_SharedTranslation_CantTranslateToLoc>();
    }

    // ResultType Visit(IrExp_Namespace* irExp);
    // ResultType Visit(IrExp_Class* irExp);
    // ResultType Visit(IrExp_Struct* irExp);

    ResultType Visit(IrExp_Static* irExp)
    {
        return irExp->loc;
    }

    ResultType Visit(IrExp_ClassVar* irExp)
    {
        return TranslateIrExp_ClassVarToMLoc(irExp, contexts);
    }

    ResultType Visit(IrExp_SharedStructVar* irExp)
    {
        return TranslateIrExp_SharedStructVarToMLoc(irExp, contexts);
    }

    ResultType Visit(IrExp_StructVar* irExp)
    {
        return TranslateIrExp_StructVarToMLoc(irExp, contexts);
    }

    ResultType Visit(IrExp_SharedDeref* irExp) 
    { 
        return contexts.mFactory->MakeMLoc<MLoc_SharedDeref>(irExp->innerLoc);
    }

    ResultType Visit(IrExp_Exp* irExp) { throw NotImplementedException{}; }
    ResultType Visit(IrExp_Loc* irExp) { throw NotImplementedException{}; }
};

expected<MLoc*, DiagPtr> TranslateIrExpToMLoc(IrExp* irExp, TranslationContexts& contexts)
{
    return Accept(IrExpToMLocTranslator{contexts}, irExp);
}

} // namespace 