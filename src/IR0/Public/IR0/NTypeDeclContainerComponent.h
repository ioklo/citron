#pragma once

#include "IR0Config.h"

#include <vector>
#include <unordered_map>

#include "RIdentifier.h"
#include "NTypeDecl.h"


namespace Citron {

class NTypeDeclContainerComponent
{
    std::vector<NTypeDeclPtr> types;
    std::unordered_map<RIdentifier, NTypeDeclPtr> typeDict;

public:
    IR0_API NTypeDeclContainerComponent();

    IR0_API size_t GetTypeCount();
    IR0_API NTypeDeclPtr GetType(int index);
    IR0_API NTypeDeclPtr GetType(const RIdentifier& identifier);
    IR0_API void AddType(NTypeDeclPtr&& typeDecl);
};

}