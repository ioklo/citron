module;
#include "Macros.h"

export module Citron.Identifiers:SymbolId;

import <optional>;
import <vector>;
import <memory>;
import <ranges>;
import <variant>;
import Citron.Names;
import :TypeId;

namespace Citron {

class SymbolPath;

class SymbolPath
{
    std::shared_ptr<SymbolPath> pOuter;
    Name name;
    std::vector<TypeId> typeArgs;
    std::vector<TypeId> paramIds;

public:
    SymbolPath(std::shared_ptr<SymbolPath>&& pOuter, Name&& name, std::vector<TypeId>&& typeArgs, std::vector<TypeId>&& paramIds);
    DECLARE_DEFAULTS(SymbolPath)
    
    std::shared_ptr<SymbolPath> Child(std::shared_ptr<SymbolPath>&& sharedThis, Name&& name, std::vector<TypeId>&& typeArgs, std::vector<TypeId>&& paramIds);
};

class SymbolId
{
    std::string moduleName;
    std::shared_ptr<SymbolPath> pPath;

public:
    SymbolId(std::string&& moduleName, std::shared_ptr<SymbolPath>&& pPath);
    DECLARE_DEFAULTS(SymbolId)
    
    SymbolId Child(Name name, std::vector<TypeId>&& typeArgs, std::vector<TypeId>&& paramIds);
};

export extern SymbolId boolSymbolId;
export extern SymbolId intSymbolId;
export extern SymbolId stringSymbolId;

}