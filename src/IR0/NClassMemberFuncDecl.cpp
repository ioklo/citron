#include "NClassMemberFuncDecl.h"
#include "NClassDecl.h"

using namespace std;

namespace Citron {

RDecl* NClassMemberFuncDecl::GetROuter()
{
    return _class.lock().get();
}

RIdentifier NClassMemberFuncDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), NCommonFuncDeclComponent::GetParamIds() };
}

optional<RMember> NClassMemberFuncDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

} // namespace Citron