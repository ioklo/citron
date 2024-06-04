export module Citron.DeclSymbols:TypeDeclSymbol;

import <variant>;

namespace Citron {

class ClassDeclSymbol;
class StructDeclSymbol;
class EnumDeclSymbol;
class EnumElemDeclSymbol;
class InterfaceDeclSymbol;
class LambdaDeclSymbol;

export using TypeDeclSymbol = std::variant<
    ClassDeclSymbol,
    StructDeclSymbol,
    EnumDeclSymbol,
    EnumElemDeclSymbol,
    InterfaceDeclSymbol,
    LambdaDeclSymbol
>;

}