#include "ReExpToMExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Logging/Logger.h"
#include "Logging/Diag.h"
#include "MIR/MExp.h"
#include "MIR/MFactory.h"

#include "ReExp.h"
#include "ScopeContext.h"
#include "ReExpToMLocTranslation.h"
#include "TranslationContexts.h"

using namespace std;

namespace Citron {

namespace {

// 기본적으로 load를 한다
class ReExpToMExpTranslator
{
public:
    using ResultType = expected<MExp*, DiagPtr>;

private:
    TranslationContexts& contexts;

public:
    ReExpToMExpTranslator(TranslationContexts& contexts)
        : contexts{contexts}
    {
    }

    ResultType HandleLoc(expected<MLoc*, DiagPtr>&& eLoc)
    {
        if (!eLoc)
            return unexpected{move(eLoc).error()};
        else
            return contexts.mFactory->MakeMExp<MExp_Load>(*eLoc);
    }

    ResultType Visit(ReExp_ThisVar* exp)
    {
        auto e_nLoc = TranslateReThisVarExpToMLoc(exp, contexts);
        return HandleLoc(move(e_nLoc));
    }

    ResultType Visit(ReExp_LocalVar* exp)
    {
        auto e_nLoc = TranslateReLocalVarExpToMLoc(exp, contexts);
        return HandleLoc(move(e_nLoc));
    }

    ResultType Visit(ReExp_LambdaVar* exp)
    {
        auto e_nLoc = TranslateReLambdaVarExpToMLoc(exp, contexts);
        return HandleLoc(move(e_nLoc));
    }

    ResultType Visit(ReExp_ClassVar* exp)
    {
        auto e_nLoc = TranslateReClassVarExpToMLoc(exp, contexts);
        return HandleLoc(move(e_nLoc));
    }

    ResultType Visit(ReExp_StructVar* exp)
    {
        auto e_nLoc = TranslateReStructVarExpToMLoc(exp, contexts);
        return HandleLoc(move(e_nLoc));
    }

    ResultType Visit(ReExp_EnumElemVar* exp)
    {
        auto e_nLoc = TranslateReEnumElemVarExpToMLoc(exp, contexts);
        return HandleLoc(move(e_nLoc));
    }

    ResultType Visit(ReExp_Deref* exp)
    {
        auto e_nLoc = TranslateReDerefExpToMLoc(exp, contexts);
        return HandleLoc(move(e_nLoc));
    }

    // *x
    ResultType Visit(ReExp_BoxDeref* exp)
    {
        auto e_nLoc = TranslateReBoxDerefExpToMLoc(exp, contexts);
        return HandleLoc(move(e_nLoc));
    }

    ResultType Visit(ReExp_ListIndexer* exp)
    {
        auto e_nLoc = TranslateReListIndexerExpToMLoc(exp, contexts);
        return HandleLoc(move(e_nLoc));
    }

    ResultType Visit(ReExp_Else* exp)
    {
        return exp->mExp;
    }
};

} // namespace 

expected<MExp*, DiagPtr> TranslateReExpToMExp(ReExp* reExp, TranslationContexts& contexts)
{
    ReExpToMExpTranslator translator{contexts};
    return Accept(translator, reExp);
}

}
