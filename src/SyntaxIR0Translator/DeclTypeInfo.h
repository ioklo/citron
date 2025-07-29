#pragma once

#include <memory>

namespace Citron {

class RType;
using RTypePtr = std::shared_ptr<RType>;

namespace SyntaxIR0Translator {

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
    RTypePtr type;
};

} // namespace SyntaxIR0Translator

} // namespace Citron