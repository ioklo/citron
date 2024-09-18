#pragma once

#include "IR0Config.h"

#include <string>
#include <memory>

#include "RIdentifier.h"

namespace Citron {

class RDeclPath
{
    std::shared_ptr<RDeclPath> outer;
    RIdentifier identifier;

private:
    RDeclPath(std::shared_ptr<RDeclPath>&& outer, RIdentifier&& identifier);
    friend class MDeclIdFactory;
};

using RDeclPathPtr = std::shared_ptr<RDeclPath>;

class RDeclId
{
    std::string moduleName;
    RDeclPathPtr path;

private:
    RDeclId(std::string&& moduleName, RDeclPathPtr&& path);
    friend class MDeclIdFactory;
};

using RDeclIdPtr = std::shared_ptr<RDeclId>;

// flyweight
class RDeclIdFactory
{
public:
    IR0_API RDeclIdPtr GetBool();
    IR0_API RDeclIdPtr GetInt();
    IR0_API RDeclIdPtr GetString();

    IR0_API RDeclIdPtr Get(std::string&& moduleName, RIdentifier&& identifier);
    IR0_API RDeclIdPtr GetChild(RDeclIdPtr&& id, RIdentifier&& identifier);
};

} // namespace Citron
