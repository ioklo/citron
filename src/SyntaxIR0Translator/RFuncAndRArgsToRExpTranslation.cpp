#include "pch.h"
#include "RFuncAndRArgsToRExpTranslation.h"

#include <vector>
#include <cassert>

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <IR0/RFuncDecl.h>
#include <IR0/RTypeArguments.h>
#include <IR0/RLoc.h>
#include <IR0/RExp.h>
#include <IR0/RArgument.h>
#include <IR0/RClassMemberFuncDecl.h>
#include <IR0/RStructMemberFuncDecl.h>

using namespace std;

namespace Citron::SyntaxIR0Translator {
namespace {

class RFuncAndRArgsToRExpTranslator : public RFuncDeclVisitor
{
    shared_ptr<RFuncDecl> sharedFuncDecl;

    RTypeArgumentsPtr typeArgs;
    RLocPtr instance;
    vector<RArgument> args;

    RExpPtr* result;

public:
    RFuncAndRArgsToRExpTranslator(const RTypeArgumentsPtr& typeArgs, RLocPtr&& instance, vector<RArgument>&& args, RExpPtr* result)
        : typeArgs(typeArgs), instance(std::move(instance)), args(std::move(args)), result(result)
    {
    }

    void Visit(RGlobalFuncDecl& func) override 
    {
        throw NotImplementedException();
    }

    void Visit(RClassConstructorDecl& func) override 
    {
        throw NotImplementedException();
    }

    void Visit(RClassMemberFuncDecl& func) override 
    {
        auto sharedClassMemberFuncDecl = dynamic_pointer_cast<RClassMemberFuncDecl>(sharedFuncDecl);
        assert(sharedClassMemberFuncDecl);

        *result = MakePtr<RExp_CallClassMemberFunc>(std::move(sharedClassMemberFuncDecl), std::move(typeArgs), std::move(instance), std::move(args));
    }

    void Visit(RStructConstructorDecl& func) override 
    {
        throw new NotImplementedException();
    }

    void Visit(RStructMemberFuncDecl& func) override 
    {   
        auto sharedStructMemberFuncDecl = dynamic_pointer_cast<RStructMemberFuncDecl>(sharedFuncDecl);
        assert(sharedStructMemberFuncDecl);

        *result = MakePtr<RExp_CallStructMemberFunc>(std::move(sharedStructMemberFuncDecl), std::move(typeArgs), std::move(instance), std::move(args));
    }

    void Visit(RLambdaDecl& func) override 
    {
        throw NotImplementedException();
    }
};

} // namespace Citron::SyntaxIR0Translator

RExpPtr TranslateRFuncAndRArgsToRExp(const shared_ptr<RFuncDecl>& decl, const RTypeArgumentsPtr& typeArgs, RLocPtr&& instance, vector<RArgument>&& args)
{
    RExpPtr exp;
    RFuncAndRArgsToRExpTranslator binder(typeArgs, std::move(instance), std::move(args), &exp);
    decl->Accept(binder);
    return exp;
}


}
