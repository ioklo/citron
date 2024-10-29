#include "pch.h"
#include "IrExpToNExpTranslation.h"

#include <Infra/Exceptions.h>
#include <Infra/Ptr.h>
#include <Logging/Logger.h>
#include <IR0/NExp.h>

#include "IrExp.h"
#include "TranslationContext.h"

namespace Citron::SyntaxIR0Translator {

namespace {

struct IrBoxRefExpToNExpTranslator : public IrBoxRefExpVisitor
{
    NExpPtr* result;

    IrBoxRefExpToNExpTranslator(NExpPtr* result)
        : result(result) { }

    // &c.x
    void Visit(IrExp_BoxRef_ClassMember& boxRef)
    {
        *result = MakePtr<NExp_ClassMemberBoxRef>(boxRef.loc, boxRef.decl, boxRef.typeArgs);
    }

    // &(*pS).x
    void Visit(IrExp_BoxRef_StructIndirectMember& boxRef)
    {
        *result = MakePtr<NExp_StructIndirectMemberBoxRef>(boxRef.loc, boxRef.decl, boxRef.typeArgs);
    }

    // &c.x.a
    // &(box S()).x.y
    void Visit(IrExp_BoxRef_StructMember& boxRef) override
    {
        NExpPtr parentExp;
        IrBoxRefExpToNExpTranslator parentTranslator(&parentExp);
        boxRef.parent->Accept(parentTranslator);

        *result = MakePtr<NExp_StructMemberBoxRef>(MakePtr<NLoc_Temp>(parentExp), boxRef.decl, boxRef.typeArgs);
    }
};

struct IrExpToNExpTranslator : public IrExpVisitor
{
    TranslationContext& context;
    NExpPtr* result;

    IrExpToNExpTranslator(NExpPtr* result, TranslationContext& context)
        : result(result), context(context) { }

    // &NS
    void Visit(IrExp_Namespace& irExp) override
    {
        context.Log(&Logger::Fatal_Reference_CantMakeReference);
    }

    // &T
    void Visit(IrExp_TypeVar& irExp) override
    {
        context.Log(&Logger::Fatal_Reference_CantMakeReference);
    }

    // &C
    void Visit(IrExp_Class& irExp) override
    {
        context.Log(&Logger::Fatal_Reference_CantMakeReference);
    }

    // &S
    void Visit(IrExp_Struct& irExp) override
    {
        context.Log(&Logger::Fatal_Reference_CantMakeReference);
    }

    // &E
    void Visit(IrExp_Enum& irExp) override
    {
        context.Log(&Logger::Fatal_Reference_CantMakeReference);
    }

    // &this, this는 특수 키워드이고, local storage에 속하지 않는다. 에러를 내도록 한다
    void Visit(IrExp_ThisVar& irExp) override
    {   
        context.Log(&Logger::Fatal_Reference_CantReferenceThis);
    }

    // &C.x
    void Visit(IrExp_StaticRef& irExp) override
    {   
        throw NotImplementedException();
    }

    // &c.x
    void Visit(IrExp_BoxRef& irExp) override
    {
        IrBoxRefExpToNExpTranslator translator(result);
        irExp.Accept(translator);
    }

    // 가장 쉬운 &s.x
    void Visit(IrExp_LocalRef& irExp) override
    {
        *result = MakePtr<NExp_LocalRef>(irExp.loc);
    }

    // box S* pS = ...
    // &(*pS)
    void Visit(IrExp_DerefedBoxValue& irExp) override
    {
        context.Log(&Logger::Fatal_Reference_UselessDereferenceReferencedValue);
    }

    // &G()
    void Visit(IrExp_LocalValue& irExp) override
    {
        context.Log(&Logger::Fatal_Reference_CantReferenceTempValue);
    }
};


} // namespace 

NExpPtr TranslateIrExpToNExp(IrExp& irExp, TranslationContext& context)
{
    NExpPtr result;
    IrExpToNExpTranslator translator(&result, context);
    irExp.Accept(translator);
    return result;
}

} // Citron::SyntaxIR0Translator