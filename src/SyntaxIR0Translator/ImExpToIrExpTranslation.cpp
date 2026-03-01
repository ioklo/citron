#include "ImExpToIrExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RNamespaceDecl.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "NSymbol/NLambdaVarDecl.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"
#include "MIR/MSharedExp.h"

#include "ImExp.h"
#include "IrExp.h"
#include "TranslationContexts.h"
#include "SRTFactory.h"
#include "FuncContext.h"
#include "Misc.h"

using namespace std;

namespace Citron {

namespace {

struct ImExpToIrExpTranslator
{
    using ResultType = expected<IrExp*, DiagPtr>;
    TranslationContexts& contexts;

    ImExpToIrExpTranslator(TranslationContexts& contexts)
        : contexts{contexts}
    {
    }

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, IrExp>
    ResultType Value(TArgs&&... args)
    {
        return contexts.srtFactory->MakeIrExp<TValue>(forward<TArgs>(args)...);
    }
    
public:
    // 지원 불가능한 타입은 모아서 처리
    ResultType Visit(ImExp* imExp)
    {
        return Error<Error_SharedTranslation_CantMakeSharedFromBase>();
    }

    // &NS.C.x 를 지원해야 한다
    ResultType Visit(ImExp_Namespace* imExp)
    {
        return Value<IrExp_Namespace>(imExp->_namespace);
    }

    // &C.x 지원 용도
    ResultType Visit(ImExp_Class* imExp)
    {
        return Value<IrExp_Class>(imExp->classDecl, imExp->typeArgs);
    }

    // &S.x 지원 용도
    ResultType Visit(ImExp_Struct* imExp)
    {
        return Value<IrExp_Struct>(imExp->structDecl, imExp->typeArgs);
    }

    // &this.a
    ResultType Visit(ImExp_ThisVar* imExp)
    {
        return Value<IrExp_Loc>(contexts.mFactory->MakeMLoc<MLoc_This>(imExp->type));
    }

    // &l.x
    ResultType Visit(ImExp_LocalVar* imExp)
    {
        // IrExp
        return Value<IrExp_Loc>(contexts.mFactory->MakeMLoc<MLoc_LocalVar>(imExp->name, imExp->type));
    }

    // shared<int>& s = ...;
    // &s.x
    ResultType Visit(ImExp_LocalRef* imExp)
    {
        return Value<IrExp_Loc>(contexts.mFactory->MakeMLoc<MLoc_LocalRef>(imExp->name, imExp->type));
    }

    // &lv.x
    ResultType Visit(ImExp_LambdaVar* imExp)
    {
        // TODO: [10] box lambda이면 box로 판단해야 한다
        return Value<IrExp_Loc>(contexts.mFactory->MakeMLoc<MLoc_LambdaVar>(imExp->decl, imExp->typeArgs));
    }

    // x (C.x, this.x)
    ResultType Visit(ImExp_ClassVar* imExp)
    {
        if (imExp->decl->IsStatic()) // &C.x
        {
            auto* loc = contexts.mFactory->MakeMLoc<MLoc_ClassVar>(/*instance*/nullptr, imExp->decl, imExp->typeArgs);
            return Value<IrExp_Static>(loc, contexts.rFactory);
        }
        else // &this.x
        {   
            // auto classType = imExp.decl->GetClassType(imExp.typeArgs, factory);
            return Value<IrExp_ClassVar>(
                contexts.funcContext->MakeThisLoc(), imExp->decl, imExp->typeArgs, contexts.rFactory);
        }
    }

    // x (S.x, this->x)
    ResultType Visit(ImExp_StructVar* imExp)
    {
        if (imExp->decl->IsStatic())
        {
            auto* loc = contexts.mFactory->MakeMLoc<MLoc_StructVar>(/*instance*/nullptr, imExp->decl, imExp->typeArgs);
            return Value<IrExp_Static>(loc, contexts.mFactory);
        }
        else
        {   
            // TODO: [10] shared함수이면 this를 shared로 판단해야 한다
            // 지금은 this의 타입이 S&이다.
            return Error<Error_NotImplemented>();
        }
    }

    // &x (E.First.x)    
    ResultType Visit(ImExp_EnumElemVar* imExp)
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException{};
    }

    ResultType Visit(ImExp_ListIndexer* imExp)
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException{};
    }

    ResultType Visit(ImExp_PtrDeref* imExp)
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException{};
    }

    ResultType Visit(ImExp_SharedDeref* imExp)
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException{};
    }

    ResultType Visit(ImExp_Else* imExp)
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException{};
    }
};

} // namespace 

expected<IrExp*, DiagPtr> TranslateImExpToIrExp(ImExp* imExp, TranslationContexts& contexts)
{
    ImExpToIrExpTranslator translator{contexts};
    return Accept(translator, imExp);
}

} // namespace Citron::SyntaxIR0Translation

