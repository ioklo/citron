#include "IrExpToNExpTranslation.h"

#include <expected>

#include "Infra/Exceptions.h"
#include "Infra/Ptr.h"
#include "Logging/Logger.h"
#include "IR0/NExp.h"
#include "IR0/NLoc.h"

#include "IrExp.h"
#include "TranslationContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

struct IrBoxRefExpToNExpTranslator : public IrBoxRefExpVisitor
{
    expected<NExp*, DiagPtr>* result;

private:
    template<typename TValue, typename... TArgs> requires std::is_base_of_v<NExp, TValue>
    void Value(TArgs&&... args)
    {
        *result = MakePtr<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    void Error(expected<TValue, DiagPtr>&& e)
    {
        *result = unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::is_base_of_v<Diag, TDiag>
    void Error(TArgs&&... args)
    {
        *result = unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

public:
    IrBoxRefExpToNExpTranslator(expected<NExp*, DiagPtr>* result)
        : result(result) { }

    // &c.x
    void Visit(IrExp_BoxRef_ClassMember& boxRef)
    {
        return Value<NExp_ClassMemberBoxRef>(boxRef.loc, boxRef.decl, boxRef.typeArgs);
    }

    // &(*pS).x
    void Visit(IrExp_BoxRef_StructIndirectMember& boxRef)
    {
        return Value<NExp_StructIndirectMemberBoxRef>(boxRef.loc, boxRef.decl, boxRef.typeArgs);
    }

    // &c.x.a
    // &(box S()).x.y
    void Visit(IrExp_BoxRef_StructMember& boxRef) override
    {
        expected<NExp*, DiagPtr> eParent;
        IrBoxRefExpToNExpTranslator parentTranslator{&eParent};
        boxRef.parent->Accept(parentTranslator);
        if (!eParent) return Error(move(eParent));

        return Value<NExp_StructMemberBoxRef>(MakePtr<NLoc_Temp>(*eParent), boxRef.decl, boxRef.typeArgs);
    }
};

struct IrExpToNExpTranslator : public IrExpVisitor
{
    expected<NExp*, DiagPtr>* result;
    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::is_base_of_v<NExp, TValue>
    void Value(TArgs&&... args)
    {
        *result = MakePtr<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    void Error(expected<TValue, DiagPtr>&& e)
    {
        *result = unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::is_base_of_v<Diag, TDiag>
    void Error(TArgs&&... args)
    {
        *result = unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

public:

    IrExpToNExpTranslator(expected<NExp*, DiagPtr>* result, TranslationContext& context)
        : result(result), context(context) { }

    // &NS
    void Visit(IrExp_Namespace& irExp) override
    {
        return Error<Error_Reference_CantMakeReference>();
    }

    // &T
    void Visit(IrExp_TypeVar& irExp) override
    {
        return Error<Error_Reference_CantMakeReference>();
    }

    // &C
    void Visit(IrExp_Class& irExp) override
    {
        return Error<Error_Reference_CantMakeReference>();
    }

    // &S
    void Visit(IrExp_Struct& irExp) override
    {
        return Error<Error_Reference_CantMakeReference>();
    }

    // &E
    void Visit(IrExp_Enum& irExp) override
    {
        return Error<Error_Reference_CantMakeReference>();
    }

    // &this, this는 특수 키워드이고, local storage에 속하지 않는다. 에러를 내도록 한다
    void Visit(IrExp_ThisVar& irExp) override
    {   
        return Error<Error_Reference_CantReferenceThis>();
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
        return Value<NExp_LocalRef>(irExp.loc);
    }

    // box S* pS = ...
    // &(*pS)
    void Visit(IrExp_DerefedBoxValue& irExp) override
    {
        return Error<Error_Reference_UselessDereferenceReferencedValue>();
    }

    // &G()
    void Visit(IrExp_LocalValue& irExp) override
    {
        return Error<Error_Reference_CantReferenceTempValue>();
    }
};


} // namespace 

expected<NExp*, DiagPtr> TranslateIrExpToNExp(IrExp& irExp, TranslationContext& context)
{
    expected<NExp*, DiagPtr> result;
    IrExpToNExpTranslator translator{&result, context};
    irExp.Accept(translator);
    return result;
}

} // Citron::SyntaxIR0Translator