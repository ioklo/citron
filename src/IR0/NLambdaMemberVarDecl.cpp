#include "NLambdaMemberVarDecl.h"
#include "NLambdaDecl.h"

using namespace std;

namespace Citron {

NDecl* NLambdaMemberVarDecl::GetOuter()
{
    return lambda.lock().get();
}

RIdentifier NLambdaMemberVarDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

RTypePtr NLambdaMemberVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return type->Apply(typeArgs, factory);
}

optional<RMember> NLambdaMemberVarDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}


} // namespace Citron