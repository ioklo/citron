#include "pch.h"
#include "ReExpToRExpTranslation.h"

#include <Infra/Ptr.h>
#include <Logging/Logger.h>
#include <IR0/RExp.h>

#include "ReExp.h"
#include "ScopeContext.h"
#include "ReExpToRLocTranslation.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

    // 기본적으로 load를 한다
class ReExpToRExpTranslator : public ReExpVisitor
{
    ScopeContextPtr context;
    LoggerPtr logger;
    RTypeFactory& factory;
    RExpPtr* result;

public:
    ReExpToRExpTranslator(const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory, RExpPtr* result)
        : context(context), logger(logger), factory(factory), result(result)
    {
    }

    void HandleLoc(RLocPtr&& loc)
    {
        if (!loc)
            *result = nullptr;
        else
            *result = MakePtr<RLoadExp>(std::move(loc));
    }

    void Visit(ReThisVarExp& exp) override
    {
        auto rLoc = TranslateReThisVarExpToRLoc(exp);
        HandleLoc(std::move(rLoc));
    }

    void Visit(ReLocalVarExp& exp) override
    {
        auto rLoc = TranslateReLocalVarExpToRLoc(exp);
        HandleLoc(std::move(rLoc));
    }

    void Visit(ReLambdaMemberVarExp& exp) override
    {
        auto rLoc = TranslateReLambdaMemberVarExpToRLoc(exp);
        HandleLoc(std::move(rLoc));
    }

    void Visit(ReClassMemberVarExp& exp) override
    {
        auto rLoc = TranslateReClassMemberVarExpToRLoc(exp, context, logger, factory);
        HandleLoc(std::move(rLoc));
    }

    void Visit(ReStructMemberVarExp& exp) override
    {
        auto rLoc = TranslateReStructMemberVarExpToRLoc(exp, context, logger, factory);
        HandleLoc(std::move(rLoc));
    }

    void Visit(ReEnumElemMemberVarExp& exp) override
    {
        auto rLoc = TranslateReEnumElemMemberVarExpToRLoc(exp, context, logger, factory);
        HandleLoc(std::move(rLoc));
    }

    void Visit(ReLocalDerefExp& exp) override
    {
        auto rLoc = TranslateReLocalDerefExpToRLoc(exp, context, logger, factory);
        HandleLoc(std::move(rLoc));
    }

    // *x
    void Visit(ReBoxDerefExp& exp) override 
    {
        auto rLoc = TranslateReBoxDerefExpToRLoc(exp, context, logger, factory);
        HandleLoc(std::move(rLoc));
    }

    void Visit(ReListIndexerExp& exp) override
    {
        auto rLoc = TranslateReListIndexerExpToRLoc(exp, context, logger, factory);
        HandleLoc(std::move(rLoc));
    }

    void Visit(ReElseExp& exp) override
    {   
        *result = exp.rExp;
    }
};

RExpPtr TranslateReExpToRExp(ReExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    RExpPtr rExp;
    ReExpToRExpTranslator translator(context, logger, factory, &rExp);
    reExp.Accept(translator);

    return rExp;
}

}
