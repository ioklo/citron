#include "ImExpToReExpTranslation.h"

#include <expected>

#include "Infra/Exceptions.h"
#include "Infra/Ptr.h"
#include "Logging/Logger.h"

#include "RSymbol/RFactory.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/REnumElemDecl.h"
#include "MIR/MArgument.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"

#include "ImExp.h"
#include "ReExp.h"
#include "TranslationContexts.h"
#include "SRTFactory.h"
#include "Misc.h"
#include "FuncContext.h"
#include "DesignatedDiagnostic.h"

using namespace std;

namespace Citron {

namespace {

// expected<ReExp*, DiagPtr>을 돌려준다
struct ImExpToReExpTranslator
{   
public:
    using ResultType = expected<ReExp*, DiagPtr>;

    TranslationContexts& contexts;

    ImExpToReExpTranslator(TranslationContexts& contexts)
        : contexts{contexts}
    {
    }

private:

    template<typename TLoc, typename... TArgs> requires std::derived_from<TLoc, MLoc>
    ResultType Loc(TArgs&&... args)
    {
        auto* loc = contexts.mFactory->MakeMLoc<TLoc>(forward<TArgs>(args)...);
        return contexts.srtFactory->MakeReExp<ReExp_Loc>(loc);
    }

    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, MExp>
    ResultType Exp(TArgs&&... args)
    {
        auto* exp = contexts.mFactory->MakeMExp<TExp>(forward<TArgs>(args)...);
        return contexts.srtFactory->MakeReExp<ReExp_Exp>(exp);
    }

public:
    ResultType Visit(ImExp_Namespace* imExp)
    {
        return Error<Error_ResolveIdentifier_CantUseNamespaceAsExpression>();
    }

    // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
    ResultType Visit(ImExp_GlobalFuncs* imExp)
    {
        throw NotImplementedException{};
    }

    ResultType Visit(ImExp_TypeVar* imExp)
    {
        return Error<Error_ResolveIdentifier_CantUseTypeAsExpression>();
    }

    ResultType Visit(ImExp_Class* imExp)
    {
        return Error<Error_ResolveIdentifier_CantUseTypeAsExpression>();
    }

    ResultType Visit(ImExp_ClassFuncs* imExp)
    {
        // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
        throw NotImplementedException{};
    }

    ResultType Visit(ImExp_Struct* imExp)
    {
        return Error<Error_ResolveIdentifier_CantUseTypeAsExpression>();
    }

    ResultType Visit(ImExp_StructFuncs* imExp)
    {
        // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
        throw NotImplementedException{};
    }

    ResultType Visit(ImExp_Enum* imExp)
    {
        return Error<Error_ResolveIdentifier_CantUseTypeAsExpression>();
    }

    ResultType Visit(ImExp_EnumElem* imExp)
    {
        // if standalone, 값으로 처리한다
        if (imExp->decl->GetVarCount() == 0)
        {
            return Exp<MExp_NewEnumElem>(imExp->decl, imExp->typeArgs, vector<MArgument>{}, contexts.rFactory);
        }

        // lambda (boxed lambda)로 변환할 수 있다.
        throw NotImplementedException{};

    }
    ResultType Visit(ImExp_ThisVar* imExp)
    {
        return Loc<MLoc_This>(imExp->type);
    }
    ResultType Visit(ImExp_LocalVar* imExp)
    {
        return Loc<MLoc_LocalVar>(imExp->type, imExp->name);
    }
    ResultType Visit(ImExp_LocalRef* imExp)
    {
        return Loc<MLoc_LocalRef>(imExp->type, imExp->name);
    }
    ResultType Visit(ImExp_LambdaVar* imExp)
    {
        return Loc<MLoc_LambdaVar>(imExp->decl, imExp->typeArgs);
    }

    ResultType Visit(ImExp_ClassVar* imExp)
    {
        if (imExp->hasExplicitInstance) // c.x, C.x 둘다 해당
        {
            return Loc<MLoc_ClassVar>(imExp->explicitInstance, imExp->decl, imExp->typeArgs);
        }
        else // x, x (static) 둘다 해당
        {
            MLoc* mInstanceLoc = imExp->decl->IsStatic() ? nullptr : contexts.funcContext->MakeThisLoc();
            return Loc<MLoc_ClassVar>(mInstanceLoc, imExp->decl, imExp->typeArgs);
        }

        return Loc<MLoc_ClassVar>(imExp->decl, imExp->typeArgs, imExp->hasExplicitInstance, imExp->explicitInstance);
    }
    ResultType Visit(ImExp_StructVar* imExp)
    {
        return Loc<MLoc_StructVar>(imExp->decl, imExp->typeArgs, imExp->hasExplicitInstance, imExp->explicitInstance);
    }
    ResultType Visit(ImExp_EnumElemVar* imExp)
    {
        return Loc<MLoc_EnumElemVar>(imExp->decl, imExp->typeArgs, imExp->instance);
    }
    ResultType Visit(ImExp_ListIndexer* imExp)
    {
        return Loc<MLoc_ListIndexer>(imExp->instance, imExp->index, imExp->itemType);
    }
    ResultType Visit(ImExp_PtrDeref* imExp)
    {
        return Loc<MLoc_PtrDeref>(imExp->target);
    }
    ResultType Visit(ImExp_SharedDeref* imExp)
    {
        return Loc<MLoc_SharedDeref>(imExp->target);
    }
    ResultType Visit(ImExp_Exp* imExp)
    {
        return contexts.srtFactory->MakeReExp<ReExp_Exp>(imExp->exp);
    }
};

}

// outermost로 변경
expected<ReExp*, DiagPtr> TranslateImExpToReExp(ImExp* imExp, TranslationContexts& contexts)
{
    ImExpToReExpTranslator translator{contexts};
    return Accept(translator, imExp);
}

}
