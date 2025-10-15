#pragma once
#include "IR0Config.h"

#include <string>

#include "RModule.h"
#include "NDecl.h"

namespace Citron {

class NNamespaceDecl;

class NModule : public RModule
{
public:
    std::string name;

public:
    IR0_API NModule(std::string&& name);
};

}