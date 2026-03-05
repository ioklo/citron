#pragma once
#include <variant>

namespace Citron {

struct MLoc;
struct MLoc_Materialize;

struct MMoveSource_MovedLoc { MLoc* loc; };
struct MMoveSource_Materialized { MLoc_Materialize* loc; };

using MMoveSource = std::variant<MMoveSource_MovedLoc, MMoveSource_Materialized>;

} // namespace Citron
