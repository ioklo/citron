#include "NClassConstructorDecl.h"
#include "NClassDecl.h"

using namespace std;

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

shared_ptr<RClassDecl> NClassConstructorDecl::GetClassDecl()
{
    return _class.lock();
}

} // namespace Citron