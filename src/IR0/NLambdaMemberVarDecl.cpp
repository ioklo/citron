#include "NLambdaMemberVarDecl.h"
#include "NLambdaDecl.h"

namespace Citron {

RTypePtr NLambdaMemberVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return type->Apply(typeArgs, factory);
}

NDecl* NLambdaMemberVarDecl::GetOuter()
{
    return lambda.lock().get();
}

RIdentifier NLambdaMemberVarDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

} // namespace Citron