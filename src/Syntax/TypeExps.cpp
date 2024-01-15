#include "pch.h"
#include "Syntax/TypeExps.h"

#include <utility> // std::move

#include "Syntax/SyntaxMacros.h"

using namespace std;

namespace Citron::Syntax {

IdTypeExp::IdTypeExp(string name, vector<TypeExp> typeArgs)
    : name(std::move(name)), typeArgs(std::move(typeArgs)) { }

IMPLEMENT_DEFAULTS_DEFAULT(IdTypeExp)

struct MemberTypeExp::Impl
{
    TypeExp typeExp;
    string name;
    vector<TypeExp> typeArgs;
};

MemberTypeExp::MemberTypeExp(TypeExp typeExp, string name, vector<TypeExp> typeArgs)
    : impl(new Impl{ std::move(typeExp), std::move(name), std::move(typeArgs) })
{   
}

IMPLEMENT_DEFAULTS_PIMPL(MemberTypeExp)

TypeExp& MemberTypeExp::GetParent()
{
    return impl->typeExp;
}

string& MemberTypeExp::GetMemberName()
{
    return impl->name;
}

vector<TypeExp>& MemberTypeExp::GetTypeArgs()
{
    return impl->typeArgs;
}

struct NullableTypeExp::Impl
{
    TypeExp innerTypeExp;
};

NullableTypeExp::NullableTypeExp(TypeExp typeExp)
    : impl(new Impl{ std::move(typeExp)})
{   
}
IMPLEMENT_DEFAULTS_PIMPL(NullableTypeExp)

TypeExp& NullableTypeExp::GetInnerTypeExp()
{
    return impl->innerTypeExp;
}

struct LocalPtrTypeExp::Impl
{
    TypeExp innerTypeExp;
};

LocalPtrTypeExp::LocalPtrTypeExp(TypeExp typeExp)
    : impl(new Impl{ std::move(typeExp) })
{

}

IMPLEMENT_DEFAULTS_PIMPL(LocalPtrTypeExp)

TypeExp& LocalPtrTypeExp::GetInnerTypeExp()
{
    return impl->innerTypeExp;
}

struct BoxPtrTypeExp::Impl
{
    TypeExp innerTypeExp;
};

BoxPtrTypeExp::BoxPtrTypeExp(TypeExp typeExp)
    : impl(new Impl{ std::move(typeExp) })
{

}

IMPLEMENT_DEFAULTS_PIMPL(BoxPtrTypeExp)

TypeExp& BoxPtrTypeExp::GetInnerTypeExp()
{
    return impl->innerTypeExp;
}

struct LocalTypeExp::Impl
{
    TypeExp innerTypeExp;
};

LocalTypeExp::LocalTypeExp(TypeExp typeExp)
    : impl(new Impl{ std::move(typeExp) })
{

}

IMPLEMENT_DEFAULTS_PIMPL(LocalTypeExp)

TypeExp& LocalTypeExp::GetInnerTypeExp()
{
    return impl->innerTypeExp;
}




}