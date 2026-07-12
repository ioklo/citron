#pragma once

#include "RSymbolConfig.h"

#include <optional>
#include <unordered_map>
#include "Infra/Ref.h"
#include "RNames.h"
#include "RDeclRes.h"

namespace Citron {

class RNamespaceDecl;

class RNamespaceDeclContainerComponent
{
public:
    std::vector<RNamespaceDecl*> namespaceDecls; // preserve order
    std::unordered_map<RName, RNamespaceDecl*> namespaceDict;

public:
    RNamespaceDeclContainerComponent();

    RSYMBOL_API void AddNamespace(RNamespaceDecl* _namespace);
    RSYMBOL_API RNamespaceDecl* GetNamespace(InRef<RName> name);

    // internal
    std::optional<RDeclRes> ResolveNamespaceMember(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount);
};

}


