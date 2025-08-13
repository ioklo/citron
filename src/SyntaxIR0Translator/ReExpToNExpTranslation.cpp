#include "ReExpToNExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Logging/Logger.h"
#include "Logging/Diag.h"
#include "IR0/NExp.h"

#include "ReExp.h"
#include "ScopeContext.h"
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
            *result = MakePtr<NExp_Load>(move(*eLoc));
    }

    void Visit(ReExp_ThisVar& exp) override
    {
        auto nLoc = TranslateReThisVarExpToNLoc(exp, context);
        return HandleLoc(move(nLoc));
    }

    void Visit(ReExp_LocalVar& exp) override
    {
        auto nLoc = TranslateReLocalVarExpToNLoc(exp);
        return HandleLoc(move(nLoc));
    }

    void Visit(ReExp_LambdaVar& exp) override
    {
        auto nLoc = TranslateReLambdaVarExpToNLoc(exp);
        return HandleLoc(move(nLoc));
    }

    void Visit(ReExp_ClassVar& exp) override
    {
        auto nLoc = TranslateReClassVarExpToNLoc(exp, context);
        return HandleLoc(move(nLoc));
    }

    void Visit(ReExp_StructVar& exp) override
    {
        auto nLoc = TranslateReStructVarExpToNLoc(exp, context);
        return HandleLoc(move(nLoc));
    }

    void Visit(ReExp_EnumElemVar& exp) override
    {
        auto nLoc = TranslateReEnumElemVarExpToNLoc(exp, context);
        return HandleLoc(move(nLoc));
    }

    void Visit(ReExp_LocalDeref& exp) override
    {
        auto nLoc = TranslateReLocalDerefExpToNLoc(exp, context);
        return HandleLoc(move(nLoc));
    }

    // *x
    void Visit(ReExp_BoxDeref& exp) override
    {
        auto nLoc = TranslateReBoxDerefExpToNLoc(exp, context);
        return HandleLoc(move(nLoc));
    }

    void Visit(ReExp_ListIndexer& exp) override
    {
        auto nLoc = TranslateReListIndexerExpToNLoc(exp, context);
        return HandleLoc(move(nLoc));
    }

    void Visit(ReExp_Else& exp) override
    {
        *result = exp.nExp;
    }
};

} // namespace 

expected<NExp*, DiagPtr> TranslateReExpToNExp(ReExp& reExp, TranslationContext& context)
{
    expected<NExp*, DiagPtr> nExp;
    ReExpToNExpTranslator translator(&nExp, context);
    reExp.Accept(translator);
    return nExp;
}

}
