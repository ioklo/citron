#include "RClassMemberVarDecl.h"
#include "RClassDecl.h"
#include "RTypeFactory.h"

namespace Citron {

RTypePtr RClassMemberVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return declType->Apply(typeArgs, factory);
}

RTypePtr RClassMemberVarDecl::GetClassType(const RTypeArgumentsPtr& typeArgs, RTypeFactory& factory)
{   
    return factory.MakeInstanceType(_class.lock(), typeArgs);
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