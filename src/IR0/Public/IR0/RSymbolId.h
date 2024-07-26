#pragma once

#include <memory>
#include <vector>

#include <Infra/Defaults.h>
#include "RNames.h"

namespace Citron {

class RTypeId;

class RSymbolPath
{
    std::shared_ptr<RSymbolPath> pOuter;
    RName name;
    std::vector<std::shared_ptr<RTypeId>> typeArgs;
    std::vector<std::shared_ptr<RTypeId>> paramIds;

public:
    RSymbolPath(
        std::shared_ptr<RSymbolPath>&& pOuter,
        RName&& name, 
        std::vector<std::shared_ptr<RTypeId>>&& typeArgs, 
        std::vector<std::shared_ptr<RTypeId>>&& paramIds);
    DECLARE_DEFAULTS(IR0_API, RSymbolPath)
    
    std::shared_ptr<RSymbolPath> Child(std::shared_ptr<RSymbolPath>&& sharedThis, RName&& name, std::vector<std::shared_ptr<RTypeId>>&& typeArgs, std::vector<std::shared_ptr<RTypeId>>&& paramIds);
};

class RSymbolId
{
    std::string moduleName;
    std::shared_ptr<RSymbolPath> pPath;

public:
    RSymbolId(std::string&& moduleName, std::shared_ptr<RSymbolPath>&& pPath);
    DECLARE_DEFAULTS(IR0_API, RSymbolId)
    
    RSymbolId Child(RName name, std::vector<std::shared_ptr<RTypeId>>&& typeArgs, std::vector<std::shared_ptr<RTypeId>>&& paramIds);
};

extern RSymbolId boolSymbolId;
extern RSymbolId intSymbolId;
extern RSymbolId stringSymbolId;

}