#include "pch.h"
#include "IrExpToRExpTranslation.h"

#include <Infra/NotImplementedException.h>
#include <Infra/Ptr.h>
#include <Logging/Logger.h>
#include <IR0/RExp.h>

#include "IrExp.h"
#include "ScopeContext.h"

namespace Citron::SyntaxIR0Translator {

namespace {

struct IrBoxRefExpToRExpTranslator : public IrBoxRefExpVisitor
{
    RExpPtr* result;

    IrBoxRefExpToRExpTranslator(RExpPtr* result)
        : result(result) { }

    // &c.x
    void Visit(IrClassMemberBoxRefExp& boxRef)
    {
        *result = MakePtr<RClassMemberBoxRefExp>(boxRef.loc, boxRef.decl, boxRef.typeArgs);
    }

    // &(*pS).x
    void Visit(IrStructIndirectMemberBoxRefExp& boxRef)
    {
        *result = MakePtr<RStructIndirectMemberBoxRefExp>(boxRef.loc, boxRef.decl, boxRef.typeArgs);
    }

    // &c.x.a
    // &(box S()).x.y
    void Visit(IrStructMemberBoxRefExp& boxRef) override
    {
        RExpPtr parentExp;
        IrBoxRefExpToRExpTranslator parentTranslator(&parentExp);
        boxRef.parent->Accept(parentTranslator);

        *result = MakePtr<RStructMemberBoxRefExp>(MakePtr<RTempLoc>(parentExp), boxRef.decl, boxRef.typeArgs);
    }
};

struct IrExpToRExpTranslator : public IrExpVisitor
{
    ScopeContextPtr scopeContext;
    LoggerPtr logger;
    RExpPtr* result;

    IrExpToRExpTranslator(const ScopeContextPtr& scopeContext, const LoggerPtr& logger, RExpPtr* result)
        : scopeContext(scopeContext), logger(logger), result(result) { }

    // &NS
    void Visit(IrNamespaceExp& irExp) override
    {
        logger->Fatal_CantMakeReference();
    }

    // &T
    void Visit(IrTypeVarExp& irExp) override
    {
        logger->Fatal_CantMakeReference();
    }

    // &C
    void Visit(IrClassExp& irExp) override
    {
        logger->Fatal_CantMakeReference();
    }

    // &S
    void Visit(IrStructExp& irExp) override
    {
        logger->Fatal_CantMakeReference();
    }

    // &E
    void Visit(IrEnumExp& irExp) override
    {
        logger->Fatal_CantMakeReference();
    }

    // &this, this는 특수 키워드이고, local storage에 속하지 않는다. 에러를 내도록 한다
    void Visit(IrThisVarExp& irExp) override
    {   
        logger->Fatal_CantReferenceThis();
    }

    // &C.x
    void Visit(IrStaticRefExp& irExp) override
    {   
        throw NotImplementedException();
    }

    // &c.x
    void Visit(IrBoxRefExp& irExp) override
    {
        IrBoxRefExpToRExpTranslator translator(result);
        irExp.Accept(translator);
    }

    // 가장 쉬운 &s.x
    void Visit(IrLocalRefExp& irExp) override
    {
        *result = MakePtr<RLocalRefExp>(irExp.loc);
    }

    // box S* pS = ...
    // &(*pS)
    void Visit(IrDerefedBoxValueExp& irExp) override
    {
        logger->Fatal_UselessDereferenceReferencedValue();
    }

    // &G()
    void Visit(IrLocalValueExp& irExp) override
    {
        logger->Fatal_CantReferenceTempValue();
    }
};


} // namespace 

RExpPtr TranslateIrExpToRExp(IrExpPtr irExp, const ScopeContextPtr& scopeContext, const LoggerPtr& logger)
{
    RExpPtr result;
    IrExpToRExpTranslator translator(scopeContext, logger, &result);
    irExp->Accept(translator);
    return result;
}

} // Citron::SyntaxIR0Translator