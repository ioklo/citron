#pragma once

#include "RSymbolConfig.h"

#include <vector>
#include <unordered_map>
#include <optional>
#include "Infra/Ref.h"
#include "RIdentifier.h"
#include "RTypeDecl.h"
#include "RDeclRes.h"

namespace Citron {

class RTypeDeclContainerComponent
{
public:
    std::vector<RTypeDecl*> types;
    std::unordered_map<RIdentifier, RTypeDecl*> typeDict;

public:
    RSYMBOL_API RTypeDeclContainerComponent();

    RSYMBOL_API size_t GetTypeCount();
    RSYMBOL_API RTypeDecl* GetType(int index);
    RSYMBOL_API RTypeDecl* GetType(const RIdentifier& identifier);
    RSYMBOL_API void AddType(RTypeDecl* typeDecl);

    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name);
    RSYMBOL_API std::optional<RDeclRes> ResolveTypeMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount);
};

}