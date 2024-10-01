#pragma once

#include "IR0Config.h"

#include <vector>
#include <unordered_map>

#include "RIdentifier.h"
#include "RTypeDecl.h"


namespace Citron {

class RTypeDeclContainerComponent
{
    std::vector<RTypeDeclPtr> types;
    std::unordered_map<RIdentifier, RTypeDeclPtr> typeDict;

public:
    IR0_API RTypeDeclContainerComponent();

    IR0_API size_t GetTypeCount();
    IR0_API RTypeDeclPtr GetType(int index);
    IR0_API RTypeDeclPtr GetType(const RIdentifier& identifier);
    IR0_API void AddType(RTypeDeclPtr&& typeDecl);
};

}