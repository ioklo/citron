#include "RClassMemberFuncDecl.h"
#include "RClassDecl.h"

namespace Citron {

RIdentifier RClassMemberFuncDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), RCommonFuncDeclComponent::GetParamIds() };
}

RDecl* RClassMemberFuncDecl::GetOuter()
{
    return _class.lock().get();
}

RDecl* RClassMemberFuncDecl::GetDecl()
{
    return this;
}

} // namespace Citron