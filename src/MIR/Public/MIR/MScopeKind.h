#pragma once
#include <variant>

namespace Citron {

struct MScopeKind_Default {};
struct MScopeKind_Loop { size_t labelId; };
struct MScopeKind_Switch { size_t labelId; };
using MScopeKind = std::variant<MScopeKind_Default, MScopeKind_Loop, MScopeKind_Switch>;

} // namespace Citron