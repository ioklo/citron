#pragma once
#include <memory>
#include <vector>

#include <Infra/Defaults.h>

#include "MNames.h"

namespace Citron {

class MTypeId;

class MSymbolPath
{
    std::shared_ptr<MSymbolPath> pOuter;
    MName name;
    std::vector<std::shared_ptr<MTypeId>> typeArgs;
    std::vector<std::shared_ptr<MTypeId>> paramIds;

public:
    MSymbolPath(
        std::shared_ptr<MSymbolPath>&& pOuter,
        MName&& name, 
        std::vector<std::shared_ptr<MTypeId>>&& typeArgs, 
        std::vector<std::shared_ptr<MTypeId>>&& paramIds);
    DECLARE_DEFAULTS(SYMBOL_API, MSymbolPath)
    
    std::shared_ptr<MSymbolPath> Child(std::shared_ptr<MSymbolPath>&& sharedThis, MName&& name, std::vector<std::shared_ptr<MTypeId>>&& typeArgs, std::vector<std::shared_ptr<MTypeId>>&& paramIds);
};

class MSymbolId
{
    std::string moduleName;
    std::shared_ptr<MSymbolPath> pPath;

public:
    MSymbolId(std::string&& moduleName, std::shared_ptr<MSymbolPath>&& pPath);
    DECLARE_DEFAULTS(SYMBOL_API, MSymbolId)
    
    MSymbolId Child(MName name, std::vector<std::shared_ptr<MTypeId>>&& typeArgs, std::vector<std::shared_ptr<MTypeId>>&& paramIds);
};

extern MSymbolId boolSymbolId;
extern MSymbolId intSymbolId;
extern MSymbolId stringSymbolId;

}