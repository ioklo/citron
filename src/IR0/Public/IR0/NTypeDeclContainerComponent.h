#pragma once

#include "IR0Config.h"

#include <vector>
#include <unordered_map>
#include <optional>

#include "RIdentifier.h"
#include "NTypeDecl.h"
#include "RMember.h"

namespace Citron {

class NTypeDeclContainerComponent
{
public:
    std::vector<NTypeDeclPtr> types;
    std::unordered_map<RIdentifier, NTypeDeclPtr> typeDict;

public:
    IR0_API NTypeDeclContainerComponent();

    IR0_API size_t GetTypeCount();
    IR0_API NTypeDeclPtr GetType(int index);
    IR0_API NTypeDeclPtr GetType(const RIdentifier& identifier);
    IR0_API void AddType(NTypeDeclPtr&& typeDecl);
    
    // internal
    std::optional<RMember> GetMemberType(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount);
};

}