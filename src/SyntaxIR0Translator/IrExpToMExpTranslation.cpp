//#include "IrExpToMExpTranslation.h"
//
//#include <expected>
//
//#include "Infra/Exceptions.h"
//#include "Infra/Ptr.h"
//#include "Logging/Logger.h"
//#include "MIR/MExp.h"
//#include "MIR/MLoc.h"
//#include "MIR/MFactory.h"
//
//#include "IrExp.h"
//#include "TranslationContexts.h"
//#include "Misc.h"
//
//using namespace std;
//
//namespace Citron {
//
//namespace {
//
//struct IrBoxRefExpToMExpTranslator
//{
//public:
//    using ResultType = expected<MExp*, DiagPtr>;
//    TranslationContexts& contexts;
//
//private:
//    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, MExp>
//    ResultType Value(TArgs&&... args)
//    {
//        return contexts.mFactory->MakeMExp<TValue>(forward<TArgs>(args)...);
//    }
//    
//public:
//    IrBoxRefExpToMExpTranslator(TranslationContexts& contexts)
//        : contexts{contexts}
//    { }
//
//    // &c.x
//    ResultType Visit(IrExp_SharedRef_ClassVar* boxRef)
//    {
//        return Value<MExp_ClassMemberBoxRef>(boxRef->loc, boxRef->decl, boxRef->typeArgs, contexts.rFactory);
//    }
//
//    // &(*pS).x
//    ResultType Visit(IrExp_SharedRef_SharedStructVar* boxRef)
//    {
//        return Value<MExp_StructIndirectMemberBoxRef>(boxRef->loc, boxRef->decl, boxRef->typeArgs, contexts.rFactory);
//    }
//
//    // &c.x.a
//    // &(box S()).x.y
//    ResultType Visit(IrExp_SharedRef_StructVar* boxRef)
//    {
//        IrBoxRefExpToMExpTranslator parentTranslator{contexts};
//        auto e_parent = Accept(parentTranslator, boxRef->parent);
//        RETURN_ON_ERROR(e_parent);
//
//        return Value<MExp_StructMemberBoxRef>(contexts.mFactory->MakeMLoc<MLoc_Temp>(*e_parent), boxRef->decl, boxRef->typeArgs, contexts.rFactory);
//    }
//};
//
//struct IrExpToMExpTranslator
//{
//    using ResultType = expected<MExp*, DiagPtr>;
//    TranslationContexts& contexts;
//
//private:
//    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, MExp>
//    ResultType Value(TArgs&&... args)
//    {
//        return contexts.mFactory->MakeMExp<TValue>(forward<TArgs>(args)...);
//    }
//
//public:
//
//    IrExpToMExpTranslator(TranslationContexts& contexts)
//        : contexts{contexts} { }
//
//    // &NS
//    ResultType Visit(IrExp_Namespace* irExp)
//    {
//        return Error<Error_Reference_CantMakeReference>();
//    }
//    
//    // &C
//    ResultType Visit(IrExp_Class* irExp)
//    {
//        return Error<Error_Reference_CantMakeReference>();
//    }
//
//    // &S
//    ResultType Visit(IrExp_Struct* irExp)
//    {
//        return Error<Error_Reference_CantMakeReference>();
//    }
//
//    // &c.x
//    ResultType Visit(IrExp_Shared* irExp)
//    {
//        IrBoxRefExpToMExpTranslator translator{contexts};
//        return Accept(translator, irExp);
//    }
//
//    // box S* pS = ...
//    // &(*pS)
//    ResultType Visit(IrExp_SharedDeref* irExp)
//    {
//        return Error<Error_Reference_UselessDereferenceReferencedValue>();
//    }
//
//    // &G()
//    ResultType Visit(IrExp_Exp* irExp)
//    {
//        return Error<Error_Reference_CantReferenceTempValue>();
//    }
//
//    ResultType Visit(IrExp_Loc* irExp)
//    {
//        static_assert(false);
//    }
//};
//
//} // namespace 
//
//expected<MExp*, DiagPtr> TranslateIrExpToMExp(IrExp* irExp, TranslationContexts& contexts)
//{
//    IrExpToMExpTranslator translator{contexts};
//    return Accept(translator, irExp);
//}
//
//} // Citron::SyntaxIR0Translator