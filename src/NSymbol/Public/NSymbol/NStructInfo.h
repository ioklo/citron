#pragma once
#include <vector>
#include <variant>
#include "NImplTrait.h"

namespace Citron {

class RTraitDecl;
class RTraitFuncDecl;
class RTypeArguments;

struct NStructInfo
{
    std::vector<NImplTrait> implTraits; // struct가 구현한 trait들
};

} // namespace Citron
