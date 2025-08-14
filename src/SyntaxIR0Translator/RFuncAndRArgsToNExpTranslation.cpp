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

class RFuncAndRArgsToNExpTranslator : public RFuncDeclVisitor
{
    expected<NExp*, DiagPtr>* result;
    
    RTypeArguments* typeArgs;
    NLoc* instance;
    vector<NArgument> args;

    TranslationContext& context;

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, NExp>
    void Value(TArgs&&... args)
    {
        *result = context.MakeNExp<TValue>(forward<TArgs>(args)...);
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
    RFuncAndRArgsToNExpTranslator(expected<NExp*, DiagPtr>* result, RTypeArguments* typeArgs, NLoc* instance, vector<NArgument>&& args, TranslationContext& context)
        : result{result}, typeArgs{typeArgs}, instance{instance}, args{move(args)}, context{context}
    {
    }

    void Visit(RGlobalFuncDecl* func) override 
    {
        throw NotImplementedException();
    }

    void Visit(RClassCtorDecl* func) override 
    {
        throw NotImplementedException();
    }

    void Visit(RClassFuncDecl* func) override 
    {
        return Value<NExp_CallClassFunc>(func, typeArgs, instance, move(args));
    }

    void Visit(RStructCtorDecl* func) override 
    {
        throw NotImplementedException();
    }

    void Visit(RStructFuncDecl* func) override 
    {   
        return Value<NExp_CallStructFunc>(func, typeArgs, instance, move(args));
    }

    void Visit(RLambdaDecl* func) override 
    {
        throw NotImplementedException();
    }
};

} // namespace Citron::SyntaxIR0Translator

expected<NExp*, DiagPtr> TranslateRFuncAndNArgsToNExp(RFuncDecl* decl, RTypeArguments* typeArgs, NLoc* instance, vector<NArgument>&& args, TranslationContext& context)
{
    expected<NExp*, DiagPtr> exp;
    RFuncAndRArgsToNExpTranslator binder{&exp, typeArgs, instance, move(args), context};
    decl->Accept(binder);
    return exp;
}


}
