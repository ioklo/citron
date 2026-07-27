#include "ImExpToReExp.h"

#include "Infra/Exceptions.h"
#include "Logging/Diag.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/REnumElemDecl.h"
#include "MIR/MArgument.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"
#include "MIR/MInitExp.h"
#include "MIR/MFactory.h"
#include "ImExp.h"
#include "ReExp.h"
#include "SmTranslationContexts.h"
#include "Misc.h"
#include "ImExpTranslations.h"

using namespace std;

namespace Citron {

namespace {

// expected<ReExp, DiagPtr>을 돌려준다
struct ImExpToReExpTranslator
{   
    using ResultType = expected<ReExp, DiagPtr>;
    SmTranslationContexts& contexts;

    ImExpToReExpTranslator(SmTranslationContexts& contexts)
        : contexts{contexts}
    {
    }

private:

    template<typename TLoc, typename... TArgs> requires std::derived_from<TLoc, MLoc>
    ResultType Loc(TArgs&&... args)
    {
        auto* loc = contexts.mFactory->MakeMLoc<TLoc>(forward<TArgs>(args)...);
        return ReExp_Loc{loc};
    }

    template<typename TMExp, typename... TArgs> requires std::derived_from<TMExp, MExp>
    ResultType Exp(TArgs&&... args)
    {
        auto* exp = contexts.mFactory->MakeMExp<TMExp>(forward<TArgs>(args)...);
        return ReExp_Exp{exp};
    }

    template<typename TMInitExp, typename... TArgs> requires std::derived_from<TMInitExp, MInitExp>
    ResultType InitExp(TArgs&&... args)
    {
        auto* exp = contexts.mFactory->MakeMInitExp<TMInitExp>(forward<TArgs>(args)...);
        return ReExp_InitExp{exp};
    }

public:
    ResultType Visit(ImExp_Namespace* imExp)
    {
        return Error<Error_ResolveIdentifier_CantUseNamespaceAsExpression>();
    }
    
    ResultType Visit(ImExp_GlobalFuncs* imExp)
    {
        // TODO: [51] 단일 ClassFuncs, StructFuncs가 expression으로 쓰이면, Lambda로 쓰일수 있게 변환
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
        // TODO: [51] 단일 ClassFuncs, StructFuncs가 expression으로 쓰이면, Lambda로 쓰일수 있게 변환
        throw NotImplementedException{};
    }

    ResultType Visit(ImExp_Struct* imExp)
    {
        return Error<Error_ResolveIdentifier_CantUseTypeAsExpression>();
    }

    ResultType Visit(ImExp_StructFuncs* imExp)
    {
        // TODO: [51] 단일 ClassFuncs, StructFuncs가 expression으로 쓰이면, Lambda로 쓰일수 있게 변환
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
            auto* enumElemType = contexts.rFactory->MakeEnumElemType(imExp->decl, imExp->typeArgs);
            switch (enumElemType->GetCopyStrategy())
            {
            case RCopyStrategy::Void: throw RuntimeFatalException{};
            case RCopyStrategy::Bitwise:
                return Exp<MExp_NewEnumElem>(imExp->decl, imExp->typeArgs, vector<MArgument>{});
            case RCopyStrategy::NonBitwise:
                return InitExp<MInitExp_NewEnumElem>(imExp->decl, imExp->typeArgs, vector<MArgument>{});
            }

            unreachable();
        }

        // TODO: [51] 단일 ClassFuncs, StructFuncs 등이 expression으로 쓰이면, Lambda로 쓰일수 있게 변환
        throw NotImplementedException{};
    }

    ResultType Visit(ImExp_ClassVar* imExp)
    {
        return ReExp_Loc{TranslateImExp_ClassVarToMLoc_ClassVar(imExp, contexts)};
    }

    ResultType Visit(ImExp_StructVar* imExp)
    {
        return ReExp_Loc{TranslateImExp_StructVarToMLoc_StructVar(imExp, contexts)};
    }

    ResultType Visit(ImExp_ReExp* imExp)
    {
        return imExp->reExp;
    }
};

}

// outermost로 변경
expected<ReExp, DiagPtr> TranslateImExpToReExp(ImExp* imExp, SmTranslationContexts& contexts)
{
    ImExpToReExpTranslator translator{contexts};
    return Accept(translator, imExp);
}

}
