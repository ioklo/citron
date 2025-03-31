#include "pch.h"
#include "RFuncAndRArgsToNExpTranslation.h"

import <vector>;
import <cassert>;

import Citron.Ptr;
import Citron.Exceptions;
#include <IR0/RFuncDecl.h>
#include <IR0/RTypeArguments.h>
#include <IR0/NLoc.h>
#include <IR0/NExp.h>
#include <IR0/NArgument.h>
#include <IR0/NClassFuncDecl.h>
#include <IR0/NStructFuncDecl.h>

using namespace std;

namespace Citron::SyntaxIR0Translator {
namespace {

class RFuncAndRArgsToNExpTranslator : public RFuncDeclVisitor
{
    shared_ptr<RFuncDecl> sharedFuncDecl;

    RTypeArgumentsPtr typeArgs;
    NLocPtr instance;
    vector<NArgument> args;

    NExpPtr* result;

public:
    RFuncAndRArgsToNExpTranslator(shared_ptr<RFuncDecl> sharedFuncDecl, const RTypeArgumentsPtr& typeArgs, NLocPtr&& instance, vector<NArgument>&& args, NExpPtr* result)
        : sharedFuncDecl(sharedFuncDecl), typeArgs(typeArgs), instance(std::move(instance)), args(std::move(args)), result(result)
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

        *result = MakePtr<NExp_CallClassFunc>(std::move(sharedClassFuncDecl), std::move(typeArgs), std::move(instance), std::move(args));
    }

    void Visit(RStructCtorDecl& func) override 
    {
        throw new NotImplementedException();
    }

    void Visit(RStructFuncDecl& func) override 
    {   
        auto sharedStructFuncDecl = dynamic_pointer_cast<RStructFuncDecl>(sharedFuncDecl);
        assert(sharedStructFuncDecl);

        *result = MakePtr<NExp_CallStructFunc>(std::move(sharedStructFuncDecl), std::move(typeArgs), std::move(instance), std::move(args));
    }

    void Visit(RLambdaDecl& func) override 
    {
        throw NotImplementedException();
    }
};

} // namespace Citron::SyntaxIR0Translator

NExpPtr TranslateRFuncAndNArgsToNExp(const shared_ptr<RFuncDecl>& decl, const RTypeArgumentsPtr& typeArgs, NLocPtr&& instance, vector<NArgument>&& args)
{
    NExpPtr exp;
    RFuncAndRArgsToNExpTranslator binder(decl, typeArgs, std::move(instance), std::move(args), &exp);
    decl->Accept(binder);
    return exp;
}


}
