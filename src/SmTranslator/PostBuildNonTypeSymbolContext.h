#pragma once
#include "Infra/Ref.h"
#include "RSymbol/RNames.h"

namespace Citron {

class STypeExp;
class RTraitDecl;
class RTypeArguments;
class RTypeDecl;

class PostBuildNonTypeSymbolContext
{

public:
    struct MakeTraitResult { RTraitDecl* decl; RTypeArguments* args; };
    MakeTraitResult MakeTrait(RTypeDecl* rTypeDecl, STypeExp* sTypeExp);
};

} // namespace Citron
