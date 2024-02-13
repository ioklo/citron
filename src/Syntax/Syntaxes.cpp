#include "pch.h"
#include <Syntax/Syntaxes.g.h>

#include <Infra/make_vector.h>

using namespace tcb;

namespace Citron {

ArgumentSyntax::ArgumentSyntax(ExpSyntax exp)
    : ArgumentSyntax(false, false, std::move(exp))
{
}

StringExpSyntax::StringExpSyntax(std::string str)
    : StringExpSyntax(make_vector<StringExpSyntaxElement>(TextStringExpSyntaxElement(str))) 
{ 
}

MemberExpSyntax::MemberExpSyntax(ExpSyntax parent, std::string memberName)
    : MemberExpSyntax(std::move(parent), std::move(memberName), {})
{
}

}