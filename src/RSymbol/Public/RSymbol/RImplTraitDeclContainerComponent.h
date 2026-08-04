#pragma once
#include <vector>

namespace Citron {

class RImplTraitDecl;

class RImplTraitDeclContainerComponent
{
    std::vector<RImplTraitDecl*> implTraits;

public:
    void AddImplTrait(RImplTraitDecl* decl) { implTraits.push_back(decl); }
};

}
