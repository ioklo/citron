#include "pch.h"
#include "ImExpToReExpTranslation.h"

#include <Infra/Exceptions.h>
#include <Infra/Ptr.h>
#include <Logging/Logger.h>
#include <IR0/NEnumDecl.h>

#include "ImExp.h"
#include "ReExp.h"
#include "TranslationContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

struct ImExpToReExpTranslator : public ImExpVisitor
{   
    ReExpPtr* result;
    TranslationContext& context;

    ImExpToReExpTranslator(ReExpPtr* result, TranslationContext& context)
        : result(result), context(context)
    {
    }

    void Visit(ImExp_Namespace& imExp) override
    {
        context.Log(&Logger::Fatal_ResolveIdentifier_CantUseNamespaceAsExpression);
    }

    // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
    void Visit(ImExp_GlobalFuncs& imExp) override
    {
        throw NotImplementedException();
    }

    void Visit(ImExp_TypeVar& imExp) override
    {
        context.Log(&Logger::Fatal_ResolveIdentifier_CantUseTypeAsExpression);
    }

    void Visit(ImExp_Class& imExp) override
    {
        context.Log(&Logger::Fatal_ResolveIdentifier_CantUseTypeAsExpression);
    }

    void Visit(ImExp_ClassFuncs& imExp) override
    {
        // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
        throw NotImplementedException();
    }

    void Visit(ImExp_Struct& imExp) override
    {
        context.Log(&Logger::Fatal_ResolveIdentifier_CantUseTypeAsExpression);
    }

    void Visit(ImExp_StructFuncs& imExp) override
    {
        // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
        throw NotImplementedException();
    }

    void Visit(ImExp_Enum& imExp) override
    {
        context.Log(&Logger::Fatal_ResolveIdentifier_CantUseTypeAsExpression);
    }

    void Visit(ImExp_EnumElem& imExp) override
    {
        // if standalone, 값으로 처리한다
        if (imExp.decl->GetMemberVarCount() == 0)
        {
            *result = MakePtr<ReExp_Else>(MakePtr<NExp_NewEnumElem>(imExp.decl, imExp.typeArgs, vector<NArgument>()));
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
ReExpPtr TranslateImExpToReExp(ImExp& imExp, TranslationContext& context)
{
    ReExpPtr result;
    ImExpToReExpTranslator translator(&result, context);
    imExp.Accept(translator);

    return result;
}

}
