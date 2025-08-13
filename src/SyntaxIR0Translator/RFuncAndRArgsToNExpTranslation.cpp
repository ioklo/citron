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

using namespace std;

namespace Citron::SyntaxIR0Translator {
namespace {

class RFuncAndRArgsToNExpTranslator : public RFuncDeclVisitor
{
    expected<NExp*, DiagPtr>* result;
    
    RTypeArguments* typeArgs;
    NLoc* instance;
    vector<NArgument> args;

private:
    template<typename TValue, typename... TArgs> requires std::is_base_of_v<NExp, TValue>
    void Value(TArgs&&... args)
    {
        *result = MakePtr<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    void Error(expected<TValue, DiagPtr>&& e)
    {
        *result = unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::is_base_of_v<Diag, TDiag>
    void Error(TArgs&&... args)
    {
        *result = unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

public:
    RFuncAndRArgsToNExpTranslator(expected<NExp*, DiagPtr>* result, RTypeArguments* typeArgs, NLoc* instance, vector<NArgument>&& args)
        : result(result), typeArgs(typeArgs), instance(move(instance)), args(move(args))
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
        return Exp<NExp_CallClassFunc>(func, move(typeArgs), move(instance), move(args));
    }

    void Visit(RStructCtorDecl* func) override 
    {
        throw NotImplementedException();
    }

    void Visit(RStructFuncDecl* func) override 
    {   
        return Exp<NExp_CallStructFunc>(func, move(typeArgs), move(instance), move(args));
    }

    void Visit(RLambdaDecl* func) override 
    {
        throw NotImplementedException();
    }
};

} // namespace Citron::SyntaxIR0Translator

expected<NExp*, DiagPtr> TranslateRFuncAndNArgsToNExp(RFuncDecl* decl, RTypeArguments* typeArgs, NLoc* instance, vector<NArgument>&& args)
{
    expected<NExp*, DiagPtr> exp;
    RFuncAndRArgsToNExpTranslator binder{&exp, typeArgs, move(instance), move(args)};
    decl->Accept(binder);
    return exp;
}


}
