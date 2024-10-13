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

namespace {

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
            *result = MakePtr<RExp_Load>(std::move(loc));
    }

    void Visit(ReExp_ThisVar& exp) override
    {
        auto rLoc = TranslateReThisVarExpToRLoc(exp, *context, factory);
        HandleLoc(std::move(rLoc));
    }

    void Visit(ReExp_LocalVar& exp) override
    {
        auto rLoc = TranslateReLocalVarExpToRLoc(exp);
        HandleLoc(std::move(rLoc));
    }

    void Visit(ReExp_LambdaMemberVar& exp) override
    {
        auto rLoc = TranslateReLambdaMemberVarExpToRLoc(exp);
        HandleLoc(std::move(rLoc));
    }

    void Visit(ReExp_ClassMemberVar& exp) override
    {
        auto rLoc = TranslateReClassMemberVarExpToRLoc(exp, context, logger, factory);
        HandleLoc(std::move(rLoc));
    }

    void Visit(ReExp_StructMemberVar& exp) override
    {
        auto rLoc = TranslateReStructMemberVarExpToRLoc(exp, context, logger, factory);
        HandleLoc(std::move(rLoc));
    }

    void Visit(ReExp_EnumElemMemberVar& exp) override
    {
        auto rLoc = TranslateReEnumElemMemberVarExpToRLoc(exp, context, logger, factory);
        HandleLoc(std::move(rLoc));
    }

    void Visit(ReExp_LocalDeref& exp) override
    {
        auto rLoc = TranslateReLocalDerefExpToRLoc(exp, context, logger, factory);
        HandleLoc(std::move(rLoc));
    }

    // *x
    void Visit(ReExp_BoxDeref& exp) override
    {
        auto rLoc = TranslateReBoxDerefExpToRLoc(exp, context, logger, factory);
        HandleLoc(std::move(rLoc));
    }

    void Visit(ReExp_ListIndexer& exp) override
    {
        auto rLoc = TranslateReListIndexerExpToRLoc(exp, context, logger, factory);
        HandleLoc(std::move(rLoc));
    }

    void Visit(ReExp_Else& exp) override
    {
        *result = exp.rExp;
    }
};

} // namespace 

RExpPtr TranslateReExpToRExp(ReExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    RExpPtr rExp;
    ReExpToRExpTranslator translator(context, logger, factory, &rExp);
    reExp.Accept(translator);

    return rExp;
}

}
