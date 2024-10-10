#include "RClassConstructorDecl.h"
#include "RClassDecl.h"

namespace Citron {

RDecl* RClassConstructorDecl::GetOuter()
{
    return _class.lock().get();
}

RIdentifier RClassConstructorDecl::GetIdentifier()
{
    return RIdentifier { RName_Reserved("Constructor"), 0, RCommonFuncDeclComponent::GetParamIds() };
}

RDecl* RClassConstructorDecl::GetDecl()
{
    return this;
}

} // namespace Citron