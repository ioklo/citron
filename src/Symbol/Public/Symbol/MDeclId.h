#pragma once
#include "SymbolConfig.h"

#include <string>

#include "MIdentifier.h"

namespace Citron {

class MDeclId;

class MDeclIdFactory;

class MDeclPath
{
    MDeclPath* outer;
    MIdentifier identifier;

private:
    MDeclPath(MDeclPath* outer, MIdentifier&& identifier);
    friend MDeclIdFactory;
};

class MDeclId
{
    std::string moduleName;
    MDeclPath* path;

private:
    MDeclId(std::string&& moduleName, MDeclPath*&& path);
    friend MDeclIdFactory;
};

// flyweight
class MDeclIdFactory
{
public:
    SYMBOL_API MDeclId* GetBool();
    SYMBOL_API MDeclId* GetInt();
    SYMBOL_API MDeclId* GetString();

    SYMBOL_API MDeclId* Get(std::string&& moduleName, MIdentifier&& identifier);
    SYMBOL_API MDeclId* GetChild(MDeclId* id, MIdentifier&& identifier);
};

} // namespace Citron
