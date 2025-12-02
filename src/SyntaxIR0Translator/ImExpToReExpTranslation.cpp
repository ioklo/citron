#include "ImExpToReExpTranslation.h"

#include <expected>

#include "Infra/Exceptions.h"
#include "Infra/Ptr.h"
#include "Logging/Logger.h"

#include "RSymbol/RFactory.h"
#include "RSymbol/REnumElemDecl.h"
#include "MIR/MArgument.h"
#include "MIR/MExp.h"

#include "ImExp.h"
#include "ReExp.h"
#include "TranslationContext.h"

using namespace std;

namespace Citron {

namespace {

// expected<ReExp*, DiagPtr>을 돌려준다
struct ImExpToReExpTranslator
{   
public:
    using ResultType = expected<ReExp*, DiagPtr>;

    TranslationContext& context;

    ImExpToReExpTranslator(TranslationContext& context)
        : context(context)
    {
    }

private:

    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, ReExp>
    ResultType Value(TArgs&&... args)
    {
        return context.MakeReExp<TValue>(forward<TArgs>(args)...);
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
            return Value<ReExp_Else>(context.MakeMExp<MExp_NewEnumElem>(imExp->decl, imExp->typeArgs, vector<MArgument>()));
        }

        // lambda (boxed lambda)로 변환할 수 있다.
        throw NotImplementedException{};

    }
    ResultType Visit(ImExp_ThisVar* imExp)
    {
        return Value<ReExp_ThisVar>(imExp->type);
    }
    ResultType Visit(ImExp_LocalVar* imExp)
    {
        return Value<ReExp_LocalVar>(imExp->type, imExp->name);
    }
    ResultType Visit(ImExp_LambdaVar* imExp)
    {
        return Value<ReExp_LambdaVar>(imExp->decl, imExp->typeArgs);
    }
    ResultType Visit(ImExp_ClassVar* imExp)
    {
        return Value<ReExp_ClassVar>(imExp->decl, imExp->typeArgs, imExp->hasExplicitInstance, imExp->explicitInstance);
    }
    ResultType Visit(ImExp_StructVar* imExp)
    {
        return Value<ReExp_StructVar>(imExp->decl, imExp->typeArgs, imExp->hasExplicitInstance, imExp->explicitInstance);
    }
    ResultType Visit(ImExp_EnumElemVar* imExp)
    {
        return Value<ReExp_EnumElemVar>(imExp->decl, imExp->typeArgs, imExp->instance);
    }
    ResultType Visit(ImExp_ListIndexer* imExp)
    {
        return Value<ReExp_ListIndexer>(imExp->instance, imExp->index, imExp->itemType);
    }
    ResultType Visit(ImExp_LocalDeref* imExp)
    {
        return Value<ReExp_LocalDeref>(imExp->target);
    }
    ResultType Visit(ImExp_BoxDeref* imExp)
    {
        return Value<ReExp_BoxDeref>(imExp->target);
    }
    ResultType Visit(ImExp_Else* imExp)
    {
        return Value<ReExp_Else>(imExp->exp);
    }
};

}

// outermost로 변경
expected<ReExp*, DiagPtr> TranslateImExpToReExp(ImExp* imExp, TranslationContext& context)
{
    ImExpToReExpTranslator translator{context};
    return Accept(translator, imExp);
}

}
