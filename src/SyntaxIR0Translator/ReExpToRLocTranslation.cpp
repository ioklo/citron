#include "pch.h"
#include "ReExpToRLocTranslation.h"

#include <Infra/Ptr.h>
#include <Logging/Logger.h>
#include <IR0/RLoc.h>
#include <IR0/RClassMemberVarDecl.h>
#include <IR0/RStructMemberVarDecl.h>
#include <IR0/RTypeFactory.h>

#include "ScopeContext.h"
#include "DesignatedErrorLogger.h"
#include "ReExp.h"

namespace Citron::SyntaxIR0Translator {

RLocPtr TranslateReThisVarExpToRLoc(ReExp_ThisVar& reExp, ScopeContext& context, RTypeFactory& factory) // nothrow
{
    return context.MakeThisLoc(factory);
}

RLocPtr TranslateReClassMemberVarExpToRLoc(ReExp_ClassMemberVar& reExp, ScopeContext& context, Logger& logger, RTypeFactory& factory)
{
    if (reExp.hasExplicitInstance) // c.x, C.x 둘다 해당
    {
        RLocPtr instance = nullptr;
        
        if (reExp.explicitInstance != nullptr)
        {   
            DesignatedErrorLogger designatedErrorLogger(logger, &Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

            instance = TranslateReExpToRLoc(*reExp.explicitInstance, /* bWrapExpAsLoc */ true, &designatedErrorLogger, context, logger, factory);
            if (!instance) return nullptr;
        }

        return MakePtr<RLoc_ClassMember>(std::move(instance), reExp.decl, reExp.typeArgs);
    }
    else // x, x (static) 둘다 해당
    {   
        RLocPtr rInstanceLoc = reExp.decl->bStatic ? nullptr : context.MakeThisLoc(factory);
        return MakePtr<RLoc_ClassMember>(std::move(rInstanceLoc), reExp.decl, reExp.typeArgs);
    }
}

RLocPtr TranslateReLocalVarExpToRLoc(ReExp_LocalVar& reExp)
{
    return MakePtr<RLoc_LocalVar>(reExp.name, reExp.type);
}

RLocPtr TranslateReLambdaMemberVarExpToRLoc(ReExp_LambdaMemberVar& reExp)
{
    return MakePtr<RLoc_LambdaMemberVar>(reExp.decl, reExp.typeArgs);
}

RLocPtr TranslateReStructMemberVarExpToRLoc(ReExp_StructMemberVar& reExp, ScopeContext& context, Logger& logger, RTypeFactory& factory)
{
    if (reExp.hasExplicitInstance) // c.x, C.x 둘다 해당
    {
        RLocPtr instance = nullptr;

        if (reExp.explicitInstance != nullptr)
        {
            DesignatedErrorLogger designatedErrorLogger(logger, &Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

            instance = TranslateReExpToRLoc(*reExp.explicitInstance, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context, logger, factory);
            if (!instance)
                return nullptr;
        }

        return MakePtr<RLoc_StructMember>(instance, reExp.decl, reExp.typeArgs);
    }
    else // x, x (static) 둘다 해당
    {   
        // TODO: [10] box 함수 내부이면, local ptr대신 box ptr로 변경해야 한다
        RLocPtr rInstanceLoc = reExp.decl->bStatic ? nullptr : MakePtr<RLoc_LocalDeref>(context.MakeThisLoc(factory));
        return MakePtr<RLoc_StructMember>(rInstanceLoc, reExp.decl, reExp.typeArgs);
    }
}

RLocPtr TranslateReEnumElemMemberVarExpToRLoc(ReExp_EnumElemMemberVar& reExp, ScopeContext& context, Logger& logger, RTypeFactory& factory)
{   
    DesignatedErrorLogger designatedErrorLogger(logger, &Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

    auto rInstLoc = TranslateReExpToRLoc(*reExp.instance, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context, logger, factory);
    if (!rInstLoc) return nullptr;

    return MakePtr<RLoc_EnumElemMember>(rInstLoc, reExp.decl, reExp.typeArgs);
}

RLocPtr TranslateReListIndexerExpToRLoc(ReExp_ListIndexer& reExp, ScopeContext& context, Logger& logger, RTypeFactory& factory)
{
    DesignatedErrorLogger designatedErrorLogger(logger, &Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

    auto rInstLoc = TranslateReExpToRLoc(*reExp.instance, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context, logger, factory);
    if (!rInstLoc) return nullptr;

    return MakePtr<RLoc_ListIndexer>(std::move(rInstLoc), reExp.index, reExp.itemType);
}

RLocPtr TranslateReLocalDerefExpToRLoc(ReExp_LocalDeref& reExp, ScopeContext& context, Logger& logger, RTypeFactory& factory)
{
    // *x, *G()
    DesignatedErrorLogger designatedErrorLogger(logger, &Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

    auto rTargetLoc = TranslateReExpToRLoc(*reExp.target, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context, logger, factory);
    if (!rTargetLoc) return nullptr;

    return MakePtr<RLoc_LocalDeref>(std::move(rTargetLoc));
}

RLocPtr TranslateReBoxDerefExpToRLoc(ReExp_BoxDeref& reExp, ScopeContext& context, Logger& logger, RTypeFactory& factory)
{
    // *x, *G()
    DesignatedErrorLogger designatedErrorLogger(logger, &Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

    auto rTargetLoc = TranslateReExpToRLoc(*reExp.target, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context, logger, factory);
    if (!rTargetLoc) return nullptr;

    return MakePtr<RLoc_BoxDeref>(std::move(rTargetLoc));
}

namespace {

class ReExpToRLocTranslator : public ReExpVisitor
{
    bool bWrapExpAsLoc;
    IDesignatedErrorLogger* notLocationErrorLogger;
    RLocPtr* result;

    ScopeContext& context;
    Logger& logger;
    RTypeFactory& factory;

public:
    ReExpToRLocTranslator(bool bWrapExpAsLoc, IDesignatedErrorLogger* notLocationErrorLogger, RLocPtr* result, ScopeContext& context, Logger& logger, RTypeFactory& factory)
        : bWrapExpAsLoc(bWrapExpAsLoc), notLocationErrorLogger(notLocationErrorLogger), result(result), context(context), logger(logger), factory(factory)
    {
    }

    void Visit(ReExp_ThisVar& exp) override
    {
        *result = TranslateReThisVarExpToRLoc(exp, context, factory);
    }

    void Visit(ReExp_LocalVar& exp) override
    {
        *result = TranslateReLocalVarExpToRLoc(exp);
    }

    void Visit(ReExp_LambdaMemberVar& exp) override
    {
        *result = TranslateReLambdaMemberVarExpToRLoc(exp);
    }

    void Visit(ReExp_ClassMemberVar& exp) override
    {
        *result = TranslateReClassMemberVarExpToRLoc(exp, context, logger, factory);
    }

    void Visit(ReExp_StructMemberVar& exp) override
    {
        *result = TranslateReStructMemberVarExpToRLoc(exp, context, logger, factory);
    }

    void Visit(ReExp_EnumElemMemberVar& exp) override
    {
        *result = TranslateReEnumElemMemberVarExpToRLoc(exp, context, logger, factory);
    }

    void Visit(ReExp_LocalDeref& exp) override
    {
        *result = TranslateReLocalDerefExpToRLoc(exp, context, logger, factory);
    }

    void Visit(ReExp_BoxDeref& exp) override
    {
        *result = TranslateReBoxDerefExpToRLoc(exp, context, logger, factory);
    }

    void Visit(ReExp_ListIndexer& exp) override
    {
        *result = TranslateReListIndexerExpToRLoc(exp, context, logger, factory);
    }

    void Visit(ReExp_Else& exp) override
    {
        if (bWrapExpAsLoc)
        {
            *result = MakePtr<RLoc_Temp>(exp.rExp);
        }
        else
        {
            notLocationErrorlogger->Log();
            *result = nullptr;
        }
    }
};

}

RLocPtr TranslateReExpToRLoc(ReExp& reExp, bool bWrapExpAsLoc, IDesignatedErrorLogger* notLocationErrorLogger, ScopeContext& context, Logger& logger, RTypeFactory& factory)
{
    RLocPtr rLoc;
    ReExpToRLocTranslator translator(bWrapExpAsLoc, notLocationErrorLogger, &rLoc, context, logger, factory);
    reExp.Accept(translator);
    return rLoc;
}

}
