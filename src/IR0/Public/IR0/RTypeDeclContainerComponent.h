#pragma once

#include <vector>
#include <unordered_map>

#include "RNames.h"
#include "RTypeDecl.h"


namespace Citron {

class RTypeDeclContainerComponent
{
    std::vector<RTypeDecl> types;
    std::unordered_map<RName, size_t> typeDict;

public:
    RTypeDeclContainerComponent();

    // public IEnumerable<ITypeDeclSymbol> GetEnumerable()
    RTypeDecl* GetType(const RName& name); // RTypeDecl* for std::optional<TypeDeclSymbol&>
    void AddType(RTypeDecl&& typeDecl);
};

}