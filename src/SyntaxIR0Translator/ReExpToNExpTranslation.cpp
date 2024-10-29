#include "pch.h"
#include "ReExpToNExpTranslation.h"

#include <Infra/Ptr.h>
#include <Logging/Logger.h>
#include <IR0/NExp.h>

#include "ReExp.h"
#include "ScopeContext.h"
#include "ReExpToNLocTranslation.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

// 기본적으로 load를 한다
class ReExpToNExpTranslator : public ReExpVisitor
{   
    NExpPtr* result;
    TranslationContext& context;

public:
    ReExpToNExpTranslator(NExpPtr* result, TranslationContext& context)
        : result(result), context(context)
    {
    }

    void HandleLoc(NLocPtr&& loc)
    {
        if (!loc)
            *result = nullptr;
        else
            *result = MakePtr<NExp_Load>(std::move(loc));
    }

    void Visit(ReExp_ThisVar& exp) override
    {
        auto nLoc = TranslateReThisVarExpToNLoc(exp, context);
        HandleLoc(std::move(nLoc));
    }

    void Visit(ReExp_LocalVar& exp) override
    {
        auto nLoc = TranslateReLocalVarExpToNLoc(exp);
        HandleLoc(std::move(nLoc));
    }

    void Visit(ReExp_LambdaMemberVar& exp) override
    {
        auto nLoc = TranslateReLambdaMemberVarExpToNLoc(exp);
        HandleLoc(std::move(nLoc));
    }

    void Visit(ReExp_ClassMemberVar& exp) override
    {
        auto nLoc = TranslateReClassMemberVarExpToNLoc(exp, context);
        HandleLoc(std::move(nLoc));
    }

    void Visit(ReExp_StructMemberVar& exp) override
    {
        auto nLoc = TranslateReStructMemberVarExpToNLoc(exp, context);
        HandleLoc(std::move(nLoc));
    }

    void Visit(ReExp_EnumElemMemberVar& exp) override
    {
        auto nLoc = TranslateReEnumElemMemberVarExpToNLoc(exp, context);
        HandleLoc(std::move(nLoc));
    }

    void Visit(ReExp_LocalDeref& exp) override
    {
        auto nLoc = TranslateReLocalDerefExpToNLoc(exp, context);
        HandleLoc(std::move(nLoc));
    }

    // *x
    void Visit(ReExp_BoxDeref& exp) override
    {
        auto nLoc = TranslateReBoxDerefExpToNLoc(exp, context);
        HandleLoc(std::move(nLoc));
    }

    void Visit(ReExp_ListIndexer& exp) override
    {
        auto nLoc = TranslateReListIndexerExpToNLoc(exp, context);
        HandleLoc(std::move(nLoc));
    }

    void Visit(ReExp_Else& exp) override
    {
        *result = exp.nExp;
    }
};

} // namespace 

NExpPtr TranslateReExpToNExp(ReExp& reExp, TranslationContext& context)
{
    NExpPtr nExp;
    ReExpToNExpTranslator translator(&nExp, context);
    reExp.Accept(translator);

    return nExp;
}

}
