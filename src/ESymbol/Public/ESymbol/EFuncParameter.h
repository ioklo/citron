#pragma once

#include <string>

namespace Citron {

class EType;

class EFuncParameter
{
    bool bOut;
    EType* type;
    std::string name;
};


}
