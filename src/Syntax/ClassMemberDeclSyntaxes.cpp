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

}