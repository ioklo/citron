module Citron.SyntaxIR0Translator:RFuncAndRArgsToNExpTranslation;

import <vector>;
import <cassert>;
import <expected>;

import Citron.Ptr;
import Citron.Exceptions;
import Citron.Diag;
import Citron.RDecls;
import Citron.NDecls;

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
    void Value(NExpPtr&& nExp)
    {
        *result = move(nExp);
    }

    void Error(DiagPtr&& diag)
    {
        *result = unexpected{move(diag)};
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

        return Value(MakePtr<NExp_CallClassFunc>(move(sharedClassFuncDecl), move(typeArgs), move(instance), move(args)));
    }

    void Visit(RStructCtorDecl& func) override 
    {
        throw NotImplementedException();
    }

    void Visit(RStructFuncDecl& func) override 
    {   
        auto sharedStructFuncDecl = dynamic_pointer_cast<RStructFuncDecl>(sharedFuncDecl);
        assert(sharedStructFuncDecl);

        return Value(MakePtr<NExp_CallStructFunc>(move(sharedStructFuncDecl), move(typeArgs), move(instance), move(args)));
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
