module;
#include "Macros.h"

export module Citron.Symbols:MSymbolId;

import <optional>;
import <vector>;
import <memory>;
import <ranges>;
import <variant>;
import :MNames;
import :MTypeId;

namespace Citron {

export class MSymbolPath
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

export class MSymbolId
{
    std::string moduleName;
    std::shared_ptr<MSymbolPath> pPath;

public:
    MSymbolId(std::string&& moduleName, std::shared_ptr<MSymbolPath>&& pPath);
    DECLARE_DEFAULTS(MSymbolId)
    
    MSymbolId Child(MName name, std::vector<MTypeId>&& typeArgs, std::vector<MTypeId>&& paramIds);
};

export extern MSymbolId boolSymbolId;
export extern MSymbolId intSymbolId;
export extern MSymbolId stringSymbolId;

}