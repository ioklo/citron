#pragma once
#include "SymbolMacros.h"

#include "MNames.h"
#include "MTypeId.h"

namespace Citron {

class MSymbolPath
{
    std::shared_ptr<MSymbolPath> pOuter;
    MName name;
    std::vector<MTypeId> typeArgs;
    std::vector<MTypeId> paramIds;

public:
    MSymbolPath(std::shared_ptr<MSymbolPath>&& pOuter, MName&& name, std::vector<MTypeId>&& typeArgs, std::vector<MTypeId>&& paramIds);
    DECLARE_DEFAULTS(MSymbolPath)
    
    std::shared_ptr<MSymbolPath> Child(std::shared_ptr<MSymbolPath>&& sharedThis, MName&& name, std::vector<MTypeId>&& typeArgs, std::vector<MTypeId>&& paramIds);
};

class MSymbolId
{
    std::string moduleName;
    std::shared_ptr<MSymbolPath> pPath;

public:
    MSymbolId(std::string&& moduleName, std::shared_ptr<MSymbolPath>&& pPath);
    DECLARE_DEFAULTS(MSymbolId)
    
    MSymbolId Child(MName name, std::vector<MTypeId>&& typeArgs, std::vector<MTypeId>&& paramIds);
};

extern MSymbolId boolSymbolId;
extern MSymbolId intSymbolId;
extern MSymbolId stringSymbolId;

}