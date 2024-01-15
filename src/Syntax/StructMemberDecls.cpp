#include "pch.h"
#include "Syntax/StructMemberDecls.h"
#include "Syntax/TypeDecl.h"

namespace Citron::Syntax {

struct StructMemberTypeDecl::Impl
{
    TypeDecl typeDecl;
};

StructMemberTypeDecl::StructMemberTypeDecl(TypeDecl typeDecl)
    : impl(new Impl { std::move(typeDecl) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(StructMemberTypeDecl)

TypeDecl& StructMemberTypeDecl::GetTypeDecl()
{
    return impl->typeDecl;
}

}