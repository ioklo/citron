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

namespace {

struct ImExpToReExpTranslator : public ImExpVisitor
{
    ScopeContext& context;
    Logger& logger;
    ReExpPtr* result;

    ImExpToReExpTranslator(ScopeContext& context, Logger& logger, ReExpPtr* result)
        : result(result), context(context), logger(logger)
    {
    }

    void Visit(ImExp_Namespace& imExp) override
    {
        logger.Fatal_ResolveIdentifier_CantUseNamespaceAsExpression();
    }

    // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
    void Visit(ImExp_GlobalFuncs& imExp) override
    {
        throw NotImplementedException();
    }

    void Visit(ImExp_TypeVar& imExp) override
    {
        logger.Fatal_ResolveIdentifier_CantUseTypeAsExpression();
    }

    void Visit(ImExp_Class& imExp) override
    {
        logger.Fatal_ResolveIdentifier_CantUseTypeAsExpression();
    }

    void Visit(ImExp_ClassMemberFuncs& imExp) override
    {
        // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
        throw NotImplementedException();
    }

    void Visit(ImExp_Struct& imExp) override
    {
        logger.Fatal_ResolveIdentifier_CantUseTypeAsExpression();
    }

    void Visit(ImExp_StructMemberFuncs& imExp) override
    {
        // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
        throw NotImplementedException();
    }

    void Visit(ImExp_Enum& imExp) override
    {
        logger.Fatal_ResolveIdentifier_CantUseTypeAsExpression();
    }

    void Visit(ImExp_EnumElem& imExp) override
    {
        // if standalone, 값으로 처리한다
        if (imExp.decl->memberVars.size() == 0)
        {
            *result = MakePtr<ReExp_Else>(MakePtr<RExp_NewEnumElem>(imExp.decl, imExp.typeArgs, vector<RArgument>()));
            return;
        }

        // lambda (boxed lambda)로 변환할 수 있다.
        throw NotImplementedException();

    }
    void Visit(ImExp_ThisVar& imExp) override
    {
        *result = MakePtr<ReExp_ThisVar>(imExp.type);
    }
    void Visit(ImExp_LocalVar& imExp) override
    {
        *result = MakePtr<ReExp_LocalVar>(imExp.type, imExp.name);
    }
    void Visit(ImExp_LambdaMemberVar& imExp) override
    {
        *result = MakePtr<ReExp_LambdaMemberVar>(imExp.decl, imExp.typeArgs);
    }
    void Visit(ImExp_ClassMemberVar& imExp) override
    {
        *result = MakePtr<ReExp_ClassMemberVar>(imExp.decl, imExp.typeArgs, imExp.hasExplicitInstance, imExp.explicitInstance);
    }
    void Visit(ImExp_StructMemberVar& imExp) override
    {
        *result = MakePtr<ReExp_StructMemberVar>(imExp.decl, imExp.typeArgs, imExp.hasExplicitInstance, imExp.explicitInstance);
    }
    void Visit(ImExp_EnumElemMemberVar& imExp) override
    {
        *result = MakePtr<ReExp_EnumElemMemberVar>(imExp.decl, imExp.typeArgs, imExp.instance);
    }
    void Visit(ImExp_ListIndexer& imExp) override
    {
        *result = MakePtr<ReExp_ListIndexer>(imExp.instance, imExp.index, imExp.itemType);
    }
    void Visit(ImExp_LocalDeref& imExp) override
    {
        *result = MakePtr<ReExp_LocalDeref>(imExp.target);
    }
    void Visit(ImExp_BoxDeref& imExp) override
    {
        *result = MakePtr<ReExp_BoxDeref>(imExp.target);
    }
    void Visit(ImExp_Else& imExp) override
    {
        *result = MakePtr<ReExp_Else>(imExp.exp);
    }
};

}

// outermost로 변경
ReExpPtr TranslateImExpToReExp(ImExp& imExp, ScopeContext& context, Logger& logger)
{
    ReExpPtr result;
    ImExpToReExpTranslator translator(context, logger, &result);
    imExp.Accept(translator);

    return result;
}

}
