#include "REnumElemDecl.h"

using namespace std;

namespace Citron {

REnumElemDecl::REnumElemDecl(weak_ptr<REnumDecl> _enum, string name, size_t memberVarCount)
    : _enum(move(_enum)), name(move(name))
{
    memberVars.reserve(memberVarCount);
}

void REnumElemDecl::AddMemberVar(std::shared_ptr<REnumElemMemberVarDecl> memberVar)
{
    memberVars.push_back(std::move(memberVar));
}

}