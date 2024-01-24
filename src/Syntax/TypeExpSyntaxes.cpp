#include "pch.h"
#include <Syntax/TypeExpSyntaxes.h>

#include <utility> // std::move

#include <Syntax/SyntaxMacros.h>

using namespace std;

namespace Citron {

IdTypeExpSyntax::IdTypeExpSyntax(u32string name, vector<TypeExpSyntax> typeArgs)
    : name(std::move(name)), typeArgs(std::move(typeArgs)) { }

IMPLEMENT_DEFAULTS_DEFAULT(IdTypeExpSyntax)

struct MemberTypeExpSyntax::Impl
{
    TypeExpSyntax typeExp;
    u32string name;
    vector<TypeExpSyntax> typeArgs;
};

MemberTypeExpSyntax::MemberTypeExpSyntax(TypeExpSyntax typeExp, u32string name, vector<TypeExpSyntax> typeArgs)
    : impl(new Impl{ std::move(typeExp), std::move(name), std::move(typeArgs) })
{   
}

IMPLEMENT_DEFAULTS_PIMPL(MemberTypeExpSyntax)

TypeExpSyntax& MemberTypeExpSyntax::GetParent()
{
    return impl->typeExp;
}

u32string& MemberTypeExpSyntax::GetMemberName()
{
    return impl->name;
}

vector<TypeExpSyntax>& MemberTypeExpSyntax::GetTypeArgs()
{
    return impl->typeArgs;
}

struct NullableTypeExpSyntax::Impl
{
    TypeExpSyntax innerTypeExp;
};

NullableTypeExpSyntax::NullableTypeExpSyntax(TypeExpSyntax typeExp)
    : impl(new Impl{ std::move(typeExp)})
{   
}
IMPLEMENT_DEFAULTS_PIMPL(NullableTypeExpSyntax)

TypeExpSyntax& NullableTypeExpSyntax::GetInnerTypeExp()
{
    return impl->innerTypeExp;
}

struct LocalPtrTypeExpSyntax::Impl
{
    TypeExpSyntax innerTypeExp;
};

LocalPtrTypeExpSyntax::LocalPtrTypeExpSyntax(TypeExpSyntax typeExp)
    : impl(new Impl{ std::move(typeExp) })
{

}

IMPLEMENT_DEFAULTS_PIMPL(LocalPtrTypeExpSyntax)

TypeExpSyntax& LocalPtrTypeExpSyntax::GetInnerTypeExp()
{
    return impl->innerTypeExp;
}

struct BoxPtrTypeExpSyntax::Impl
{
    TypeExpSyntax innerTypeExp;
};

BoxPtrTypeExpSyntax::BoxPtrTypeExpSyntax(TypeExpSyntax typeExp)
    : impl(new Impl{ std::move(typeExp) })
{

}

IMPLEMENT_DEFAULTS_PIMPL(BoxPtrTypeExpSyntax)

TypeExpSyntax& BoxPtrTypeExpSyntax::GetInnerTypeExp()
{
    return impl->innerTypeExp;
}

struct LocalTypeExpSyntax::Impl
{
    TypeExpSyntax innerTypeExp;
};

LocalTypeExpSyntax::LocalTypeExpSyntax(TypeExpSyntax typeExp)
    : impl(new Impl{ std::move(typeExp) })
{

}

IMPLEMENT_DEFAULTS_PIMPL(LocalTypeExpSyntax)

TypeExpSyntax& LocalTypeExpSyntax::GetInnerTypeExp()
{
    return impl->innerTypeExp;
}




}