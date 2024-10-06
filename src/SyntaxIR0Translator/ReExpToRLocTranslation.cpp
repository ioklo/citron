#include "pch.h"
#include "ReExpToRLocTranslation.h"

#include <Infra/Ptr.h>
#include <Logging/Logger.h>
#include <IR0/RLoc.h>
#include <IR0/RClassMemberVarDecl.h>
#include <IR0/RStructMemberVarDecl.h>
#include <IR0/RTypeFactory.h>

#include "ScopeContext.h"
#include "NotLocationErrorLogger.h"
#include "ReExp.h"

namespace Citron::SyntaxIR0Translator {

RLocPtr TranslateReThisVarExpToRLoc(ReThisVarExp& reExp) // nothrow
{
    return MakePtr<RThisLoc>(reExp.type);
}

RLocPtr TranslateReClassMemberVarExpToRLoc(ReClassMemberVarExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    if (reExp.hasExplicitInstance) // c.x, C.x 둘다 해당
    {
        RLocPtr instance = nullptr;
        
        if (reExp.explicitInstance != nullptr)
        {
            ExpressionIsNotLocationErrorLogger notLocationErrorLogger(logger);

            instance = TranslateReExpToRLoc(*reExp.explicitInstance, /* bWrapExpAsLoc */ true, &notLocationErrorLogger, context, logger, factory);
            if (!instance) return nullptr;
        }

        return MakePtr<RClassMemberLoc>(std::move(instance), reExp.decl, reExp.typeArgs);
    }
    else // x, x (static) 둘다 해당
    {
        auto type = reExp.decl->GetClassType(reExp.typeArgs, factory);
        RLocPtr rInstanceLoc = reExp.decl->bStatic ? nullptr : MakePtr<RThisLoc>(std::move(type));
        return MakePtr<RClassMemberLoc>(std::move(rInstanceLoc), reExp.decl, reExp.typeArgs);
    }
}

RLocPtr TranslateReLocalVarExpToRLoc(ReLocalVarExp& reExp)
{
    return MakePtr<RLocalVarLoc>(reExp.name, reExp.type);
}

RLocPtr TranslateReLambdaMemberVarExpToRLoc(ReLambdaMemberVarExp& reExp)
{
    return MakePtr<RLambdaMemberVarLoc>(reExp.decl, reExp.typeArgs);
}

RLocPtr TranslateReStructMemberVarExpToRLoc(ReStructMemberVarExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    if (reExp.hasExplicitInstance) // c.x, C.x 둘다 해당
    {
        RLocPtr instance = nullptr;

        if (reExp.explicitInstance != nullptr)
        {
            ExpressionIsNotLocationErrorLogger notLocationErrorLogger(logger);

            instance = TranslateReExpToRLoc(*reExp.explicitInstance, /*bWrapExpAsLoc*/ true, &notLocationErrorLogger, context, logger, factory);
            if (!instance)
                return nullptr;
        }

        return MakePtr<RStructMemberLoc>(instance, reExp.decl, reExp.typeArgs);
    }
    else // x, x (static) 둘다 해당
    {   
        auto type = reExp.decl->GetStructType(reExp.typeArgs, factory);

        // TODO: [10] box 함수 내부이면, local ptr대신 box ptr로 변경해야 한다
        RLocPtr rInstanceLoc = reExp.decl->bStatic ? nullptr : MakePtr<RLocalDerefLoc>(MakePtr<RThisLoc>(std::move(type)));
        return MakePtr<RStructMemberLoc>(rInstanceLoc, reExp.decl, reExp.typeArgs);
    }
}

RLocPtr TranslateReEnumElemMemberVarExpToRLoc(ReEnumElemMemberVarExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    ExpressionIsNotLocationErrorLogger notLocationErrorLogger(logger);

    auto rInstLoc = TranslateReExpToRLoc(*reExp.instance, /*bWrapExpAsLoc*/ true, &notLocationErrorLogger, context, logger, factory);
    if (!rInstLoc) return nullptr;

    return MakePtr<REnumElemMemberLoc>(rInstLoc, reExp.decl, reExp.typeArgs);
}

RLocPtr TranslateReListIndexerExpToRLoc(ReListIndexerExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    ExpressionIsNotLocationErrorLogger notLocationErrorLogger(logger);

    auto rInstLoc = TranslateReExpToRLoc(*reExp.instance, /*bWrapExpAsLoc*/ true, &notLocationErrorLogger, context, logger, factory);
    if (!rInstLoc) return nullptr;

    return MakePtr<RListIndexerLoc>(std::move(rInstLoc), reExp.index, reExp.itemType);
}

RLocPtr TranslateReLocalDerefExpToRLoc(ReLocalDerefExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    // *x, *G()
    ExpressionIsNotLocationErrorLogger notLocationErrorLogger(logger);

    auto rTargetLoc = TranslateReExpToRLoc(*reExp.target, /*bWrapExpAsLoc*/ true, &notLocationErrorLogger, context, logger, factory);
    if (!rTargetLoc) return nullptr;

    return MakePtr<RLocalDerefLoc>(std::move(rTargetLoc));
}

RLocPtr TranslateReBoxDerefExpToRLoc(ReBoxDerefExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    // *x, *G()
    ExpressionIsNotLocationErrorLogger notLocationErrorLogger(logger);

    auto rTargetLoc = TranslateReExpToRLoc(*reExp.target, /*bWrapExpAsLoc*/ true, &notLocationErrorLogger, context, logger, factory);
    if (!rTargetLoc) return nullptr;

    return MakePtr<RBoxDerefLoc>(std::move(rTargetLoc));
}

class ReExpToRLocTranslator : public ReExpVisitor
{
    ScopeContextPtr context;
    bool bWrapExpAsLoc;
    LoggerPtr logger;
    INotLocationErrorLogger* notLocationErrorLogger;
    RLocPtr* result;
    RTypeFactory& factory;

public:
    ReExpToRLocTranslator(const ScopeContextPtr& context, bool bWrapExpAsLoc, const LoggerPtr& logger, INotLocationErrorLogger* notLocationErrorLogger, RLocPtr* result, RTypeFactory& factory)
        : context(context), bWrapExpAsLoc(bWrapExpAsLoc), logger(logger), notLocationErrorLogger(notLocationErrorLogger), result(result), factory(factory)
    {
    }
    
    void Visit(ReThisVarExp& exp) override 
    {
        *result = TranslateReThisVarExpToRLoc(exp);
    }

    void Visit(ReLocalVarExp& exp) override 
    { 
        *result = TranslateReLocalVarExpToRLoc(exp);
    }

    void Visit(ReLambdaMemberVarExp& exp) override 
    { 
        *result = TranslateReLambdaMemberVarExpToRLoc(exp);
    }

    void Visit(ReClassMemberVarExp& exp) override 
    { 
        *result = TranslateReClassMemberVarExpToRLoc(exp, context, logger, factory);
    }

    void Visit(ReStructMemberVarExp& exp) override 
    { 
        *result = TranslateReStructMemberVarExpToRLoc(exp, context, logger, factory);
    }

    void Visit(ReEnumElemMemberVarExp& exp) override 
    { 
        *result = TranslateReEnumElemMemberVarExpToRLoc(exp, context, logger, factory);
    }

    void Visit(ReLocalDerefExp& exp) override 
    { 
        *result = TranslateReLocalDerefExpToRLoc(exp, context, logger, factory);
    }

    void Visit(ReBoxDerefExp& exp) override 
    { 
        *result = TranslateReBoxDerefExpToRLoc(exp, context, logger, factory);
    }

    void Visit(ReListIndexerExp& exp) override 
    { 
        *result = TranslateReListIndexerExpToRLoc(exp, context, logger, factory);
    }

    void Visit(ReElseExp& exp) override 
    { 
        if (bWrapExpAsLoc)
        {   
            *result = MakePtr<RTempLoc>(exp.rExp);
        }
        else
        {
            notLocationErrorLogger->Log();
            *result = nullptr;
        }
    }
};

RLocPtr TranslateReExpToRLoc(ReExp& reExp, bool bWrapExpAsLoc, INotLocationErrorLogger* notLocationErrorLogger, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    RLocPtr rLoc;
    ReExpToRLocTranslator translator(context, bWrapExpAsLoc, logger, notLocationErrorLogger, &rLoc, factory);
    reExp.Accept(translator);
    return rLoc;
}

}
