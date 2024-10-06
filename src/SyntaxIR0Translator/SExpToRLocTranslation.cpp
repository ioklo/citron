#include "pch.h"
#include "SExpToRLocTranslation.h"

#include <Infra/Ptr.h>
#include <Logging/Logger.h>
#include <Syntax/Syntax.h>
#include <IR0/RLoc.h>

#include "NotLocationErrorLogger.h"
#include "ReExpToRLocTranslation.h"
#include "SExpToReExpTranslation.h"

namespace Citron::SyntaxIR0Translator {

class SExpToRLocTranslator : public SExpVisitor
{
    ScopeContextPtr context;
    RTypePtr hintType;
    bool bWrapExpAsLoc;
    LoggerPtr logger;
    INotLocationErrorLogger* notLocationErrorLogger;
    RLocPtr* result;

public:
    SExpToRLocTranslator(const ScopeContextPtr& context, const RTypePtr& hintType, bool bWrapExpAsLoc, const LoggerPtr& logger, INotLocationErrorLogger* notLocationErrorLogger, RLocPtr* result)
        : context(context), hintType(hintType), bWrapExpAsLoc(bWrapExpAsLoc), notLocationErrorLogger(notLocationErrorLogger), result(result)
    {
    }

    void HandleDefault(SExp& sExp)
    {
        if (auto reExp = TranslateSExpToReExp(sExp, hintType, context))
        {
            ExpressionIsNotLocationErrorLogger notLocationErrorLogger(logger);
            *result = TranslateReExpToRLoc(*reExp, context, bWrapExpAsLoc, logger, &notLocationErrorLogger);
        }
        else // invalid
        {
            *result = nullptr;
        }
    }

    // fast track
    void HandleExp(RExpPtr&& rExp)
    {
        if (!rExp)
        {
            *result = nullptr;
        }
        else if (bWrapExpAsLoc)
        {
            *result = MakePtr<RTempLoc>(std::move(rExp));
        }
        else
        {
            notLocationErrorLogger->Log();
            *result = nullptr;
        }
    }
   
    void Visit(SIdentifierExp& exp) override 
    { 
        HandleDefault(exp);
    }

    void Visit(SStringExp& exp) override 
    { 
        auto rExp = TranslateSStringExpToRExp(exp);
        HandleExp(std::move(rExp));
    }

    void Visit(SIntLiteralExp& exp) override 
    { 
        auto rExp = TranslateSIntLiteralExpToRExp(exp);
        HandleExp(std::move(rExp));
    }

    void Visit(SBoolLiteralExp& exp) override 
    { 
        auto rExp = TranslateSBoolLiteralExpToRExp(exp);
        HandleExp(std::move(rExp));
    }

    void Visit(SNullLiteralExp& exp) override 
    { 
        auto rExp = TranslateSNullLiteralExpToRExp(exp, hintType, context);
        HandleExp(std::move(rExp));
    }

    void Visit(SBinaryOpExp& exp) override 
    {   
        RExpPtr rExp = TranslateSBinaryOpExpToRExp(exp, hintType, context);
        HandleExp(std::move(rExp));
    }

    void Visit(SUnaryOpExp& exp) override 
    { 
        // Deref는 loc으로 변경되어야 한다
        if (exp.kind ==  SUnaryOpKind::Deref)
        {
            HandleDefault(exp);
        }
        else
        {
            auto rExp = TranslateSUnaryOpExpToRExp(exp);
            HandleExp(std::move(rExp));
        }
    }

    void Visit(SCallExp& exp) override 
    { 
        RExpPtr rExp = TranslateSCallExpToRExp(exp, hintType, context);
        HandleExp(std::move(rExp));
    }

    void Visit(SLambdaExp& exp) override 
    { 
        auto rExp = TranslateSLambdaExpToRExp(exp, hintType, context);
        HandleExp(std::move(rExp));
    }

    void Visit(SIndexerExp& exp) override 
    { 
        HandleDefault(exp);
    }

    void Visit(SMemberExp& exp) override 
    { 
        HandleDefault(exp);
    }

    // s->x
    void Visit(SIndirectMemberExp& exp) override { static_assert(false); }

    void Visit(SListExp& exp) override 
    { 
        auto rExp = TranslateSListExpToRExp(exp, hintType, context);
        HandleExp(std::move(rExp));
    }

    void Visit(SNewExp& exp) override 
    { 
        auto rExp = TranslateSNewExpToRExp(exp);
        HandleExp(std::move(rExp));
    }

    void Visit(SBoxExp& exp) override 
    { 
        auto rExp = TranslateSBoxExpToRExp(exp);
        HandleExp(std::move(rExp));
    }

    void Visit(SIsExp& exp) override 
    { 
        auto rExp = TranslateSIsExpToRExp(exp);
        HandleExp(std::move(rExp));
    }

    void Visit(SAsExp& exp) override 
    {
        auto rExp = TranslateSAsExpToRExp(exp);
        HandleExp(std::move(rExp));
    }    
};

RLocPtr TranslateSExpToRLoc(SExp& sExp, const RTypePtr& hintType, bool bWrapExpAsLoc, INotLocationErrorLogger* notLocationLogger, const ScopeContextPtr& context, const LoggerPtr& logger)
{
    RLocPtr rLoc;
    SExpToRLocTranslator translator(context, hintType, bWrapExpAsLoc, logger, notLocationLogger, &rLoc);
    sExp.Accept(translator);

    return rLoc;
}


} // namespace Citron::SyntaxIR0Translator
