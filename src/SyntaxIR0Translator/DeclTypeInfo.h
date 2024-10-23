#pragma once

#include <memory>
namespace Citron {

using RTypePtr = std::shared_ptr<class RType>;


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