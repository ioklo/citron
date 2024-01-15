#include "pch.h"
#include "Syntax/ClassMemberDecls.h"

#include <utility>
#include <memory>

#include "Syntax/TypeDecl.h"
#include "Syntax/SyntaxMacros.h"

namespace Citron::Syntax {

struct ClassMemberTypeDecl::Impl
{
    TypeDecl typeDecl;
};

ClassMemberTypeDecl::ClassMemberTypeDecl(TypeDecl typeDecl)
    : impl(new Impl{ std::move(typeDecl) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(ClassMemberTypeDecl)

TypeDecl& ClassMemberTypeDecl::GetTypeDecl()
{
    return impl->typeDecl;
}

}