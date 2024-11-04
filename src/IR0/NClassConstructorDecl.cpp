#include "NClassConstructorDecl.h"
#include "NClassDecl.h"

using namespace std;

namespace Citron {

RDecl* NClassConstructorDecl::GetROuter()
{
    return _class.lock().get();
}

RIdentifier NClassConstructorDecl::GetIdentifier()
{
    return RIdentifier { RName_Reserved("Constructor"), 0, NCommonFuncDeclComponent::GetParamIds() };
}

optional<RMember> NClassConstructorDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

shared_ptr<RClassDecl> NClassConstructorDecl::GetClassDecl()
{
    return _class.lock();
}

} // namespace Citron