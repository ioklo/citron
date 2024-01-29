#include "pch.h"
#include <Syntax/ClassMemberDeclSyntaxes.h>

#include <utility>
#include <memory>

#include <Syntax/TypeDeclSyntax.h>
#include <Syntax/SyntaxMacros.h>

namespace Citron {

struct ClassMemberTypeDeclSyntax::Impl
{
    TypeDeclSyntax typeDecl;
};

ClassMemberTypeDeclSyntax::ClassMemberTypeDeclSyntax(TypeDeclSyntax typeDecl)
    : impl(new Impl{ std::move(typeDecl) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(ClassMemberTypeDeclSyntax)

TypeDeclSyntax& ClassMemberTypeDeclSyntax::GetTypeDecl()
{
    return impl->typeDecl;
}

BEGIN_IMPLEMENT_JSON_CLASS(ClassMemberTypeDeclSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, typeDecl)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(ClassMemberFuncDeclSyntax)
    IMPLEMENT_JSON_MEMBER(accessModifier)
    IMPLEMENT_JSON_MEMBER(bStatic)
    IMPLEMENT_JSON_MEMBER(bSequence)
    IMPLEMENT_JSON_MEMBER(retType)
    IMPLEMENT_JSON_MEMBER(name)
    IMPLEMENT_JSON_MEMBER(typeParams)
    IMPLEMENT_JSON_MEMBER(parameters)
    IMPLEMENT_JSON_MEMBER(body)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(ClassConstructorDeclSyntax)
    IMPLEMENT_JSON_MEMBER(accessModifier)
    IMPLEMENT_JSON_MEMBER(name)
    IMPLEMENT_JSON_MEMBER(parameters)
    IMPLEMENT_JSON_MEMBER(baseArgs)
    IMPLEMENT_JSON_MEMBER(body)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(ClassMemberVarDeclSyntax)
    IMPLEMENT_JSON_MEMBER(accessModifier)
    IMPLEMENT_JSON_MEMBER(varType)
    IMPLEMENT_JSON_MEMBER(varNames)
END_IMPLEMENT_JSON_CLASS()

JsonItem ToJson(ClassMemberDeclSyntax& syntax)
{
    return std::visit([](auto&& decl) { return decl.ToJson(); }, syntax);
}

}