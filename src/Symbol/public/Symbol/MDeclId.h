#pragma once
#include "SymbolConfig.h"

#include <string>
#include <memory>

#include "MIdentifier.h"

namespace Citron {

class MDeclPath
{
    std::shared_ptr<MDeclPath> outer;
    MIdentifier identifier;

private:
    MDeclPath(std::shared_ptr<MDeclPath>&& outer, MIdentifier&& identifier);
    friend class MDeclIdFactory;
};

using MDeclPathPtr = std::shared_ptr<MDeclPath>;

class MDeclId
{
    std::string moduleName;
    MDeclPathPtr path;

private:
    MDeclId(std::string&& moduleName, MDeclPathPtr&& path);
    friend class MDeclIdFactory;
};

using MDeclIdPtr = std::shared_ptr<MDeclId>;

// flyweight
class MDeclIdFactory
{
public:
    SYMBOL_API MDeclIdPtr GetBool();
    SYMBOL_API MDeclIdPtr GetInt();
    SYMBOL_API MDeclIdPtr GetString();

    SYMBOL_API MDeclIdPtr Get(std::string&& moduleName, MIdentifier&& identifier);
    SYMBOL_API MDeclIdPtr GetChild(MDeclIdPtr&& id, MIdentifier&& identifier);
};

} // namespace Citron
