#pragma once
#include <variant>

namespace Citron {

class RType;

struct RThisKind_Static {};
struct RThisKind_Handle { RType* type; }; // handle 자체의 타입. 즉, class C라면 C
struct RThisKind_Ref { RType* type; }; // type은 원래 타입. 즉 struct S라면 S*가 아니라 S

using RThisKind = std::variant<RThisKind_Static, RThisKind_Handle, RThisKind_Ref>;


} // namespace Citron
