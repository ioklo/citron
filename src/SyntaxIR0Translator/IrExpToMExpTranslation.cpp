#include "IrExpToMExpTranslation.h"

#include <expected>

#include "Infra/Exceptions.h"
#include "Infra/Ptr.h"
#include "Logging/Logger.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"

#include "IrExp.h"
#include "TranslationContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

struct IrBoxRefExpToMExpTranslator
{
public:
    using ResultType = expected<MExp*, DiagPtr>;
    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, MExp>
    ResultType Value(TArgs&&... args)
    {
        return context.MakeMExp<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    ResultType Error(expected<TValue, DiagPtr>&& e)
    {
        return unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    ResultType Error(TArgs&&... args)
    {
        return unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

public:
    IrBoxRefExpToMExpTranslator(TranslationContext& context)
        : context{context}
    { }

    // &c.x
    ResultType Visit(IrExp_BoxRef_ClassMember* boxRef)
    {
        return Value<MExp_ClassMemberBoxRef>(boxRef->loc, boxRef->decl, boxRef->typeArgs);
    }

    // &(*pS).x
    ResultType Visit(IrExp_BoxRef_StructIndirectMember* boxRef)
    {
        return Value<MExp_StructIndirectMemberBoxRef>(boxRef->loc, boxRef->decl, boxRef->typeArgs);
    }

    // &c.x.a
    // &(box S()).x.y
    ResultType Visit(IrExp_BoxRef_StructMember* boxRef)
    {
        IrBoxRefExpToMExpTranslator parentTranslator{context};
        auto eParent = Accept(parentTranslator, boxRef->parent);
        if (!eParent) return Error(move(eParent));

        return Value<MExp_StructMemberBoxRef>(context.MakeNLoc<MLoc_Temp>(*eParent), boxRef->decl, boxRef->typeArgs);
    }
};

struct IrExpToMExpTranslator
{
    using ResultType = expected<MExp*, DiagPtr>;
    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, MExp>
    ResultType Value(TArgs&&... args)
    {
        return context.MakeMExp<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    ResultType Error(expected<TValue, DiagPtr>&& e)
    {
        return unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    ResultType Error(TArgs&&... args)
    {
        return unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

public:

    IrExpToMExpTranslator(TranslationContext& context)
        : context{context} { }

    // &NS
    ResultType Visit(IrExp_Namespace* irExp)
    {
        return Error<Error_Reference_CantMakeReference>();
    }

    // &T
    ResultType Visit(IrExp_TypeVar* irExp)
    {
        return Error<Error_Reference_CantMakeReference>();
    }

    // &C
    ResultType Visit(IrExp_Class* irExp)
    {
        return Error<Error_Reference_CantMakeReference>();
    }

    // &S
    ResultType Visit(IrExp_Struct* irExp)
    {
        return Error<Error_Reference_CantMakeReference>();
    }

    // &E
    ResultType Visit(IrExp_Enum* irExp)
    {
        return Error<Error_Reference_CantMakeReference>();
    }

    // &this, this는 특수 키워드이고, local storage에 속하지 않는다. 에러를 내도록 한다
    ResultType Visit(IrExp_ThisVar* irExp)
    {   
        return Error<Error_Reference_CantReferenceThis>();
    }

    // &C.x
    ResultType Visit(IrExp_StaticRef* irExp)
    {   
        throw NotImplementedException{};
    }

    // &c.x
    ResultType Visit(IrExp_BoxRef* irExp)
    {
        IrBoxRefExpToMExpTranslator translator{context};
        return Accept(translator, irExp);
    }

    // 가장 쉬운 &s.x
    ResultType Visit(IrExp_LocalRef* irExp)
    {
        return Value<MExp_LocalRef>(irExp->loc);
    }

    // box S* pS = ...
    // &(*pS)
    ResultType Visit(IrExp_DerefedBoxValue* irExp)
    {
        return Error<Error_Reference_UselessDereferenceReferencedValue>();
    }

    // &G()
    ResultType Visit(IrExp_LocalValue* irExp)
    {
        return Error<Error_Reference_CantReferenceTempValue>();
    }
};


} // namespace 

expected<MExp*, DiagPtr> TranslateIrExpToMExp(IrExp* irExp, TranslationContext& context)
{
    IrExpToMExpTranslator translator{context};
    return Accept(translator, irExp);
}

} // Citron::SyntaxIR0Translator