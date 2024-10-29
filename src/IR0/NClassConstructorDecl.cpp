#include "NClassConstructorDecl.h"
#include "NClassDecl.h"

namespace Citron {

NDecl* NClassConstructorDecl::GetOuter()
{
    return _class.lock().get();
}

RIdentifier NClassConstructorDecl::GetIdentifier()
{
    return RIdentifier { RName_Reserved("Constructor"), 0, NCommonFuncDeclComponent::GetParamIds() };
}

NDecl* NClassConstructorDecl::GetDecl()
{
    return this;
}

} // namespace Citron