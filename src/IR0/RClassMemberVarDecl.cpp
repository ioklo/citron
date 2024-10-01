#include "RClassMemberVarDecl.h"

namespace Citron {

RTypePtr RClassMemberVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return declType->Apply(typeArgs, factory);
}

} // namespace Citron