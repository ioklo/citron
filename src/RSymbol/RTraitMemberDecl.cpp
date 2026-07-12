#include "RTraitMemberDecl.h"
#include "RTraitFuncDecl.h"

using namespace std;

namespace Citron {

RDecl* RTraitMemberDecl::GetDecl()
{
    return visit([](auto* memberDecl) -> RDecl* { return memberDecl; }, v);
}

} // namespace Citron