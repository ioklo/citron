#include "Syntax.h"

#include <utility>
#include <string>

#include "Infra/make_vector.h"

#include "SFactory.h"

using namespace tcb;
using namespace std;

namespace Citron {

SArgument::SArgument(SExp* exp)
    : SArgument(nullopt, exp)
{
}

STypeExp_Id::STypeExp_Id(std::string&& name)
    : STypeExp_Id(name, {})
{ }

SExp_String::SExp_String(std::string&& str, SFactory& factory)
    : SExp_String(make_vector<SStringExpElement*>(factory.MakeSStringExpElement_Text(str)))
{
}

SExp_Member::SExp_Member(SExp* parent, std::string&& memberName)
    : SExp_Member(parent, move(memberName), {})
{
}

SExp_IndirectMember::SExp_IndirectMember(SExp* parent, std::string&& memberName)
    : SExp_IndirectMember(parent, move(memberName), {})
{
}

}