#include "pch.h"
#include <Syntax/TypeExpSyntaxes.h>

#include <utility> // std::move

#include <Infra/Json.h>
#include <Syntax/SyntaxMacros.h>

using namespace std;

namespace Citron {

IdTypeExpSyntax::IdTypeExpSyntax(u32string name, vector<TypeExpSyntax> typeArgs)
    : name(std::move(name)), typeArgs(std::move(typeArgs)) { }

IMPLEMENT_DEFAULTS_DEFAULT(IdTypeExpSyntax)

BEGIN_IMPLEMENT_JSON_CLASS(IdTypeExpSyntax)
    IMPLEMENT_JSON_MEMBER(name)
    IMPLEMENT_JSON_MEMBER(typeArgs)
END_IMPLEMENT_JSON_CLASS()

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

BEGIN_IMPLEMENT_JSON_CLASS(MemberTypeExpSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, typeExp)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, name)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, typeArgs)
END_IMPLEMENT_JSON_CLASS()

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

BEGIN_IMPLEMENT_JSON_CLASS(NullableTypeExpSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, innerTypeExp)
END_IMPLEMENT_JSON_CLASS()

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

BEGIN_IMPLEMENT_JSON_CLASS(LocalPtrTypeExpSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, innerTypeExp)
END_IMPLEMENT_JSON_CLASS()

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

BEGIN_IMPLEMENT_JSON_CLASS(BoxPtrTypeExpSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, innerTypeExp)
END_IMPLEMENT_JSON_CLASS()

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

BEGIN_IMPLEMENT_JSON_CLASS(LocalTypeExpSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, innerTypeExp)
END_IMPLEMENT_JSON_CLASS()

JsonItem ToJson(TypeExpSyntax& typeExp)
{
    return std::visit([](auto&& typeExp) { return typeExp.ToJson(); }, typeExp);
}

}