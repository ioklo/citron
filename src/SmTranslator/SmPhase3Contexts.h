#pragma once
#include <expected>
#include <memory>
#include <span>
#include "RSymbol/RAppliedDecl.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class STypeExp;
class RTypeParam;
class SmDeclContext;
class RTraitDecl;
class RFactory;

struct SmPhase3Contexts
{
    RFactory* rFactory;

public:
    std::expected<RAppliedDecl<RTraitDecl>, DiagPtr> MakeTrait(STypeExp* sTypeExp, SmDeclContext* declContext, std::span<RTypeParam*> typeParams);

};

} // namespace Citron