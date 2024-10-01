#include "pch.h"
#include "Syntaxes.g.h"

#include <Infra/make_vector.h>
#include <Infra/Ptr.h>

using namespace tcb;
using namespace std;

namespace Citron {

SArgument::SArgument(SExpPtr exp)
    : SArgument(false, false, std::move(exp))
{
}

SIdTypeExp::SIdTypeExp(std::string name)
    : SIdTypeExp(std::move(name), {})
{ }

SStringExp::SStringExp(std::string str)
    : SStringExp(make_vector<SStringExpElementPtr>(MakePtr<STextStringExpElement>(std::move(str))))
{ 
}

SMemberExp::SMemberExp(SExpPtr parent, std::string memberName)
    : SMemberExp(std::move(parent), std::move(memberName), {})
{
}

SIndirectMemberExp::SIndirectMemberExp(SExpPtr parent, std::string memberName)
    : SIndirectMemberExp(std::move(parent), std::move(memberName), {})
{
}

}