#include "pch.h"
#include <Syntax/Syntaxes.g.h>

#include <Infra/make_vector.h>

using namespace tcb;

namespace Citron {

ArgumentSyntax::ArgumentSyntax(ExpSyntax exp)
    : ArgumentSyntax(false, false, std::move(exp))
{
}

IdTypeExpSyntax::IdTypeExpSyntax(std::string name)
    : IdTypeExpSyntax(std::move(name), {})
{ }

StringExpSyntax::StringExpSyntax(std::string str)
    : StringExpSyntax(make_vector<StringExpSyntaxElement>(TextStringExpSyntaxElement(str))) 
{ 
}

MemberExpSyntax::MemberExpSyntax(ExpSyntax parent, std::string memberName)
    : MemberExpSyntax(std::move(parent), std::move(memberName), {})
{
}

IndirectMemberExpSyntax::IndirectMemberExpSyntax(ExpSyntax parent, std::string memberName)
    : IndirectMemberExpSyntax(std::move(parent), std::move(memberName), {})
{
}

}