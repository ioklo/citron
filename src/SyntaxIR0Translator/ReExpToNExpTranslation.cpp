#include "ReExpToNExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Logging/Logger.h"
#include "Logging/Diag.h"
#include "IR0/NExp.h"

#include "ReExp.h"
#include "ScopeContext.h"
#include "TranslationContext.h"
#include "ReExpToNLocTranslation.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

// 기본적으로 load를 한다
class ReExpToNExpTranslator
{
public:
    using ResultType = expected<NExp*, DiagPtr>;

private:
    TranslationContext& context;

public:
    ReExpToNExpTranslator(TranslationContext& context)
        : context(context)
    {
    }

    ResultType HandleLoc(expected<NLoc*, DiagPtr>&& eLoc)
    {
        if (!eLoc)
            return unexpected{move(eLoc).error()};
        else
            return context.MakeNExp<NExp_Load>(*eLoc);
    }

    ResultType Visit(ReExp_ThisVar* exp)
    {
        auto eNLoc = TranslateReThisVarExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    ResultType Visit(ReExp_LocalVar* exp)
    {
        auto eNLoc = TranslateReLocalVarExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    ResultType Visit(ReExp_LambdaVar* exp)
    {
        auto eNLoc = TranslateReLambdaVarExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    ResultType Visit(ReExp_ClassVar* exp)
    {
        auto eNLoc = TranslateReClassVarExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    ResultType Visit(ReExp_StructVar* exp)
    {
        auto eNLoc = TranslateReStructVarExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    ResultType Visit(ReExp_EnumElemVar* exp)
    {
        auto eNLoc = TranslateReEnumElemVarExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    ResultType Visit(ReExp_LocalDeref* exp)
    {
        auto eNLoc = TranslateReLocalDerefExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    // *x
    ResultType Visit(ReExp_BoxDeref* exp)
    {
        auto eNLoc = TranslateReBoxDerefExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    ResultType Visit(ReExp_ListIndexer* exp)
    {
        auto eNLoc = TranslateReListIndexerExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    ResultType Visit(ReExp_Else* exp)
    {
        return exp->nExp;
    }
};

} // namespace 

expected<NExp*, DiagPtr> TranslateReExpToNExp(ReExp* reExp, TranslationContext& context)
{
    ReExpToNExpTranslator translator{context};
    return Accept(translator, reExp);
}

}
