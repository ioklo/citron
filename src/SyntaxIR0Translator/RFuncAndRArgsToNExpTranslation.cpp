#include "RFuncAndRArgsToNExpTranslation.h"

#include <vector>
#include <cassert>
#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Logging/Diag.h"
#include "IR0/RFuncDecl.h"
#include "IR0/NExp.h"
#include "IR0/RClassFuncDecl.h"
#include "IR0/RStructFuncDecl.h"

#include "TranslationContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

class TranslationContext;

namespace {

class RFuncAndRArgsToNExpTranslator
{
public:
    using ResultType = expected<NExp*, DiagPtr>;
    
    RTypeArguments* typeArgs;
    NLoc* instance;
    vector<NArgument> args;

    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, NExp>
    ResultType Value(TArgs&&... args)
    {
        return context.MakeNExp<TValue>(forward<TArgs>(args)...);
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
    RFuncAndRArgsToNExpTranslator(RTypeArguments* typeArgs, NLoc* instance, vector<NArgument>&& args, TranslationContext& context)
        : typeArgs{typeArgs}, instance{instance}, args{move(args)}, context{context}
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
        return Value<NExp_CallClassFunc>(func, typeArgs, instance, move(args));
    }

    ResultType Visit(RStructCtorDecl* func) 
    {
        throw NotImplementedException{};
    }

    ResultType Visit(RStructFuncDecl* func) 
    {   
        return Value<NExp_CallStructFunc>(func, typeArgs, instance, move(args));
    }

    ResultType Visit(RLambdaDecl* func) 
    {
        throw NotImplementedException{};
    }
};

} // namespace Citron::SyntaxIR0Translator

expected<NExp*, DiagPtr> TranslateRFuncAndNArgsToNExp(RFuncDecl* decl, RTypeArguments* typeArgs, NLoc* instance, vector<NArgument>&& args, TranslationContext& context)
{
    RFuncAndRArgsToNExpTranslator binder{typeArgs, instance, move(args), context};
    return Accept(binder, decl);
}

}
