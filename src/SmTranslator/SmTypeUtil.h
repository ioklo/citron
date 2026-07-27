#pragma once
#include <optional>
#include "Infra/Ref.h"

namespace Citron {

class RName;
class RType;
class SmDeclRes;

std::optional<SmDeclRes> GetMember(RType* type, InRef<RName> name);

} // namespace Citron
