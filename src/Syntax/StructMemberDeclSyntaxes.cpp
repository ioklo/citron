#include "pch.h"
#include "Syntax/StructMemberDeclSyntaxes.h"
#include "Syntax/TypeDeclSyntax.h"

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

}