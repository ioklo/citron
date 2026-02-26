#include "TranslateIrExpToMSharedExp.h"
#include "Logging/Diag.h"
#include "MIR/MFactory.h"
#include "MIR/MSharedExp.h"
#include "MIR/MExp.h"
#include "IrExp.h"
#include "TranslationContexts.h"

using namespace std;

namespace Citron {

namespace {

struct IrExpToMSharedExpTranslator
{
    using ResultType = expected<MSharedExp*, DiagPtr>;
    TranslationContexts& contexts;
    IrExpToMSharedExpTranslator(TranslationContexts& contexts)
        : contexts{contexts} { }

private:
    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    ResultType Error(TArgs&&... args)
    {
        return unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

public:
    // &NS, 최종 형태가 Namespace면 에러
    ResultType Visit(IrExp_Namespace* irExp)
    {
        return Error<Error_Reference_CantMakeReference>();
    }

    // &T
    ResultType Visit(IrExp_TypeVar* irExp)
    {
        return Error<Error_Reference_CantMakeReference>();
    }

    // &C
    ResultType Visit(IrExp_Class* irExp)
    {
        return Error<Error_Reference_CantMakeReference>();
    }

    // &S
    ResultType Visit(IrExp_Struct* irExp)
    {
        return Error<Error_Reference_CantMakeReference>();
    }

    // &c.x
    ResultType Visit(IrExp_SharedRef* irExp)
    {
        return irExp->sharedExp;
    }

    // 가장 쉬운 &s.x
    ResultType Visit(IrExp_PtrRef* irExp)
    {
        return contexts.mFactory->MakeMExp<MExp_PtrRef>(irExp->loc, contexts.rFactory);
    }

    // box S* pS = ...
    // &(*pS)
    ResultType Visit(IrExp_SharedDeref* irExp)
    {
        return Error<Error_Reference_UselessDereferenceReferencedValue>();
    }

    // &G()
    ResultType Visit(IrExp_LocalValue* irExp)
    {
        return Error<Error_Reference_CantReferenceTempValue>();
    }
};

} // namespace

expected<MSharedExp*, DiagPtr> TranslateIrExpToMSharedExp(IrExp* irExp, TranslationContexts& contexts)
{
    IrExpToMSharedExpTranslator translator{contexts};
    return Accept(translator, irExp);
}

} // namespace Citron