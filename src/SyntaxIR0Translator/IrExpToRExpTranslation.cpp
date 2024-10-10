#include "pch.h"
#include "IrExpToRExpTranslation.h"

#include <Infra/Exceptions.h>
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
    void Visit(IrExp_BoxRef_ClassMember& boxRef)
    {
        *result = MakePtr<RExp_ClassMemberBoxRef>(boxRef.loc, boxRef.decl, boxRef.typeArgs);
    }

    // &(*pS).x
    void Visit(IrExp_BoxRef_StructIndirectMember& boxRef)
    {
        *result = MakePtr<RExp_StructIndirectMemberBoxRef>(boxRef.loc, boxRef.decl, boxRef.typeArgs);
    }

    // &c.x.a
    // &(box S()).x.y
    void Visit(IrExp_BoxRef_StructMember& boxRef) override
    {
        RExpPtr parentExp;
        IrBoxRefExpToRExpTranslator parentTranslator(&parentExp);
        boxRef.parent->Accept(parentTranslator);

        *result = MakePtr<RExp_StructMemberBoxRef>(MakePtr<RLoc_Temp>(parentExp), boxRef.decl, boxRef.typeArgs);
    }
};

struct IrExpToRExpTranslator : public IrExpVisitor
{
    ScopeContextPtr context;
    LoggerPtr logger;
    RExpPtr* result;

    IrExpToRExpTranslator(const ScopeContextPtr& context, const LoggerPtr& logger, RExpPtr* result)
        : context(context), logger(logger), result(result) { }

    // &NS
    void Visit(IrExp_Namespace& irExp) override
    {
        logger->Fatal_CantMakeReference();
    }

    // &T
    void Visit(IrExp_TypeVar& irExp) override
    {
        logger->Fatal_CantMakeReference();
    }

    // &C
    void Visit(IrExp_Class& irExp) override
    {
        logger->Fatal_CantMakeReference();
    }

    // &S
    void Visit(IrExp_Struct& irExp) override
    {
        logger->Fatal_CantMakeReference();
    }

    // &E
    void Visit(IrExp_Enum& irExp) override
    {
        logger->Fatal_CantMakeReference();
    }

    // &this, this는 특수 키워드이고, local storage에 속하지 않는다. 에러를 내도록 한다
    void Visit(IrExp_ThisVar& irExp) override
    {   
        logger->Fatal_CantReferenceThis();
    }

    // &C.x
    void Visit(IrExp_StaticRef& irExp) override
    {   
        throw NotImplementedException();
    }

    // &c.x
    void Visit(IrExp_BoxRef& irExp) override
    {
        IrBoxRefExpToRExpTranslator translator(result);
        irExp.Accept(translator);
    }

    // 가장 쉬운 &s.x
    void Visit(IrExp_LocalRef& irExp) override
    {
        *result = MakePtr<RExp_LocalRef>(irExp.loc);
    }

    // box S* pS = ...
    // &(*pS)
    void Visit(IrExp_DerefedBoxValue& irExp) override
    {
        logger->Fatal_UselessDereferenceReferencedValue();
    }

    // &G()
    void Visit(IrExp_LocalValue& irExp) override
    {
        logger->Fatal_CantReferenceTempValue();
    }
};


} // namespace 

RExpPtr TranslateIrExpToRExp(IrExp& irExp, const ScopeContextPtr& context, const LoggerPtr& logger)
{
    RExpPtr result;
    IrExpToRExpTranslator translator(context, logger, &result);
    irExp.Accept(translator);
    return result;
}

} // Citron::SyntaxIR0Translator