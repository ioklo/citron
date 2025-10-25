#pragma once
#include "ESymbolConfig.h"

#include <string>

#include "EIdentifier.h"

namespace Citron {

class EDeclId;

class EDeclIdFactory;

class EDeclPath
{
    EDeclPath* outer;
    EIdentifier identifier;

private:
    EDeclPath(EDeclPath* outer, EIdentifier&& identifier);
    friend EDeclIdFactory;
};

class EDeclId
{
    std::string moduleName;
    EDeclPath* path;

private:
    EDeclId(std::string&& moduleName, EDeclPath*&& path);
    friend EDeclIdFactory;
};

// flyweight
class EDeclIdFactory
{
public:
    ESYMBOL_API EDeclId* GetBool();
    ESYMBOL_API EDeclId* GetInt();
    ESYMBOL_API EDeclId* GetString();

    ESYMBOL_API EDeclId* Get(std::string&& moduleName, EIdentifier&& identifier);
    ESYMBOL_API EDeclId* GetChild(EDeclId* id, EIdentifier&& identifier);
};

} // namespace Citron
