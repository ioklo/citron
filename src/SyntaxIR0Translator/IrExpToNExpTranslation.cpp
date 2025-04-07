module Citron.SyntaxIR0Translator:IrExpToNExpTranslation;

import <expected>;

import Citron.Exceptions;
import Citron.Ptr;
import Citron.Logger;
import Citron.NDecls;

import :IrExp;
import :TranslationContext;

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

struct IrBoxRefExpToNExpTranslator : public IrBoxRefExpVisitor
{
    expected<NExpPtr, DiagPtr>* result;

private:
    void Value(NExpPtr&& nExp)
    {
        *result = std::move(nExp);
    }

    void Error(const DiagPtr& diag)
    {
        *result = unexpected{diag};
    }

public:
    IrBoxRefExpToNExpTranslator(expected<NExpPtr, DiagPtr>* result)
        : result(result) { }

    // &c.x
    void Visit(IrExp_BoxRef_ClassMember& boxRef)
    {
        return Value(MakePtr<NExp_ClassMemberBoxRef>(boxRef.loc, boxRef.decl, boxRef.typeArgs));
    }

    // &(*pS).x
    void Visit(IrExp_BoxRef_StructIndirectMember& boxRef)
    {
        return Value(MakePtr<NExp_StructIndirectMemberBoxRef>(boxRef.loc, boxRef.decl, boxRef.typeArgs));
    }

    // &c.x.a
    // &(box S()).x.y
    void Visit(IrExp_BoxRef_StructMember& boxRef) override
    {
        expected<NExpPtr, DiagPtr> parentExp;
        IrBoxRefExpToNExpTranslator parentTranslator{&parentExp};
        boxRef.parent->Accept(parentTranslator);
        if (!parentExp) return Error(parentExp.error());

        return Value(MakePtr<NExp_StructMemberBoxRef>(MakePtr<NLoc_Temp>(*parentExp), boxRef.decl, boxRef.typeArgs));
    }
};

struct IrExpToNExpTranslator : public IrExpVisitor
{
    expected<NExpPtr, DiagPtr>* result;
    TranslationContext& context;

private:
    void Value(NExpPtr&& nExp)
    {
        *result = std::move(nExp);
    }

    void Error(const DiagPtr& diag)
    {
        *result = unexpected{diag};
    }

public:

    IrExpToNExpTranslator(expected<NExpPtr, DiagPtr>* result, TranslationContext& context)
        : result(result), context(context) { }

    // &NS
    void Visit(IrExp_Namespace& irExp) override
    {
        return Error(MakePtr<Error_Reference_CantMakeReference>());
    }

    // &T
    void Visit(IrExp_TypeVar& irExp) override
    {
        return Error(MakePtr<Error_Reference_CantMakeReference>());
    }

    // &C
    void Visit(IrExp_Class& irExp) override
    {
        return Error(MakePtr<Error_Reference_CantMakeReference>());
    }

    // &S
    void Visit(IrExp_Struct& irExp) override
    {
        return Error(MakePtr<Error_Reference_CantMakeReference>());
    }

    // &E
    void Visit(IrExp_Enum& irExp) override
    {
        return Error(MakePtr<Error_Reference_CantMakeReference>());
    }

    // &this, this는 특수 키워드이고, local storage에 속하지 않는다. 에러를 내도록 한다
    void Visit(IrExp_ThisVar& irExp) override
    {   
        return Error(MakePtr<Error_Reference_CantReferenceThis>());
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
        return Value(MakePtr<NExp_LocalRef>(irExp.loc));
    }

    // box S* pS = ...
    // &(*pS)
    void Visit(IrExp_DerefedBoxValue& irExp) override
    {
        return Error(MakePtr<Error_Reference_UselessDereferenceReferencedValue>());
    }

    // &G()
    void Visit(IrExp_LocalValue& irExp) override
    {
        return Error(MakePtr<Error_Reference_CantReferenceTempValue>());
    }
};


} // namespace 

expected<NExpPtr, DiagPtr> TranslateIrExpToNExp(IrExp& irExp, TranslationContext& context)
{
    expected<NExpPtr, DiagPtr> result;
    IrExpToNExpTranslator translator{&result, context};
    irExp.Accept(translator);
    return result;
}

} // Citron::SyntaxIR0Translator