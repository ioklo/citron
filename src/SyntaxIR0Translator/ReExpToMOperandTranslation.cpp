#include "ReExpToMOperandTranslation.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "ReExp.h"
#include "ReExpToMLocTranslation.h"

using namespace std;

namespace Citron {

namespace {

struct ReExpToMOperandTranslator
{
    using ResultType = expected<MOperand, DiagPtr>;
    TranslationContexts& contexts;

    ResultType HandleLoc(expected<MLoc*, DiagPtr>&& e_loc)
    {
        RETURN_ON_ERROR(e_loc);
        return MOperand_Loc{*e_loc};
    }

    ResultType Visit(ReExp_ThisVar* exp) 
    {
        return HandleLoc(TranslateReThisVarExpToMLoc(exp, contexts));
    }

    ResultType Visit(ReExp_LocalVar* exp)
    {
        return HandleLoc(TranslateReLocalVarExpToMLoc(exp, contexts));
    }

    ResultType Visit(ReExp_LocalRef* exp)
    {
        return HandleLoc(TranslateReLocalRefExpToMLoc(exp, contexts));
    }

    ResultType Visit(ReExp_LambdaVar* exp)
    {
        return HandleLoc(TranslateReLambdaVarExpToMLoc(exp, contexts));
    }

    ResultType Visit(ReExp_ClassVar* exp)
    {
        return HandleLoc(TranslateReClassVarExpToMLoc(exp, contexts));
    }

    ResultType Visit(ReExp_StructVar* exp)
    {
        return HandleLoc(TranslateReStructVarExpToMLoc(exp, contexts));
    }

    ResultType Visit(ReExp_EnumElemVar* exp)
    {
        return HandleLoc(TranslateReEnumElemVarExpToMLoc(exp, contexts));
    }

    ResultType Visit(ReExp_PtrDeref* exp)
    {
        return HandleLoc(TranslateReDerefExpToMLoc(exp, contexts));
    }

    ResultType Visit(ReExp_BoxDeref* exp)
    {
        return HandleLoc(TranslateReBoxDerefExpToMLoc(exp, contexts));
    }

    ResultType Visit(ReExp_ListIndexer* exp)
    {
        return HandleLoc(TranslateReListIndexerExpToMLoc(exp, contexts));
    }

    ResultType Visit(ReExp_Else* exp)
    {
        return MOperand_Exp{exp->mExp};
    }
};

} // namespace 

expected<MOperand, DiagPtr> TranslateReExpToMOperand(ReExp* reExp, TranslationContexts& contexts)
{
    ReExpToMOperandTranslator translator{contexts};
    return Accept(translator, reExp);
}

} // namespace Citron