export module Citron.SyntaxIR0Translator:DeclTypeInfo;

import <memory>;
import Citron.RDecls;

namespace Citron::SyntaxIR0Translator {

export enum class DeclTypeInfoKind
{
    Normal,
    PlainVar,
    LocalInterfaceVar, // starts with local
    BoxPtrVar,
    LocalPtrVar,
    NullableVar,
};

export struct DeclTypeInfo
{
    DeclTypeInfoKind kind;
    RTypePtr type;
};

} // namespace Citron::SyntaxIR0Translator
