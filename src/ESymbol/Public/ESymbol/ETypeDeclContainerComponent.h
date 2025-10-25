#pragma once

#include <vector>
#include <unordered_map>
#include <variant>


#include "ETypeDecl.h"
#include "ENames.h"

namespace Citron {

class ETypeDeclContainerComponent
{
    std::vector<ETypeDecl> types;
    std::unordered_map<EName, size_t> typeDict;

public:
    ETypeDeclContainerComponent();

    // public IEnumerable<ITypeDeclSymbol> GetEnumerable()
    ETypeDecl* GetType(const EName& name); // ETypeDecl* for std::optional<TypeDeclSymbol&>
    void AddType(ETypeDecl&& typeDecl);
};

}