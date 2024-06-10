export module Citron.Symbols:MTypeDeclContainerComponent;

import :MNames;

import <vector>;
import <unordered_map>;
import <optional>;

import :MTypeDecl;

namespace Citron {

class MTypeDeclContainerComponent
{
    std::vector<MTypeDecl> types;
    std::unordered_map<MName, size_t> typeDict;

public:
    MTypeDeclContainerComponent();

    // public IEnumerable<ITypeDeclSymbol> GetEnumerable()
    MTypeDecl* GetType(const MName& name); // MTypeDecl* for std::optional<TypeDeclSymbol&>
    void AddType(MTypeDecl&& typeDecl);
};

}