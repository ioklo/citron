#include "pch.h"
#include <Syntax/StructMemberDeclSyntaxes.h>
#include <Syntax/TypeDeclSyntax.h>

namespace Citron {

struct StructMemberTypeDeclSyntax::Impl
{
    TypeDeclSyntax typeDecl;
};

StructMemberTypeDeclSyntax::StructMemberTypeDeclSyntax(TypeDeclSyntax typeDecl)
    : impl(new Impl { std::move(typeDecl) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(StructMemberTypeDeclSyntax)

TypeDeclSyntax& StructMemberTypeDeclSyntax::GetTypeDecl()
{
    return impl->typeDecl;
}

BEGIN_IMPLEMENT_JSON_CLASS(StructMemberTypeDeclSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, typeDecl)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(StructMemberFuncDeclSyntax)
    IMPLEMENT_JSON_MEMBER(accessModifier)
    IMPLEMENT_JSON_MEMBER(bStatic)
    IMPLEMENT_JSON_MEMBER(bSequence)
    IMPLEMENT_JSON_MEMBER(retType)
    IMPLEMENT_JSON_MEMBER(name)
    IMPLEMENT_JSON_MEMBER(typeParams)
    IMPLEMENT_JSON_MEMBER(parameters)
    IMPLEMENT_JSON_MEMBER(body)
END_IMPLEMENT_JSON_CLASS()


BEGIN_IMPLEMENT_JSON_CLASS(StructMemberVarDeclSyntax)
    IMPLEMENT_JSON_MEMBER(accessModifier)
    IMPLEMENT_JSON_MEMBER(varType)
    IMPLEMENT_JSON_MEMBER(varNames)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(StructConstructorDeclSyntax)
    IMPLEMENT_JSON_MEMBER(accessModifier)
    IMPLEMENT_JSON_MEMBER(name)
    IMPLEMENT_JSON_MEMBER(parameters)
    IMPLEMENT_JSON_MEMBER(body)
END_IMPLEMENT_JSON_CLASS()

JsonItem ToJson(StructMemberDeclSyntax& syntax)
{
    return std::visit([](auto&& decl) { return decl.ToJson(); }, syntax);
}

}