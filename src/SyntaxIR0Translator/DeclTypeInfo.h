#pragma once

#include <memory>

namespace Citron {

class RType;

enum class DeclTypeInfoKind
{
    Normal,
    PlainVar,
    LocalInterfaceVar, // starts with local
    SharedVar,
    LocalPtrVar,
    NullableVar,
};

struct DeclTypeInfo
{
    DeclTypeInfoKind kind;
    RType* type;
};

} // namespace Citron