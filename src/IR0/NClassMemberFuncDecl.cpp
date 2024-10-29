#include "NClassMemberFuncDecl.h"
#include "NClassDecl.h"

namespace Citron {

RIdentifier NClassMemberFuncDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), NCommonFuncDeclComponent::GetParamIds() };
}

NDecl* NClassMemberFuncDecl::GetOuter()
{
    return _class.lock().get();
}

NDecl* NClassMemberFuncDecl::GetDecl()
{
    return this;
}

} // namespace Citron