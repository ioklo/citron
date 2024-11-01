#include "NClassMemberFuncDecl.h"
#include "NClassDecl.h"

using namespace std;

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

optional<RMember> NClassMemberFuncDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

} // namespace Citron