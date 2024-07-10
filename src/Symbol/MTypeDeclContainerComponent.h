#pragma once

#include <vector>
#include <unordered_map>

#include "MNames.h"
#include "MTypeDecl.h"


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