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
    expected<NExpPtr, DiagPtr>* result;

    shared_ptr<RFuncDecl> sharedFuncDecl;
    RTypeArgumentsPtr typeArgs;
    NLocPtr instance;
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
    RFuncAndRArgsToNExpTranslator(expected<NExpPtr, DiagPtr>* result, shared_ptr<RFuncDecl> sharedFuncDecl, const RTypeArgumentsPtr& typeArgs, NLocPtr&& instance, vector<NArgument>&& args)
        : result(result), sharedFuncDecl(sharedFuncDecl), typeArgs(typeArgs), instance(move(instance)), args(move(args))
    {
    }

    void Visit(RGlobalFuncDecl& func) override 
    {
        throw NotImplementedException();
    }

    void Visit(RClassCtorDecl& func) override 
    {
        throw NotImplementedException();
    }

    void Visit(RClassFuncDecl& func) override 
    {
        auto sharedClassFuncDecl = dynamic_pointer_cast<RClassFuncDecl>(sharedFuncDecl);
        assert(sharedClassFuncDecl);

        return Value<NExp_CallClassFunc>(move(sharedClassFuncDecl), move(typeArgs), move(instance), move(args));
    }

    void Visit(RStructCtorDecl& func) override 
    {
        throw NotImplementedException();
    }

    void Visit(RStructFuncDecl& func) override 
    {   
        auto sharedStructFuncDecl = dynamic_pointer_cast<RStructFuncDecl>(sharedFuncDecl);
        assert(sharedStructFuncDecl);

        return Value<NExp_CallStructFunc>(move(sharedStructFuncDecl), move(typeArgs), move(instance), move(args));
    }

    void Visit(RLambdaDecl& func) override 
    {
        throw NotImplementedException();
    }
};

} // namespace Citron::SyntaxIR0Translator

expected<NExpPtr, DiagPtr> TranslateRFuncAndNArgsToNExp(const shared_ptr<RFuncDecl>& decl, const RTypeArgumentsPtr& typeArgs, NLocPtr&& instance, vector<NArgument>&& args)
{
    expected<NExpPtr, DiagPtr> exp;
    RFuncAndRArgsToNExpTranslator binder(&exp, decl, typeArgs, move(instance), move(args));
    decl->Accept(binder);
    return exp;
}


}
