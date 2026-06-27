#pragma once
#include "RSymbol/RTraitDecl.h"

namespace Citron {

// Trait는 type으로 취급
class NTraitDecl
    : public NDecl
    , public NTypeDecl
    , public RTraitDecl
    , private NGenericsComponent
{
}

} // namespace Citron