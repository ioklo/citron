#include "NClassMemberVarDecl.h"
#include "NClassDecl.h"
#include "RTypeFactory.h"

using namespace std;

namespace Citron {

RDecl* NClassMemberVarDecl::GetROuter()
{
    return _class.lock().get();
}

RIdentifier NClassMemberVarDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

RTypePtr NClassMemberVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return declType->Apply(typeArgs, factory);
}

optional<RMember> NClassMemberVarDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

} // namespace Citron