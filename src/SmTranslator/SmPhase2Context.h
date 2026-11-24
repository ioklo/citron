#pragma once
#include <expected>
#include <memory>
#include "RSymbol/RAppliedDecl.h"

namespace Citron {

class STypeExp;
class RType;
class RTraitDecl;
class RFactory;
class SmTypeResolveScope;
using DiagPtr = std::shared_ptr<struct Diag>;

class SmPhase2Context
{
    RFactory* rFactory;

public:
    SmPhase2Context(RFactory* rFactory) : rFactory{rFactory} {}
    std::expected<RAppliedDecl<RTraitDecl>, DiagPtr> MakeTrait(STypeExp* sType, SmTypeResolveScope scope);
};

} // namespace Citron