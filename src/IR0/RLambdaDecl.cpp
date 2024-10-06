#include "RLambdaDecl.h"

namespace Citron {

RDecl* RLambdaDecl::GetOuter()
{
    return outer.lock()->GetDecl();
}

RIdentifier RLambdaDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

RDecl* RLambdaDecl::GetDecl()
{
    return this;
}

} // namespace Citron