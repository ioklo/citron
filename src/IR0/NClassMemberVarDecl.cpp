#include "NClassMemberVarDecl.h"
#include "NClassDecl.h"
#include "RTypeFactory.h"

namespace Citron {

RTypePtr NClassMemberVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return declType->Apply(typeArgs, factory);
}

NDecl* NClassMemberVarDecl::GetOuter()
{
    return _class.lock().get();
}

RIdentifier NClassMemberVarDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

} // namespace Citron