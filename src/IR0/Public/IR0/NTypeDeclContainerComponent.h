#pragma once

#include "IR0Config.h"

#include <vector>
#include <unordered_map>
#include <optional>

#include "RIdentifier.h"
#include "NTypeDecl.h"

namespace Citron {

class NTypeDeclContainerComponent
{
public:
    std::vector<NTypeDecl*> types;
    std::unordered_map<RIdentifier, NTypeDecl*> typeDict;

public:
    IR0_API NTypeDeclContainerComponent();

    IR0_API size_t GetTypeCount();
    IR0_API NTypeDecl* GetType(int index);
    IR0_API NTypeDecl* GetType(const RIdentifier& identifier);
    IR0_API void AddType(NTypeDecl* typeDecl);

    // internal
    std::optional<RMember> GetMemberType(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount);
};

}