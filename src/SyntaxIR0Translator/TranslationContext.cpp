#include "pch.h"

#include <Infra/Ptr.h>
#include "TranslationContext.h"
#include "ScopeContext.h"

namespace Citron::SyntaxIR0Translator {

TranslationContext TranslationContext::MakeNestedScopeContext()
{
    auto newScopeContext = MakePtr<ScopeContext>(scopeContext, scopeContext->nestedLoop);
    return { globalContext, bodyContext, newScopeContext, logger, factory, binOpQueryService };
}

TranslationContext TranslationContext::MakeNestedLoopScopeContext()
{
    auto newScopeContext = MakePtr<ScopeContext>(scopeContext, scopeContext->nestedLoop + 1);
    return { globalContext, bodyContext, newScopeContext, logger, factory, binOpQueryService };
}

TranslationContext TranslationContext::MakeLambdaBodyContext(RFuncReturn&& funcRet, std::vector<RFuncParameter> funcParams, bool bLastParamVariadic)
{   
    auto newBodyContext = MakePtr<BodyContext>(scopeContext, funcRet, funcParams);
    auto newScopeContext = MakePtr<ScopeContext>(nullptr, 0);

    return { globalContext, newBodyContext, newScopeContext, logger, factory, binOpQueryService };
}

}

} // namespace Citron::SyntaxIR0Translator