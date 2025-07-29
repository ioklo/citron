#include "Syntax.h"

#include <utility>
#include <string>

#include "Infra/make_vector.h"
#include "Infra/Ptr.h"

using namespace tcb;
using namespace std;

namespace Citron {

SArgument::SArgument(SExpPtr exp)
    : SArgument(false, false, move(exp))
{
}

STypeExp_Id::STypeExp_Id(std::string name)
    : STypeExp_Id(move(name), {})
{ }

SExp_String::SExp_String(std::string str)
    : SExp_String(make_vector<SStringExpElementPtr>(MakePtr<SStringExpElement_Text>(move(str))))
{ 
}

SExp_Member::SExp_Member(SExpPtr parent, std::string memberName)
    : SExp_Member(move(parent), move(memberName), {})
{
}

SExp_IndirectMember::SExp_IndirectMember(SExpPtr parent, std::string memberName)
    : SExp_IndirectMember(move(parent), move(memberName), {})
{
}

}