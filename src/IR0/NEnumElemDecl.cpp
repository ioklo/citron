#include "NEnumElemDecl.h"
#include "NEnumDecl.h"

using namespace std;

namespace Citron {

NEnumElemDecl::NEnumElemDecl(weak_ptr<NEnumDecl> _enum, string name, size_t memberVarCount)
    : _enum(move(_enum)), name(move(name))
{
    memberVars.reserve(memberVarCount);
}

void NEnumElemDecl::AddMemberVar(std::shared_ptr<NEnumElemMemberVarDecl> memberVar)
{
    memberVars.push_back(std::move(memberVar));
}

NDecl* NEnumElemDecl::GetOuter()
{
    return _enum.lock().get();
}

RIdentifier NEnumElemDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
}

} // namespace Citron