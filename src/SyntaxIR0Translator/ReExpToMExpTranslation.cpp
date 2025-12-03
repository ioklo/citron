#include "ReExpToMExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Logging/Logger.h"
#include "Logging/Diag.h"
#include "MIR/MExp.h"

#include "ReExp.h"
#include "ScopeContext.h"
#include "TranslationContext.h"
#include "ReExpToMLocTranslation.h"

using namespace std;

namespace Citron {

namespace {

// 기본적으로 load를 한다
class ReExpToMExpTranslator
{
public:
    using ResultType = expected<MExp*, DiagPtr>;

private:
    TranslationContext& context;

public:
    ReExpToMExpTranslator(TranslationContext& context)
        : context(context)
    {
    }

    ResultType HandleLoc(expected<MLoc*, DiagPtr>&& eLoc)
    {
        if (!eLoc)
            return unexpected{move(eLoc).error()};
        else
            return context.MakeMExp<MExp_Load>(*eLoc);
    }

    ResultType Visit(ReExp_ThisVar* exp)
    {
        auto eNLoc = TranslateReThisVarExpToMLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    ResultType Visit(ReExp_LocalVar* exp)
    {
        auto eNLoc = TranslateReLocalVarExpToMLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    ResultType Visit(ReExp_LambdaVar* exp)
    {
        auto eNLoc = TranslateReLambdaVarExpToMLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    ResultType Visit(ReExp_ClassVar* exp)
    {
        auto eNLoc = TranslateReClassVarExpToMLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    ResultType Visit(ReExp_StructVar* exp)
    {
        auto eNLoc = TranslateReStructVarExpToMLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    ResultType Visit(ReExp_EnumElemVar* exp)
    {
        auto eNLoc = TranslateReEnumElemVarExpToMLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    ResultType Visit(ReExp_LocalDeref* exp)
    {
        auto eNLoc = TranslateReLocalDerefExpToMLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    // *x
    ResultType Visit(ReExp_BoxDeref* exp)
    {
        auto eNLoc = TranslateReBoxDerefExpToMLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    ResultType Visit(ReExp_ListIndexer* exp)
    {
        auto eNLoc = TranslateReListIndexerExpToMLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    ResultType Visit(ReExp_Else* exp)
    {
        return exp->mExp;
    }
};

} // namespace 

expected<MExp*, DiagPtr> TranslateReExpToMExp(ReExp* reExp, TranslationContext& context)
{
    ReExpToMExpTranslator translator{context};
    return Accept(translator, reExp);
}

}
