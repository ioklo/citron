export module Citron.DeclSymbols:TypeDeclSymbolComponent;

import Citron.Names;

import <vector>;
import <unordered_map>;
import <optional>;

import :TypeDeclSymbol;

namespace Citron {

class TypeDeclSymbolComponent
{
    std::vector<TypeDeclSymbol> types;
    std::unordered_map<Name, size_t> typeDict;

public:
    TypeDeclSymbolComponent();

    // public IEnumerable<ITypeDeclSymbol> GetEnumerable()
    TypeDeclSymbol* GetType(const Name& name); // TypeDeclSymbol* for std::optional<TypeDeclSymbol&>
    void AddType(TypeDeclSymbol&& typeDecl);
};

}