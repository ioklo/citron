#pragma once
#include <expected>
#include <span>
#include "Logging/Diag.h"
#include "RSymbol/RAppliedDecl.h"

namespace Citron {

class STypeExp;
class RTypeParam;
class SmDeclContext;
class RTraitDecl;
class RFactory;

struct PostBuildTypeHierarchyContexts
{
    RFactory* rFactory;

public:
    std::expected<RAppliedDecl<RTraitDecl>, DiagPtr> MakeTrait(STypeExp* sTypeExp, SmDeclContext* declContext, std::span<RTypeParam*> typeParams);

};

} // namespace Citron