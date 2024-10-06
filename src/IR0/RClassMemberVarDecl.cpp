#include "RClassMemberVarDecl.h"
#include "RClassDecl.h"

namespace Citron {

RTypePtr RClassMemberVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return declType->Apply(typeArgs, factory);
}

RDecl* RClassMemberVarDecl::GetOuter()
{
    return _class.lock().get();
}

RIdentifier RClassMemberVarDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

} // namespace Citron