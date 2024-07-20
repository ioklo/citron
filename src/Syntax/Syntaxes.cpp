#include "pch.h"
#include <Syntax/Syntaxes.g.h>

#include <Infra/make_vector.h>

using namespace tcb;

namespace Citron {

SArgument::SArgument(SExpPtr exp)
    : SArgument(false, false, std::move(exp))
{
}

SIdTypeExp::SIdTypeExp(std::string name)
    : SIdTypeExp(std::move(name), {})
{ }

SStringExp::SStringExp(std::string str)
    : SStringExp(make_vector<SStringExpElementPtr>(make_unique<STextStringExpElement>(std::move(str))))
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