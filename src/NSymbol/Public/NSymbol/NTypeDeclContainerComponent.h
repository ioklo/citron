#pragma once

#include "NSymbolConfig.h"

#include <vector>
#include <unordered_map>
#include <optional>

#include "RSymbol/RIdentifier.h"
#include "NTypeDecl.h"

namespace Citron {

class NTypeDeclContainerComponent
{
public:
    std::vector<NTypeDecl*> types;
    std::unordered_map<RIdentifier, NTypeDecl*> typeDict;

public:
    NSYMBOL_API NTypeDeclContainerComponent();

    NSYMBOL_API size_t GetTypeCount();
    NSYMBOL_API NTypeDecl* GetType(int index);
    NSYMBOL_API NTypeDecl* GetType(const RIdentifier& identifier);
    NSYMBOL_API void AddType(NTypeDecl* typeDecl);

    RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount);
    std::optional<RDeclRes> GetMemberType(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount);
};

}