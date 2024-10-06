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
    friend class RTypeFactory;
};

using RDeclPathPtr = std::shared_ptr<RDeclPath>;

class RDeclId
{
    std::string moduleName;
    RDeclPathPtr path;

private:
    RDeclId(std::string&& moduleName, RDeclPathPtr&& path);
    friend class RTypeFactory;
};

using RDeclIdPtr = std::shared_ptr<RDeclId>;

} // namespace Citron
