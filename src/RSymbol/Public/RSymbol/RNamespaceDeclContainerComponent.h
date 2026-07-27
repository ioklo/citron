#pragma once

#include "RSymbolConfig.h"

#include <optional>
#include <unordered_map>
#include "Infra/Ref.h"
#include "RNames.h"

namespace Citron {

class RNamespace;

class RNamespaceDeclContainerComponent
{
public:
    std::vector<RNamespace*> namespaces; // preserve order
    std::unordered_map<RName, RNamespace*> namespaceDict;

public:
    RNamespaceDeclContainerComponent();

    RSYMBOL_API void AddNamespace(RNamespace* _namespace);
    RSYMBOL_API RNamespace* GetNamespace(InRef<RName> name);
};

}


