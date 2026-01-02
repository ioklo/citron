#include "RFuncAndRArgsToMExpTranslation.h"

#include <vector>
#include <cassert>
#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Logging/Diag.h"
#include "RSymbol/RFuncDecl.h"
#include "RSymbol/RClassFuncDecl.h"
#include "RSymbol/RStructFuncDecl.h"
#include "MIR/MExp.h"
#include "MIR/MFactory.h"

#include "TranslationContexts.h"

using namespace std;

namespace Citron {

namespace {

class RFuncAndRArgsToMExpTranslator
{
public:
    using ResultType = expected<MExp*, DiagPtr>;
    
    RTypeArguments* typeArgs;
    MLoc* instance;
    vector<MArgument> args;

    TranslationContexts& contexts;

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, MExp>
    ResultType Value(TArgs&&... args)
    {
        return contexts.mFactory->MakeMExp<TValue>(forward<TArgs>(args)...);
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
    RFuncAndRArgsToMExpTranslator(RTypeArguments* typeArgs, MLoc* instance, vector<MArgument>&& args, TranslationContexts& contexts)
        : typeArgs{typeArgs}, instance{instance}, args{move(args)}, contexts{contexts}
    {
    }

    ResultType Visit(RGlobalFuncDecl* func) 
    {
        throw NotImplementedException{};
    }

    ResultType Visit(RClassCtorDecl* func) 
    {
        throw NotImplementedException{};
    }

    ResultType Visit(RClassFuncDecl* func) 
    {
        return Value<MExp_CallClassFunc>(func, typeArgs, instance, move(args));
    }

    ResultType Visit(RStructCtorDecl* func) 
    {
        throw NotImplementedException{};
    }

    ResultType Visit(RStructDtorDecl* func)
    {
        throw NotImplementedException{};
    }

    ResultType Visit(RStructFuncDecl* func) 
    {   
        return Value<MExp_CallStructFunc>(func, typeArgs, instance, move(args));
    }

    ResultType Visit(RLambdaDecl* func) 
    {
        throw NotImplementedException{};
    }
};

} // namespace Citron

expected<MExp*, DiagPtr> TranslateRFuncAndNArgsToMExp(RFuncDecl* decl, RTypeArguments* typeArgs, MLoc* instance, vector<MArgument>&& args, TranslationContexts& contexts)
{
    RFuncAndRArgsToMExpTranslator binder{typeArgs, instance, move(args), contexts};
    return Accept(binder, decl);
}

}
