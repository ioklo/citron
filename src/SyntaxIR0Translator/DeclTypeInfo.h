#pragma once

#include <memory>

namespace Citron {

class RType;

enum class DeclTypeInfoKind
{
    Normal,
    PlainVar,
    LocalInterfaceVar, // starts with local
    BoxPtrVar,
    LocalPtrVar,
    NullableVar,
};

struct DeclTypeInfo
{
    DeclTypeInfoKind kind;
    RType* type;
};

} // namespace Citron