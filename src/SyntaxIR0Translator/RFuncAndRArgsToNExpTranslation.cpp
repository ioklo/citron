#include "pch.h"
#include "RFuncAndRArgsToNExpTranslation.h"

#include <vector>
#include <cassert>

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <IR0/RFuncDecl.h>
#include <IR0/RTypeArguments.h>
#include <IR0/NLoc.h>
#include <IR0/NExp.h>
#include <IR0/NArgument.h>
#include <IR0/NClassMemberFuncDecl.h>
#include <IR0/NStructMemberFuncDecl.h>

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

    void Visit(RClassConstructorDecl& func) override 
    {
        throw NotImplementedException();
    }

    void Visit(RClassMemberFuncDecl& func) override 
    {
        auto sharedClassMemberFuncDecl = dynamic_pointer_cast<RClassMemberFuncDecl>(sharedFuncDecl);
        assert(sharedClassMemberFuncDecl);

        *result = MakePtr<NExp_CallClassMemberFunc>(std::move(sharedClassMemberFuncDecl), std::move(typeArgs), std::move(instance), std::move(args));
    }

    void Visit(RStructConstructorDecl& func) override 
    {
        throw new NotImplementedException();
    }

    void Visit(RStructMemberFuncDecl& func) override 
    {   
        auto sharedStructMemberFuncDecl = dynamic_pointer_cast<RStructMemberFuncDecl>(sharedFuncDecl);
        assert(sharedStructMemberFuncDecl);

        *result = MakePtr<NExp_CallStructMemberFunc>(std::move(sharedStructMemberFuncDecl), std::move(typeArgs), std::move(instance), std::move(args));
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
