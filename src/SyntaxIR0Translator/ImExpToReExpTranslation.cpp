#include "pch.h"
#include "ImExpToReExpTranslation.h"

#include <Infra/Exceptions.h>
#include <Infra/Ptr.h>
#include <Logging/Logger.h>
#include <IR0/REnumDecl.h>

#include "ImExp.h"
#include "ReExp.h"
#include "ScopeContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

struct ImExpToReExpTranslator : public ImExpVisitor
{
    ScopeContextPtr context;
    LoggerPtr logger;
    ReExpPtr* result;

    ImExpToReExpTranslator(const ScopeContextPtr& context, const LoggerPtr& logger, ReExpPtr* result)
        : result(result), context(context), logger(logger)
    {
    }

    void Visit(ImNamespaceExp& imExp) override 
    {
        logger->Fatal_CantUseNamespaceAsExpression();
    }

    // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
    void Visit(ImGlobalFuncsExp& imExp) override 
    {
        throw NotImplementedException();
    }

    void Visit(ImTypeVarExp& imExp) override 
    {
        logger->Fatal_CantUseTypeAsExpression();
    }

    void Visit(ImClassExp& imExp) override 
    {
        logger->Fatal_CantUseTypeAsExpression();
    }

    void Visit(ImClassMemberFuncsExp& imExp) override 
    {
        // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
        throw NotImplementedException();
    }

    void Visit(ImStructExp& imExp) override 
    {   
        logger->Fatal_CantUseTypeAsExpression();
    }

    void Visit(ImStructMemberFuncsExp& imExp) override 
    {
        // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
        throw NotImplementedException();
    }

    void Visit(ImEnumExp& imExp) override 
    {
        logger->Fatal_CantUseTypeAsExpression();
    }

    void Visit(ImEnumElemExp& imExp) override 
    {
        // if standalone, 값으로 처리한다
        if (imExp.decl->memberVars.size() == 0)
        {
            *result = MakePtr<ReElseExp>(MakePtr<RNewEnumElemExp>(imExp.decl, imExp.typeArgs, vector<RArgument>()));
            return;
        }
        
        // lambda (boxed lambda)로 변환할 수 있다.
        throw NotImplementedException();

    }
    void Visit(ImThisVarExp& imExp) override 
    {
        *result = MakePtr<ReThisVarExp>(imExp.type);
    }
    void Visit(ImLocalVarExp& imExp) override 
    {
        *result = MakePtr<ReLocalVarExp>(imExp.type, imExp.name);
    }
    void Visit(ImLambdaMemberVarExp& imExp) override 
    {
        *result = MakePtr<ReLambdaMemberVarExp>(imExp.decl, imExp.typeArgs);
    }
    void Visit(ImClassMemberVarExp& imExp) override 
    {
        *result = MakePtr<ReClassMemberVarExp>(imExp.decl, imExp.typeArgs, imExp.hasExplicitInstance, imExp.explicitInstance);
    }
    void Visit(ImStructMemberVarExp& imExp) override 
    {
        *result = MakePtr<ReStructMemberVarExp>(imExp.decl, imExp.typeArgs, imExp.hasExplicitInstance, imExp.explicitInstance);
    }
    void Visit(ImEnumElemMemberVarExp& imExp) override 
    {
        *result = MakePtr<ReEnumElemMemberVarExp>(imExp.decl, imExp.typeArgs, imExp.instance);
    }
    void Visit(ImListIndexerExp& imExp) override 
    {   
        *result = MakePtr<ReListIndexerExp>(imExp.instance, imExp.index, imExp.itemType);
    }
    void Visit(ImLocalDerefExp& imExp) override 
    {   
        *result = MakePtr<ReLocalDerefExp>(imExp.target);
    }
    void Visit(ImBoxDerefExp& imExp) override
    {
        *result = MakePtr<ReBoxDerefExp>(imExp.target);
    }
    void Visit(ImElseExp& imExp) override 
    {
        *result = MakePtr<ReElseExp>(imExp.exp);
    }
};

// outermost로 변경
// IntermediateExp -> TranslationResult<ResolvedExp>

ReExpPtr TranslateImExpToReExp(const ImExpPtr& imExp, const ScopeContextPtr& context, const LoggerPtr& logger)
{
    ReExpPtr result;
    ImExpToReExpTranslator translator(context, logger, &result);

    imExp->Accept(translator);

    return result;
}

}
