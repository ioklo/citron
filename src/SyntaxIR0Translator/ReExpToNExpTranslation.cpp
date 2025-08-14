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
class ReExpToNExpTranslator : public ReExpVisitor
{   
    expected<NExp*, DiagPtr>* result;
    TranslationContext& context;

public:
    ReExpToNExpTranslator(expected<NExp*, DiagPtr>* result, TranslationContext& context)
        : result(result), context(context)
    {
    }

    void HandleLoc(expected<NLoc*, DiagPtr>&& eLoc)
    {
        if (!eLoc)
            *result = unexpected{move(eLoc).error()};
        else
            *result = context.MakeNExp<NExp_Load>(*eLoc);
    }

    void Visit(ReExp_ThisVar* exp) override
    {
        auto eNLoc = TranslateReThisVarExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    void Visit(ReExp_LocalVar* exp) override
    {
        auto eNLoc = TranslateReLocalVarExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    void Visit(ReExp_LambdaVar* exp) override
    {
        auto eNLoc = TranslateReLambdaVarExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    void Visit(ReExp_ClassVar* exp) override
    {
        auto eNLoc = TranslateReClassVarExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    void Visit(ReExp_StructVar* exp) override
    {
        auto eNLoc = TranslateReStructVarExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    void Visit(ReExp_EnumElemVar* exp) override
    {
        auto eNLoc = TranslateReEnumElemVarExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    void Visit(ReExp_LocalDeref* exp) override
    {
        auto eNLoc = TranslateReLocalDerefExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    // *x
    void Visit(ReExp_BoxDeref* exp) override
    {
        auto eNLoc = TranslateReBoxDerefExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    void Visit(ReExp_ListIndexer* exp) override
    {
        auto eNLoc = TranslateReListIndexerExpToNLoc(exp, context);
        return HandleLoc(move(eNLoc));
    }

    void Visit(ReExp_Else* exp) override
    {
        *result = exp->nExp;
    }
};

} // namespace 

expected<NExp*, DiagPtr> TranslateReExpToNExp(ReExp* reExp, TranslationContext& context)
{
    expected<NExp*, DiagPtr> nExp;
    ReExpToNExpTranslator translator(&nExp, context);
    reExp->Accept(translator);
    return nExp;
}

}
