module;
#include <vector>
#include <unordered_map>
#include <variant>

export module Citron.MDecls:MTypeDeclContainerComponent;

import :MTypeDecl;
import :MNames;

namespace Citron {

export class MTypeDeclContainerComponent
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