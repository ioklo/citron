#include "ImExpToReExpTranslation.h"

#include <expected>

#include "Infra/Exceptions.h"
#include "Infra/Ptr.h"
#include "Logging/Logger.h"

#include "IR0/RFactory.h"
#include "IR0/REnumElemDecl.h"
#include "IR0/NArgument.h"
#include "IR0/NExp.h"

#include "ImExp.h"
#include "ReExp.h"
#include "TranslationContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

struct ImExpToReExpTranslator : public ImExpVisitor
{   
    expected<ReExp*, DiagPtr>* result;
    TranslationContext& context;

    ImExpToReExpTranslator(expected<ReExp*, DiagPtr>* result, TranslationContext& context)
        : result(result), context(context)
    {
    }

private:

    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, ReExp>
    void Value(TArgs&&... args)
    {
        *result = context.MakeReExp<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    void Error(expected<TValue, DiagPtr>&& e)
    {
        *result = unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    void Error(TArgs&&... args)
    {
        *result = unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

public:
    void Visit(ImExp_Namespace* imExp) override
    {
        return Error<Error_ResolveIdentifier_CantUseNamespaceAsExpression>();
    }

    // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
    void Visit(ImExp_GlobalFuncs* imExp) override
    {
        throw NotImplementedException{};
    }

    void Visit(ImExp_TypeVar* imExp) override
    {
        return Error<Error_ResolveIdentifier_CantUseTypeAsExpression>();
    }

    void Visit(ImExp_Class* imExp) override
    {
        return Error<Error_ResolveIdentifier_CantUseTypeAsExpression>();
    }

    void Visit(ImExp_ClassFuncs* imExp) override
    {
        // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
        throw NotImplementedException{};
    }

    void Visit(ImExp_Struct* imExp) override
    {
        return Error<Error_ResolveIdentifier_CantUseTypeAsExpression>();
    }

    void Visit(ImExp_StructFuncs* imExp) override
    {
        // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
        throw NotImplementedException{};
    }

    void Visit(ImExp_Enum* imExp) override
    {
        return Error<Error_ResolveIdentifier_CantUseTypeAsExpression>();
    }

    void Visit(ImExp_EnumElem* imExp) override
    {
        // if standalone, 값으로 처리한다
        if (imExp->decl->GetVarCount() == 0)
        {
            return Value<ReExp_Else>(context.MakeNExp<NExp_NewEnumElem>(imExp->decl, imExp->typeArgs, vector<NArgument>()));
        }

        // lambda (boxed lambda)로 변환할 수 있다.
        throw NotImplementedException{};

    }
    void Visit(ImExp_ThisVar* imExp) override
    {
        return Value<ReExp_ThisVar>(imExp->type);
    }
    void Visit(ImExp_LocalVar* imExp) override
    {
        return Value<ReExp_LocalVar>(imExp->type, imExp->name);
    }
    void Visit(ImExp_LambdaVar* imExp) override
    {
        return Value<ReExp_LambdaVar>(imExp->decl, imExp->typeArgs);
    }
    void Visit(ImExp_ClassVar* imExp) override
    {
        return Value<ReExp_ClassVar>(imExp->decl, imExp->typeArgs, imExp->hasExplicitInstance, imExp->explicitInstance);
    }
    void Visit(ImExp_StructVar* imExp) override
    {
        return Value<ReExp_StructVar>(imExp->decl, imExp->typeArgs, imExp->hasExplicitInstance, imExp->explicitInstance);
    }
    void Visit(ImExp_EnumElemVar* imExp) override
    {
        return Value<ReExp_EnumElemVar>(imExp->decl, imExp->typeArgs, imExp->instance);
    }
    void Visit(ImExp_ListIndexer* imExp) override
    {
        return Value<ReExp_ListIndexer>(imExp->instance, imExp->index, imExp->itemType);
    }
    void Visit(ImExp_LocalDeref* imExp) override
    {
        return Value<ReExp_LocalDeref>(imExp->target);
    }
    void Visit(ImExp_BoxDeref* imExp) override
    {
        return Value<ReExp_BoxDeref>(imExp->target);
    }
    void Visit(ImExp_Else* imExp) override
    {
        return Value<ReExp_Else>(imExp->exp);
    }
};

}

// outermost로 변경
expected<ReExp*, DiagPtr> TranslateImExpToReExp(ImExp* imExp, TranslationContext& context)
{
    expected<ReExp*, DiagPtr> result;
    ImExpToReExpTranslator translator(&result, context);
    imExp->Accept(translator);
    return result;
}

}
