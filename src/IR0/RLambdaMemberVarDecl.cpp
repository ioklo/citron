#include "RLambdaMemberVarDecl.h"

namespace Citron {

RTypePtr RLambdaMemberVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return type->Apply(typeArgs, factory);
}

} // namespace Citron