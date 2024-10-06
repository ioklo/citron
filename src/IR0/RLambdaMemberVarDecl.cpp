#include "RLambdaMemberVarDecl.h"
#include "RLambdaDecl.h"

namespace Citron {

RTypePtr RLambdaMemberVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return type->Apply(typeArgs, factory);
}

RDecl* RLambdaMemberVarDecl::GetOuter()
{
    return lambda.lock().get();
}

RIdentifier RLambdaMemberVarDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

} // namespace Citron